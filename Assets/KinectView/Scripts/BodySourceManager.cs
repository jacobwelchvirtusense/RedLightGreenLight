using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Unity.VisualScripting;

public class BodySourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public static Windows.Kinect.Vector4 floor = new Windows.Kinect.Vector4();

    public Body[] GetData()
    {
        return _Data;
    }


    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                floor = frame.FloorClipPlane;

                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;
            }
        }
    }

    /// <summary>
    /// Calculates the distance between the specified joint and the floor.
    /// </summary>
    /// <param name="point">The point to measure the distance from.</param>
    /// <returns>The distance between the floor and the point (in meters).</returns>
    public static float DistanceFrom(CameraSpacePoint point)
    {
        float numerator = floor.X * point.X + floor.Y * point.Y + floor.Z * point.Z + floor.W;
        float denominator = Mathf.Sqrt(floor.X * floor.X + floor.Y * floor.Y + floor.Z * floor.Z);

        return numerator / denominator;
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
