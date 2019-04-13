﻿using System;
using System.Collections.Generic;

namespace Tello.App.MvvM
{
    public class OpenEventArgs : EventArgs
    {
        public OpenEventArgs() : this(null)
        {
        }

        public OpenEventArgs(Dictionary<string, object> args)
        {
            Args = args != null
                ? new Dictionary<string, object>(args)
                : new Dictionary<string, object>();
        }

        public Dictionary<string, object> Args { get; }
    }

    public class ClosingEventArgs : EventArgs
    {
        public bool CanClose { get; set; }
    }

    public class ViewModel : PropertyChangedNotifier
    {
        protected readonly IUINotifier UserNotifier;

        public ViewModel(IUIDispatcher dispatcher, IUINotifier userNotifier) : base(dispatcher)
        {
            DisplayName = $"#{GetType().Name}#";
            UserNotifier = userNotifier ?? throw new ArgumentNullException(nameof(userNotifier));
        }

        public string DisplayName { get; set; }

        public void Open(OpenEventArgs args = null)
        {
            OnOpen(args);
        }

        public bool Close()
        {
            if (CanClose)
            {
                var args = new ClosingEventArgs { CanClose = true };
                OnClosing(args);
                CanClose = args.CanClose;
            }
            return CanClose;
        }

        private bool _canClose = true;
        public bool CanClose { get => _canClose; set => SetProperty(ref _canClose, value); }

        protected virtual void OnOpen(OpenEventArgs args) { }
        protected virtual void OnClosing(ClosingEventArgs args) { }
    }
}
