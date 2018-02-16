using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    class LowPass : DynamicSignal<double>
    {
        private int _scale = 0;
        private double[][] _filterBuffers;
        
        public override double this[int sampleNr, int channel]
        {
            get
            {
                double sum = 0;
                var endSample = sampleNr - _scale;
                for (int i = sampleNr; i >= endSample; i--)
                {
                    sum += sampleNr >= 0 ? Input[sampleNr, channel] : _filterBuffers[channel][(_bufferIdx + sampleNr + 1 + _scale) % _scale];
                }
                return sum/(_scale+1);
            }
        }

        public Signal<int> Scale = new ConstantSignal<int>(() => 20);
        public Signal<double> Input;
        private int _bufferIdx;

        public LowPass(string name) : base(null, name)
        {

        }


        public override void NextBlock(byte blockNr)
        {
            if (Input == null)
                return;
            Scale.NextBlock(blockNr);

            if (Scale[0, 0] != _scale)
            {
                _scale = Math.Max(1, Scale[0, 0]);
                _filterBuffers = new double[Channels][];
                for (int channel = 0; channel < Channels; channel++)
                    _filterBuffers[channel] = new double[_scale];
                _bufferIdx = 0;
                Logger.LogLine($"set {Name} scale to {_scale}");
            }
            var inputSamples = Input.BlockSize;
            var totalSamples = Math.Min(Input.BlockSize,_scale);
            for (int i = 0; i < totalSamples; i++)
            {
                for (int c = 0; c < Channels; c++)
                    try
                    {
                        _filterBuffers[c][_bufferIdx] = Input[Input.BlockSize - i - 1, c];
                    }
                    catch
                    {
                        throw new Exception($"fuck {c} {_bufferIdx} {_filterBuffers.Length} {_filterBuffers[0].Length}");
                    }
                _bufferIdx = (_bufferIdx - 1 + _scale) % _scale;
            }
            Input.NextBlock(blockNr);            
        }
        
    }
}
