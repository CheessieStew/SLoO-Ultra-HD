using Jacobi.Vst.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynthFW
{
    public class Delay
    {
        public string Name = "Delay";
        public Signal<int> DelayLength = new ConstantSignal<int>(() => 3000);

        private int _bufferLen;
        private double[] _delayBuffer;
        private int _tapInPosition;
        private double _tapOutPosition;
        private DelaySignalSource _out;

        public DynamicSignal<double> Out { get; private set; }

        public Signal<double> PlaybackSpeed = new ConstantSignal<double>(() =>1);
        public Signal<double> Input;
        public ConstantSignal<double> Gain = new ConstantSignal<double>(() => 1);

        public Delay()
        {
            _out = new DelaySignalSource(this);
            Out = new DynamicSignal<double>(_out, Name + "_out");
        }

        private int _inBlock;
        private int _outBlock;

        private void GetBlock(double[,] output, byte blockNr)
        {
            if (Input == null || PlaybackSpeed == null)
                return;
            DelayLength.NextBlock(blockNr);
            if (DelayLength[0,0] != _bufferLen)
            {
                _bufferLen = Math.Max(1100, DelayLength[0, 0]);
                _tapInPosition = 0;
                _tapOutPosition = 0;
                _tapOutPosition = 0;
                _delayBuffer = new double[_bufferLen];
                Logger.LogLine($"{Name} delay len set to {_bufferLen}");
            }

            SetOutBlock(output, blockNr);
            ProcessInBlock(blockNr);
        }

        private void SetOutBlock(double[,] output, byte blockNr)
        {
            if (blockNr == _outBlock)
                return;
            PlaybackSpeed.NextBlock(blockNr);
            _outBlock = blockNr;
            for (int sample = 0; sample < output.GetLength(0); sample++)
            {
                var blend = _tapOutPosition - (int)_tapOutPosition;
                output[sample, 0] = _delayBuffer[((int)_tapOutPosition) % _bufferLen] * (1 - blend)
                    + _delayBuffer[((int)(_tapOutPosition + 0.5)) % _bufferLen] * blend;

                _tapOutPosition += PlaybackSpeed[sample, 0];
                if (_tapOutPosition >= _bufferLen)
                    _tapOutPosition -= _bufferLen;
            }
        }

        private void ProcessInBlock(byte blockNr)
        {
            if (blockNr == _inBlock)
                return;
            _inBlock = blockNr;
            Input.NextBlock(blockNr);
            Gain.NextBlock(blockNr);

            for (int sample = 0; sample < Input.BlockSize; sample++)
            {
                _delayBuffer[_tapInPosition] = Enumerable.Range(0, Input.Channels).Sum(c => Input[sample, c]) * Gain[sample, 0];

                _tapInPosition = (_tapInPosition + 1) % _bufferLen;
            }
        }


        private class DelaySignalSource : ISignalSource<double>
        {
            private Delay _delay;

            public DelaySignalSource(Delay delay)
            {
                _delay = delay;
            }

            public void GetBlock(double[,] buffer, byte blockNr)
            {
                _delay.GetBlock(buffer, blockNr);
            }
        }

    }
}
