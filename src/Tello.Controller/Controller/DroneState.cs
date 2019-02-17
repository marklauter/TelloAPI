﻿using System;
using System.Collections.Generic;
using System.Text;
using Tello.Messaging;

namespace Tello.Controller
{
    /// <summary>
    /// all measurements are in cm
    /// </summary>
    public class DroneState
    {
        public DroneState(IRawDroneState droneState)
        {
            if (droneState.MissionPadDetected)
            {

            }
            else
            {

            }


        }


        int SpeedX { get; }
        int SpeedY { get; }
        int VSI { get; }
    }

    public class Attitude
    {
        public int Pitch { get; }
        public int Roll { get; }
        public int Yaw { get; }
    }

}


//public interface IRawDroneState
//{
//    bool MissionPadDetected { get; }
//    int MissionPadId { get; }
//    int MissionPadX { get; }
//    int MissionPadY { get; }
//    int MissionPadZ { get; }

//    int MissionPadPitch { get; }
//    int MissionPadRoll { get; }
//    int MissionPadYaw { get; }
//    int Pitch { get; }
//    int Roll { get; }
//    int Yaw { get; }

//    int SpeedX { get; }
//    int SpeedY { get; }
//    int SpeedZ { get; }

//    int TemperatureLowC { get; }
//    int TemperatureHighC { get; }

//    int DistanceTraversedInCm { get; }
//    int HeightInCm { get; }
//    int BatteryPercent { get; }
//    int MotorTimeInSeconds { get; }
//    double BarometerInCm { get; }
//    double AccelerationX { get; }
//    double AccelerationY { get; }
//    double AccelerationZ { get; }
//}