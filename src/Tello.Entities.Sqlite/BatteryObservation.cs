﻿using System;
using Tello.State;

namespace Tello.Entities.Sqlite
{
    public sealed class BatteryObservation : Observation, IBattery
    {
        public BatteryObservation() : base() { }

        public BatteryObservation(
            IObservationGroup group,
            ITelloState state)
            : this(
                  (group ?? throw new ArgumentNullException(nameof(group))).Id,
                  state.Timestamp,
                  state.Battery)
        { }

        public BatteryObservation(
            int groupId,
            ITelloState state)
            : this(
                  groupId,
                  state.Timestamp,
                  state.Battery)
        { }

        private BatteryObservation(
            int groupId,
            DateTime timestamp,
            IBattery battery)
            : base(
                  groupId,
                  timestamp)
        {
            if (battery == null)
            {
                throw new ArgumentNullException(nameof(battery));
            }

            TemperatureLowC = battery.TemperatureLowC;
            TemperatureHighC = battery.TemperatureHighC;
            PercentRemaining = battery.PercentRemaining;
        }

        public int TemperatureLowC { get; set; }

        public int TemperatureHighC { get; set; }

        public int PercentRemaining { get; set; }
    }
}