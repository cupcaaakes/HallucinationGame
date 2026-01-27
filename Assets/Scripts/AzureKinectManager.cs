using System;
using System.Threading;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using TMPro;
using System.Diagnostics;


public class KinectManager : MonoBehaviour
{
    private Device kinect;
    private Tracker tracker;

    private float lastBodyLogTime;
    private bool hadBodyLastFrame;

    private Skeleton latestSkeleton;
    private bool hasSkeleton;

    private Thread worker;
    private volatile bool running;
    private readonly object skelLock = new object();

    [Header("Multi-person / Exhibit filtering")]
    [SerializeField] private bool useInteractionZone = true;

    [SerializeField] private float minDistanceM = 0.8f;   // Z min
    [SerializeField] private float maxDistanceM = 3.0f;   // Z max

    [SerializeField] private bool gateByCenterLane = true;
    [SerializeField] private float centerLaneHalfWidthM = 0.7f; // |X| <= this

    [SerializeField] private float switchHysteresisM = 0.4f; // new person must be this much "better"
    [SerializeField] private float lostReacquireSeconds = 0.7f;

    private uint _trackedBodyId;
    private bool _hasTrackedBodyId;
    private double _lastSeenSeconds;

    private Stopwatch _sw;


    [SerializeField] private TMP_Text debugText;

    void Start()
    {
        UnityEngine.Debug.Log("INIT KINECT");
        _sw = Stopwatch.StartNew();

        try
        {
            int count = Device.GetInstalledCount();
            UnityEngine.Debug.Log($"Devices installed: {count}");

            kinect = Device.Open(0);
            UnityEngine.Debug.Log($"Opened device: {kinect.SerialNum}");

            var config = new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorMJPG,
                ColorResolution = ColorResolution.Off,
                DepthMode = DepthMode.NFOV_Unbinned,
                CameraFPS = FPS.FPS30,
                SynchronizedImagesOnly = false,
                WiredSyncMode = WiredSyncMode.Standalone
            };

            UnityEngine.Debug.Log("Starting cameras...");
            kinect.StartCameras(config);
            UnityEngine.Debug.Log("Cameras started successfully!");

            tracker = Tracker.Create(
                kinect.GetCalibration(),
                new TrackerConfiguration
                {
                    ProcessingMode = TrackerProcessingMode.Cpu,
                    SensorOrientation = SensorOrientation.Default
                });

            UnityEngine.Debug.Log("Tracker created successfully.");

            running = true;
            worker = new Thread(TrackingLoop) { IsBackground = true };
            worker.Start();
        }
        catch (Exception ex)
        {
            debugText.text = ex.ToString();
            UnityEngine.Debug.LogError("ERROR initializing Kinect: " + ex.Message);
            //Debug.LogException(ex);
        }
    }

    private void TrackingLoop()
    {
        while (running)
        {
            if (kinect == null || tracker == null)
            {
                Thread.Sleep(5);
                continue;
            }

            try
            {
                // Blocking here is fine: it's NOT Unity's main thread.
                using (var capture = kinect.GetCapture(TimeSpan.FromMilliseconds(1000)))
                {
                    tracker.EnqueueCapture(capture);
                }

                // Drain results and keep the newest (prevents backlog lag)
                Frame newest = null;
                while (true)
                {
                    Frame f = null;
                    try { f = tracker.PopResult(TimeSpan.Zero); }
                    catch (TimeoutException) { break; }

                    if (f == null) break;

                    newest?.Dispose();
                    newest = f;
                }

                if (newest == null)
                    continue;

                using (newest)
                {
                    uint bodies = newest.NumberOfBodies;
                    bool hasBody = bodies > 0;

                    if (!hasBody)
                    {
                        lock (skelLock) hasSkeleton = false;
                        continue;
                    }

                    if (!TrySelectBody(newest, out var skel))
                    {
                        lock (skelLock) hasSkeleton = false;
                        continue;
                    }


                    lock (skelLock)
                    {
                        latestSkeleton = skel;
                        hasSkeleton = true;
                    }
                }
            }
            catch (TimeoutException)
            {
                // ignore
            }
            catch
            {
                // swallow to keep thread alive
            }
        }
    }

    public bool TryGetLatestSkeleton(out Skeleton skel)
    {
        lock (skelLock)
        {
            skel = latestSkeleton;
            return hasSkeleton;
        }
    }

    private bool TrySelectBody(Frame frame, out Skeleton selected)
    {
        selected = default;

        int n = (int)frame.NumberOfBodies;
        if (n <= 0) return false;

        double now = _sw != null ? _sw.Elapsed.TotalSeconds : 0.0;

        bool trackedFoundAndValid = false;
        float trackedZ = float.PositiveInfinity;
        float trackedScore = float.PositiveInfinity;
        Skeleton trackedSkel = default;

        bool bestFound = false;
        uint bestId = 0;
        float bestScore = float.PositiveInfinity;
        float bestZ = float.PositiveInfinity;
        Skeleton bestSkel = default;

        for (uint i = 0; i < n; i++)
        {
            var body = frame.GetBody(i);
            var skel = body.Skeleton;

            var pelvis = skel.GetJoint(JointId.Pelvis);
            if (pelvis.ConfidenceLevel == JointConfidenceLevel.None)
                continue;

            // Kinect space (mm -> m)
            float x = pelvis.Position.X * 0.001f;
            float z = pelvis.Position.Z * 0.001f;

            if (useInteractionZone)
            {
                if (z < minDistanceM || z > maxDistanceM)
                    continue;

                if (gateByCenterLane && Mathf.Abs(x) > centerLaneHalfWidthM)
                    continue;
            }

            // Score: prefer centered, then closer
            // (tune weight if needed; higher weight = center matters more)
            const float centerWeight = 0.6f;
            float score = z + Mathf.Abs(x) * centerWeight;

            if (_hasTrackedBodyId && body.Id == _trackedBodyId)
            {
                trackedFoundAndValid = true;
                trackedZ = z;
                trackedScore = score;
                trackedSkel = skel;
            }

            if (!bestFound || score < bestScore)
            {
                bestFound = true;
                bestScore = score;
                bestZ = z;
                bestId = body.Id;
                bestSkel = skel;
            }
        }

        // If our currently tracked body is still valid, keep it unless someone is MUCH better.
        if (trackedFoundAndValid)
        {
            _lastSeenSeconds = now;

            if (bestFound)
            {
                // Compare using distance hysteresis primarily (simple + intuitive)
                // Only switch if the new best is at least switchHysteresisM closer in Z
                bool bestIsMuchCloser = (trackedZ - bestZ) > switchHysteresisM;

                if (!bestIsMuchCloser)
                {
                    selected = trackedSkel;
                    return true;
                }
            }

            // Switch (only if best exists)
            if (bestFound)
            {
                _trackedBodyId = bestId;
                _hasTrackedBodyId = true;
                selected = bestSkel;
                return true;
            }

            selected = trackedSkel;
            return true;
        }

        // No valid tracked body found. Only reacquire after a short "lost" delay
        bool canReacquire = (now - _lastSeenSeconds) >= lostReacquireSeconds;

        if (bestFound && (canReacquire || !_hasTrackedBodyId))
        {
            _trackedBodyId = bestId;
            _hasTrackedBodyId = true;
            _lastSeenSeconds = now;
            selected = bestSkel;
            return true;
        }

        return false;
    }


    void OnDestroy()
    {
        running = false;
        if (worker != null && worker.IsAlive) worker.Join(200);

        try
        {
            tracker?.Dispose();
            kinect?.StopCameras();
            kinect?.Dispose();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("ERROR: Exception during shutdown: " + ex.Message);
        }
    }
}
