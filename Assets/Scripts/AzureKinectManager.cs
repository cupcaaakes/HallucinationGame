using System;
using System.Threading;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;

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

    void Start()
    {
        Debug.Log("INIT KINECT");

        try
        {
            int count = Device.GetInstalledCount();
            Debug.Log($"Devices installed: {count}");

            kinect = Device.Open(0);
            Debug.Log($"Opened device: {kinect.SerialNum}");

            var config = new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorMJPG,
                ColorResolution = ColorResolution.Off,
                DepthMode = DepthMode.NFOV_Unbinned,
                CameraFPS = FPS.FPS30,
                SynchronizedImagesOnly = false,
                WiredSyncMode = WiredSyncMode.Standalone
            };

            Debug.Log("Starting cameras...");
            kinect.StartCameras(config);
            Debug.Log("Cameras started successfully!");

            tracker = Tracker.Create(
                kinect.GetCalibration(),
                new TrackerConfiguration
                {
                    ProcessingMode = TrackerProcessingMode.Cpu,
                    SensorOrientation = SensorOrientation.Default
                });

            Debug.Log("Tracker created successfully.");

            running = true;
            worker = new Thread(TrackingLoop) { IsBackground = true };
            worker.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError("ERROR initializing Kinect: " + ex.Message);
            Debug.LogException(ex);
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

                    var skel = newest.GetBody(0).Skeleton;

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
            Debug.LogError("ERROR: Exception during shutdown: " + ex.Message);
        }
    }
}
