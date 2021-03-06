﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public class Multiply : DynamicSignal<double>
    {
        public Signal<double> Input2;
        public Signal<double> Input1;        
        

        public Multiply(string name) : base(null, name)
        {
            
        }

        public override void NextBlock(byte blockNr)
        {
            if (_blockNr != blockNr)
            {
                if (Buffer == null)
                    throw new SlooException(Name, "null output buffer");

                _blockNr = blockNr;
                Input1.NextBlock(blockNr);
                Input2.NextBlock(blockNr);
                var outChannels = Buffer.GetLength(1);

                if (outChannels == Input1.Channels)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                            Buffer[sample, channel] = Input1[sample, channel] * Input2[sample, channel];
                    }
                else if (outChannels > Input1.Channels && Input1.Channels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        for (int channel = 0; channel < Buffer.GetLength(1); channel++)
                            Buffer[sample, channel] = Input1[sample, 0] * Input2[sample, 0];
                    }
                else if (outChannels < Input1.Channels && outChannels == 1)
                    for (int sample = 0; sample < Buffer.GetLength(0); sample++)
                    {
                        double buf1 = 0;
                        double buf2 = 0;
                        for (int channel = 0; channel < Input1.Channels; channel++)
                        {
                            buf1 += Input1[sample, channel];
                            buf2 += Input2[sample, channel];
                        }
                        Buffer[sample, 0] = buf1 * buf2;
                    }                
                else throw new SlooException(Name, "Buffer sizes don't match");

            }
        }
        

    }
}
