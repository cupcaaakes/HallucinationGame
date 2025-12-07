using System;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;

public class KinectManager : MonoBehaviour
{


    public KinectAvatarMapper avatar;
    private Device kinect;
    private Tracker tracker;

    private float lastBodyLogTime;
    private bool hadBodyLastFrame;

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
                ColorResolution = ColorResolution.Off,   // depth only
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
        }
        catch (Exception ex)
        {
            Debug.LogError("ERROR initializing Kinect: " + ex.Message);
            Debug.LogException(ex);
        }
    }

    void Update()
    {
        if (kinect == null || tracker == null)
            return;

        try
        {
            // Get a capture for body tracking (may time out if nothing new)
            try
            {
                using (var capture = kinect.GetCapture(TimeSpan.FromMilliseconds(50)))
                {
                    tracker.EnqueueCapture(capture);
                }
            }
            catch (TimeoutException)
            {
                // no new capture; just skip this frame
                return;
            }

            // Try to get a body frame
            try
            {
                using (var frame = tracker.PopResult(TimeSpan.Zero))
                {
                    if (frame == null)
                        return;

                    uint bodies = frame.NumberOfBodies;
                    bool hasBody = bodies > 0;

                    if (!hasBody)
                        return;  

                    var body = frame.GetBody(0);
                    var skel = body.Skeleton;

                    // Head
                    avatar.ApplyJointRotation(avatar.head, skel.GetJoint(JointId.Head));

                    // Left arm
                    avatar.ApplyJointRotation(avatar.shoulderLeft, skel.GetJoint(JointId.ShoulderLeft));
                    avatar.ApplyJointRotation(avatar.elbowLeft, skel.GetJoint(JointId.ElbowLeft));
                    avatar.ApplyJointRotation(avatar.wristLeft, skel.GetJoint(JointId.WristLeft));

                    // Right arm
                    avatar.ApplyJointRotation(avatar.shoulderRight, skel.GetJoint(JointId.ShoulderRight));
                    avatar.ApplyJointRotation(avatar.elbowRight, skel.GetJoint(JointId.ElbowRight));
                    avatar.ApplyJointRotation(avatar.wristRight, skel.GetJoint(JointId.WristRight));

                    // Logging
                    if (hasBody && Time.time - lastBodyLogTime > 1f)
                    {
                        lastBodyLogTime = Time.time;
                        Debug.Log($"[Kinect] Bodies detected: {bodies}");
                    }

                    if (!hasBody && hadBodyLastFrame)
                    {
                        Debug.Log("[Kinect] Lost all bodies.");
                    }

                    hadBodyLastFrame = hasBody;
                }
            }
            catch (TimeoutException)
            {
                // nothing happens if we don't get a body frame
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ERROR: Exception in Update: " + ex.Message);
            Debug.LogException(ex);
        }
    }

    void OnApplicationQuit()
    {
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
