﻿using System;
using System.Text;
using System.Threading.Tasks;
using Tello.Messaging;

namespace Tello.Emulator.SDKV2
{
    internal sealed class VideoServer : IRelayService<IVideoSample>
    {
        public VideoServer(double frameRate, int secondsToBuffer)
        {
            _frameRate = frameRate;
            _sampleDuration = TimeSpan.FromSeconds(1 / _frameRate);
        }

        private readonly byte[] _sample = Encoding.UTF8.GetBytes("This is fake frame data.");
        private readonly double _frameRate;
        private readonly TimeSpan _sampleDuration;

        public event EventHandler<RelayMessageReceivedArgs<IVideoSample>> RelayMessageReceived;
        public event EventHandler<RelayExceptionThrownArgs> RelayExceptionThrown;

        public ReceiverStates State { get; private set; }

        public async void Start()
        {
            if (State != ReceiverStates.Listening)
            {
                State = ReceiverStates.Listening;

                await Task.Run(async () =>
                {
                    var frameCount = 0;
                    while (State == ReceiverStates.Listening)
                    {
                        await Task.Delay(_sampleDuration);
                        try
                        {
                            var frame = new VideoFrame(_sample, frameCount, TimeSpan.FromSeconds(frameCount / _frameRate), _sampleDuration);
                            var eventArgs = new RelayMessageReceivedArgs<IVideoSample>(frame);
                            RelayMessageReceived?.Invoke(this, eventArgs);
                            ++frameCount;
                        }
                        catch (Exception ex)
                        {
                            RelayExceptionThrown?.Invoke(this, new RelayExceptionThrownArgs(ex));
                        }
                    }
                });
            }
        }

        public void Stop()
        {
            State = ReceiverStates.Stopped;
        }
    }

    //    internal sealed class VideoServer : UdpServer
    //    {
    //        private int _sampleIndex = 0;
    //        private readonly byte[] _videoData;
    //        private readonly Sample[] _sampleDefs;
    //        private TimeSpan _previousTimeIndex = TimeSpan.FromSeconds(0.0);

    //        public VideoServer(int port, byte[] videoData, Sample[] sampleDefs) : base(port)
    //        {
    //            _videoData = videoData;
    //            _sampleDefs = sampleDefs;
    //        }


    //        private readonly TimeSpan _wait = TimeSpan.FromMilliseconds(2.4);
    //        protected override Task<byte[]> GetDatagram()
    //        {
    //            var sampleDef = _sampleDefs[_sampleIndex];

    //            // sleep to simulate Tello's actual sample rate
    //            //await Task.Delay(TimeSpan.FromMilliseconds(1));
    //            Thread.Sleep(_wait);

    //            // get the video sample 
    //            var sample = new byte[sampleDef.Length];
    //            Array.Copy(_videoData, sampleDef.Offset, sample, 0, sampleDef.Length);

    //            // advance the sample index, wrap at the end
    //            _sampleIndex = (_sampleIndex + 1) % _sampleDefs.Length;

    //            return Task.FromResult(sample);
    //        }
    //    }
}