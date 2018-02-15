using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    class LowPass
    {
        public string Name = "Delay";
        public int Channels { get; private set; }

        private int _scale;
        private double[][] _filterBuffers;
        private int _bufferPosition;
        private LowPassSignalSource _out;

        public DynamicSignal<double> Out { get; private set; }

        public Signal<int> Scale = new ConstantSignal<int>(() => 20);
        public Signal<double> Input;
        

        public LowPass(int channels)
        {
            Channels = channels;
            _out = new LowPassSignalSource(this);
            Out = new DynamicSignal<double>(_out, Name + "_out");
        }

        private int _inBlock;
        private int _outBlock;

        private void GetBlock(double[,] output, byte blockNr)
        {
            if (Input == null)
                return;
            Scale.NextBlock(blockNr);
            if (Scale[0, 0] != _scale)
            {
                _scale = Math.Max(1,Scale[0, 0]);
                _bufferPosition = 0;
                _filterBuffers = new double[Channels][];
                for (int channel = 0; channel < Channels; channel++)
                    _filterBuffers[channel] = new double[_scale];
            }

            for (int sample = 0; sample < output.GetLength(0); sample++)
            {
                for (int channel = 0; channel < output.GetLength(1); channel++)
                {
                    var val = Input[sample,channel];
                    output[sample, channel] = (val + _filterBuffers[channel].Sum()) / (_filterBuffers.GetLength(1) + 1);
                    _filterBuffers[channel][_bufferPosition] = val;
                    _bufferPosition = (_bufferPosition + 1) % _scale;
                }
            }
        }

   

        private class LowPassSignalSource : ISignalSource<double>
        {
            private LowPass _lowpass;

            public LowPassSignalSource(LowPass lowpass)
            {
                _lowpass = lowpass;
            }

            public void GetBlock(double[,] buffer, byte blockNr)
            {
                _lowpass.GetBlock(buffer, blockNr);
            }
        }
    }
}
