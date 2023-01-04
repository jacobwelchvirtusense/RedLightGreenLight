/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/29/2022 5:15:42 PM
 * 
 * Description: TODO
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Floor : MonoBehaviour
{
    public static float X { get; internal set; }
    public static float Y { get; internal set; }
    public static float Z { get; internal set; }
    public static float W { get; internal set; }

    public Floor(Windows.Kinect.Vector4 floorClipPlane)
    {
        X = floorClipPlane.X;
        Y = floorClipPlane.Y;
        Z = floorClipPlane.Z;
        W = floorClipPlane.W;
    }
    public static float Height
    {
        get { return W; }
    }
    public static double Tilt
    {
        get { return Math.Atan(Z / Y) * (180.0 / Math.PI); }
    }
    public static double DistanceFrom(CameraSpacePoint point)
    {
        /*
        print("X: " + point.X);
        print("Y: " + point.Y);
        print("Z: " + point.Z);*/

        double numerator = X * point.X + Y * point.Y + Z * point.Z + W;
        double denominator = Math.Sqrt(X * X + Y * Y + Z * Z);
        return numerator / denominator;
    }
}
