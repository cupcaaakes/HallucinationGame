using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;

public class AzureKinectManager : MonoBehaviour
{
    private Device kinect;
    private Tracker tracker;

    void Start()
    {
        Debug.Log("Kinect initialisieren...");

        kinect = Device.Open(0);
        kinect.StartCameras(new DeviceConfiguration()
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_Unbinned,
            SynchronizedImagesOnly = true
        });

        tracker = Tracker.Create(kinect.GetCalibration(), new TrackerConfiguration()
        {
            ProcessingMode = TrackerProcessingMode.Gpu
        });

        Debug.Log("Kinect + BodyTracking läuft!");
    }

    void Update()
    {
        if (tracker != null && kinect != null)
        {
            var capture = kinect.GetCapture();

         
            tracker.EnqueueCapture(capture);

            var frame = tracker.PopResult();

            if (frame != null && frame.NumberOfBodies > 0)
            {
                Debug.Log("Bodies detected: " + frame.NumberOfBodies);
            }
        }
    }


    private void OnApplicationQuit()
    {
        tracker?.Dispose();
        kinect?.Dispose();
    }
}

