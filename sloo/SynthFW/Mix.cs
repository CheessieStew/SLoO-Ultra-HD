using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public class Mix: DynamicSignal<double>
    {
        public readonly List<Signal<double>> Inputs = new List<Signal<double>>();
        public readonly List<Signal<double>> Strengths = new List<Signal<double>>();

        public Mix(string name) : base(null, name)
        {
            
        }

        public override void NextBlock(byte blockNr)
        {
            if (_blockNr != blockNr)
            {
                if (Buffer == null)
                    throw new SlooException(Name, "null output buffer");
                if (Inputs.Count == 0)
                    return;
                if (Inputs.Count != Strengths.Count)
                    throw new SlooException(Name, "Does not seem to work bro");
                _blockNr = blockNr;
                Inputs.ForEach(i => i.NextBlock(blockNr));
                Strengths.ForEach(i => i.NextBlock(blockNr));

                var outChannels = Buffer.GetLength(1);

                if (outChannels == Inputs[0].Channels)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                        {
                            Buffer[sample, channel] = 0;
                            for (int input = 0; input < Inputs.Count; input++)
                            {
                                Buffer[sample, channel] += Inputs[input][sample, channel] * Strengths[input][sample,channel];
                            }
                        }
                    }
                else if (outChannels > Inputs[0].Channels && Inputs[0].Channels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                        {
                            Buffer[sample, channel] = 0;
                            for (int input = 0; input < Inputs.Count; input++)
                            {
                                Buffer[sample, channel] += Inputs[input][sample, 0] * Strengths[input][sample, 0];
                            }
                        }
                    }
                else if (outChannels < Inputs[0].Channels && outChannels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        Buffer[sample, 0] = 0;
                        for (int channel = 0; channel < Inputs[0].Channels; channel++)
                        {
                            for (int input = 0; input < Inputs.Count; input++)
                            {
                                Buffer[sample, 0] += Inputs[input][sample, channel] * Strengths[input][sample, channel];
                            }
                        }
                        
                    }                
                else throw new SlooException(Name, "Buffer sizes don't match");

            }
        }
        

    }
}
