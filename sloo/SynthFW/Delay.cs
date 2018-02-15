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
        private int _delayLength;
        public int DelayLength
        {
            get => _delayLength;
            set
            {
                _delayLength = value;
                _delayLengthChanged = true;
                _tapInPosition = 0;
                _tapOutPosition = 0;
                _tapOutPosition = 0;
            }
        }
        private bool _delayLengthChanged;

        private double[] _delayBuffer;
        private int _tapInPosition;
        private double _tapOutPosition;
        private DelaySignalSource _out;

        public DynamicSignal<double> Out { get; private set; }

        public Signal<double> PlaybackSpeed;
        public Signal<double> Input;

        public Delay()
        {
            _out = new DelaySignalSource(this);
            Out = new DynamicSignal<double>(_out, Name + "_out");
        }

        private void GetBlock(double[,] output, byte blockNr)
        {
            if (Input == null || PlaybackSpeed == null)
                return;

            PlaybackSpeed.NextBlock(blockNr);
            Input.NextBlock(blockNr);

            if (_delayLengthChanged)
            {
                _delayBuffer = new double[_delayLength];
                _delayLengthChanged = false;
            }
            
            for (int sample = 0; sample < output.GetLength(0); sample++)
            {
                _delayBuffer[_tapInPosition] = Enumerable.Range(0, Input.Channels).Sum(c => Input[sample,c]);
                var blend = _tapOutPosition - (int)_tapOutPosition;
                output[sample,0] = _delayBuffer[(int)_tapOutPosition] * (1-blend) 
                    + _delayBuffer[(int)(_tapOutPosition+0.5) % DelayLength] * blend;

                _tapInPosition = (_tapInPosition + 1) % DelayLength;
                _tapOutPosition += PlaybackSpeed[sample, 0];
                if (_tapOutPosition >= DelayLength)
                    _tapOutPosition -= DelayLength;
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
