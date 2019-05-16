﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Simulator
{
    public sealed class SimReceiver : Receiver
    {
        private readonly IDroneTransmitter _transmitter;

        public SimReceiver(IDroneTransmitter transmitter)
        {
            _transmitter = transmitter ?? throw new ArgumentNullException(nameof(transmitter));
        }

        protected override async Task Listen(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            var wait = new SpinWait();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_transmitter.Available > 0)
                    {
                        MessageReceived(new Envelope((await _transmitter.ReceiveAsync()).Buffer));
                    }
                    else
                    {
                        wait.SpinOnce();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionThrown(ex);
                }
            }
        }
    }
}
