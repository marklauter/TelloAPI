﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tello.Messaging;

namespace Tello.Emulator.SDKV2
{
    internal sealed class StateManager
    {
        public StateManager(DroneState droneState, VideoServer videoServer, StateServer stateServer)
        {
            _droneState = droneState ?? throw new ArgumentNullException(nameof(droneState));
            _videoServer = videoServer ?? throw new ArgumentNullException(nameof(videoServer));
            _stateServer = stateServer ?? throw new ArgumentNullException(nameof(stateServer));
            DischargeBattery();
        }

        //private readonly Gate _gate = new Gate();
        private readonly DroneState _droneState;
        private readonly VideoServer _videoServer;
        private readonly StateServer _stateServer;
        private readonly Position _position = new Position();
        private readonly Stopwatch _batteryClock = new Stopwatch();

        public bool IsPoweredUp { get; private set; } = false;
        public bool IsVideoOn { get; private set; } = false;
        public bool IsSdkModeActivated { get; private set; } = false;
        public int Speed { get; private set; } = 10;
        public Position Position => new Position(_position);
        public enum FlightStates
        {
            StandingBy,
            Takingoff,
            InFlight,
            Landing,
            EmergencyStop
        }
        public FlightStates FlightState { get; private set; } = FlightStates.StandingBy;

        public void RechargeBattery()
        {
            _droneState.BatteryPercent = 100;
        }

        public void PowerOn()
        {
            if (!IsPoweredUp)
            {
                IsPoweredUp = true;
                _batteryClock.Start();
                _stateServer.Start();
                FlightState = FlightStates.StandingBy;
                IsSdkModeActivated = false;
                Speed = 10;
            }
        }

        public void PowerOff()
        {
            if (IsPoweredUp)
            {
                IsPoweredUp = false;
                _stateServer.Stop();
                _batteryClock.Stop();
                StopVideo();
                FlightState = FlightStates.StandingBy;
                IsSdkModeActivated = false;
                Speed = 10;
            }
        }

        public void EnterSdkMode()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (!IsSdkModeActivated)
            {
                IsSdkModeActivated = true;
            }
        }

        public void TakeOff()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (IsSdkModeActivated && FlightState == FlightStates.StandingBy)
            {
                _droneState.MotorClock.Start();
                FlightState = FlightStates.Takingoff;
                _position.Z = _droneState.HeightInCm = 20;
                _droneState.BarometerInCm += _droneState.HeightInCm;
                FlightState = FlightStates.InFlight;
            }
        }

        public void Land()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (IsSdkModeActivated && (FlightState == FlightStates.InFlight || FlightState == FlightStates.Takingoff))
            {
                FlightState = FlightStates.Landing;
                _droneState.MotorClock.Stop();
                _droneState.BarometerInCm -= _droneState.HeightInCm;
                _position.Z = _droneState.HeightInCm = 0;
                FlightState = FlightStates.StandingBy;
            }
        }

        public void EmergencyStop()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (IsSdkModeActivated && FlightState != FlightStates.StandingBy)
            {
                FlightState = FlightStates.EmergencyStop;
                _droneState.MotorClock.Stop();
                _droneState.HeightInCm = 0;
                _position.Z = _droneState.HeightInCm;
            }
        }

        public void SetSpeed(int speed)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (speed < 10 || speed > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            Speed = speed;
        }

        //todo: set an approximate acceleration in the movement commands before the delay and reset to zero after
        //todo: take heading into account - see Tello.Controller.Position.Move() for help
        public void GoForward(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _position.Y += cm;
        }

        public void GoBack(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _position.Y -= cm;
        }

        public void GoRight(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _position.X += cm;
        }

        public void GoLeft(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _position.X -= cm;
        }

        public void GoUp(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _droneState.BarometerInCm += cm;
            _position.Z = _droneState.HeightInCm += cm;
        }

        public void GoDown(int cm)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (cm < 20 || cm > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(cm));
            }

            _droneState.BarometerInCm -= cm;
            _droneState.HeightInCm -= cm;
            if (_droneState.HeightInCm < 0)
            {
                _droneState.HeightInCm = 0;
            }
            _position.Z = _droneState.HeightInCm;
        }

        public void TurnClockwise(int degrees)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (degrees < 1 || degrees > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(degrees));
            }

            _position.Heading += degrees;
            if (_position.Heading >= 360)
            {
                _position.Heading -= 360;
            }
        }

        public void TurnCounterClockwise(int degrees)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (degrees < 1 || degrees > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(degrees));
            }

            _position.Heading -= degrees;
            if (_position.Heading < 0)
            {
                _position.Heading += 360;
            }
        }

        public void Go(int x, int y, int z, int speed)
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (x < -500 || x > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (y < -500 || y > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            if (z < -500 || z > 500)
            {
                throw new ArgumentOutOfRangeException(nameof(z));
            }
            if (speed < 10 || speed > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            var distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            _position.X += x;
            _position.Y += y;
            _droneState.BarometerInCm += z;
            _droneState.HeightInCm += z;
            if (_droneState.HeightInCm < 0)
            {
                _droneState.HeightInCm = 0;
            }
            _position.Z = _droneState.HeightInCm;
        }

        public void StartVideo()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (!IsVideoOn)
            {
                IsVideoOn = true;
                _videoServer.Start();
            }
        }

        public void StopVideo()
        {
            if (!IsPoweredUp)
            {
                return;
            }

            if (IsVideoOn)
            {
                IsVideoOn = false;
                _videoServer.Stop();
            }
        }

        public int GetSpeed()
        {
            return IsPoweredUp
                ? Speed
                : -1;
        }

        public int GetBattery()
        {
            return IsPoweredUp
                ? _droneState.BatteryPercent
                : -1;
        }

        public int GetTime()
        {
            return IsPoweredUp
                ? _droneState.MotorTimeInSeconds
                : -1;
        }

        private async void DischargeBattery()
        {
            // documentation says there's ~ 15 minutes of battery
            await Task.Run(() =>
            {
                var wait = new SpinWait();
                while (true)
                {
                    if (IsPoweredUp)
                    {
                        _droneState.BatteryPercent = 100 - (int)(_batteryClock.Elapsed.TotalMinutes / 15.0 * 100);

                        if (_droneState.BatteryPercent < 1)
                        {
                            _droneState.BatteryPercent = 0;
                            PowerOff();
                            Log.WriteLine("battery died");
                        }
                    }
                    wait.SpinOnce();
                }
            });
        }
    }
}