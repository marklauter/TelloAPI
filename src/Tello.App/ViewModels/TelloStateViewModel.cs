﻿using System;
using System.Collections.ObjectModel;
using Tello.App.MvvM;
using Tello.Controller;
using Tello.Messaging;
using Tello.Repository;

namespace Tello.App.ViewModels
{
    //https://www.actiprosoftware.com/products/controls/wpf/gauge
    //https://docs.microsoft.com/en-us/windows/communitytoolkit/controls/radialgauge
    public class TelloStateViewModel : ViewModel
    {
        private readonly IStateChangedNotifier _telloStateChangedNotifier;
        private readonly ITrackingSession _repository;

        public TelloStateViewModel(IUIDispatcher dispatcher, IUINotifier userNotifier, IStateChangedNotifier telloStateChangedNotifier, ITrackingSession repository) : base(dispatcher, userNotifier)
        {
            _telloStateChangedNotifier = telloStateChangedNotifier ?? throw new ArgumentNullException(nameof(telloStateChangedNotifier));
            _repository = repository;
        }

        protected override void OnOpen(OpenEventArgs args)
        {
            _telloStateChangedNotifier.StateChanged += OnTelloStateChanged;
        }

        protected override void OnClosing(ClosingEventArgs args)
        {
            _telloStateChangedNotifier.StateChanged -= OnTelloStateChanged;
        }

        private void OnTelloStateChanged(object sender, StateChangedArgs e)
        {
            Dispatcher.Invoke((state) =>
            {
                StateHistory.Add(state as ITelloState);
                if(StateHistory.Count > 500)
                {
                    StateHistory.RemoveAt(0);
                }
            }, 
            State);

            State = e.State;

            if (_repository != null)
            {
                var groupId = Guid.NewGuid().ToString();
                _repository.Write(new TelloStateObservation(e.State, groupId));
                _repository.Write(new PositionObservation(e.State, groupId));
                _repository.Write(new AttitudeObservation(e.State, groupId));
                _repository.Write(new AirSpeedObservation(e.State, groupId));
                _repository.Write(new BatteryObservation(e.State, groupId));
                _repository.Write(new HobbsMeterObservation(e.State, groupId));
            }
        }

        public ObservableCollection<ITelloState> StateHistory { get; } = new ObservableCollection<ITelloState>();

        private ITelloState _state;
        public ITelloState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        internal void ClearDatabase()
        {
            if (_repository != null)
            {
                _repository.Delete<PositionObservation>();
                _repository.Delete<AttitudeObservation>();
                _repository.Delete<AirSpeedObservation>();
                _repository.Delete<BatteryObservation>();
                _repository.Delete<HobbsMeterObservation>();
                _repository.Delete<TelloStateObservation>();
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        private IInputCommand _ClearDatabaseCommand;
        public IInputCommand ClearDatabaseCommand => _ClearDatabaseCommand = _ClearDatabaseCommand ?? new InputCommand(ClearDatabase);
#pragma warning restore IDE1006 // Naming Styles
    }
}
