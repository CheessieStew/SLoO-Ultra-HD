using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public class ApplyFunction : DynamicSignal<double>
    {
        private Func<double, double> _function;
        public Signal<double> Input;        

        public ApplyFunction(Func<double, double> function, string name) : base(null, name)
        {
            _function = function;
        }

        public override void NextBlock(byte blockNr)
        {
            if (_blockNr != blockNr)
            {
                if (Buffer == null)
                    throw new SlooException(Name, "null output buffer");
                _blockNr = blockNr;
                Input.NextBlock(blockNr);
                var outChannels = Buffer.GetLength(1);
                if (outChannels == Input.Channels)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                            Buffer[sample, channel] = _function(Input[sample, channel]);
                    }
                else if (outChannels > Input.Channels && Input.Channels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                            Buffer[sample, channel] = _function(Input[sample, 0]);
                    }
                else if (outChannels < Input.Channels && outChannels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        Buffer[sample, 0] = 0;
                        for (int channel = 0; channel < Input.Channels; channel++)
                        {
                            Buffer[sample, 0] += Input[sample, channel];
                        }
                        Buffer[sample, 0] = _function(Buffer[sample, 0]);
                    }
                else throw new SlooException(Name, "Buffer sizes don't match");
            }
        }
        

    }
}
