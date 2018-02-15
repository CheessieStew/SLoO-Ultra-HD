using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynthFW
{
    public class Adsr
    {
        public string Name = "Adsr";
        private enum AdsrStage
        {
            Silent,
            Attack,
            Decay,
            Sustain,
            Release,
        }

        private (AdsrStage stage, double time)[] Stages;


        public Signal<double> Attack;
        public Signal<double> Sustain;
        
        public Signal<double> AttackLength;
        public Signal<double> DecayLength;
        public Signal<double> ReleaseLength;

        public Signal<bool> Triggers;
        public Signal<double> Input;
        public DynamicSignal<double> Out { get; private set; }

        private AdsrSignalSource _out;
        private float _sampleRate;

        public Adsr(int channels, float sampleRate)
        {
            _sampleRate = sampleRate;
            _out = new AdsrSignalSource(this);
            Out = new DynamicSignal<double>(_out, Name + "_out");
            Stages = new(AdsrStage, double)[channels];
        }

        private double AttackCurve(double x) => x;
        private double DecayCurve(double x) => Math.Pow(1 - x, 3);

        private void GetBlock(double[,] output, byte blockNr)
        {
            Triggers.NextBlock(blockNr);
            Input.NextBlock(blockNr);
            for(int channel = 0; channel < Triggers.Channels; channel++)
            {
                if (!Triggers[0,channel] && Stages[channel].stage == AdsrStage.Sustain)
                {
                    Stages[channel].stage = AdsrStage.Release;
                    Stages[channel].time = 0;
                }
                else if (Triggers[0,channel] && Stages[channel].stage == AdsrStage.Silent)
                {
                    Stages[channel].stage = AdsrStage.Attack;
                    Stages[channel].time = 0;
                }
            }

            var deltaAttack = 1/(AttackLength[0,0] * _sampleRate);
            var deltaDecay = 1/(DecayLength[0,0] * _sampleRate);
            var deltaRelease = 1/(ReleaseLength[0,0] * _sampleRate);
            var deltaSustain = 0;

            var attack = Attack[0, 0];
            var sustain = Sustain[0, 0];
            var decayTop = attack - sustain;

            for (int sample = 0; sample < output.GetLength(0); sample++)
            {
                for (int channel = 0; channel < output.GetLength(1); channel++)
                {
                    switch(Stages[channel].stage)
                    {
                        case AdsrStage.Silent:
                            output[sample, channel] = 0;
                            break;
                        case AdsrStage.Attack:
                            var ac = AttackCurve(Stages[channel].time) * attack;
                            output[sample, channel] = Input[sample,channel] * ac;
                            Stages[channel].time += deltaAttack;
                            if (Stages[channel].time > 1)
                            {
                                Stages[channel].stage = AdsrStage.Decay;
                                Stages[channel].time = 0;
                            }
                            break;
                        case AdsrStage.Decay:
                            var dc = DecayCurve(Stages[channel].time) * decayTop + sustain;
                            output[sample, channel] = Input[sample, channel] * dc;
                            Stages[channel].time += deltaDecay;
                            if (Stages[channel].time > 1)
                            {
                                Stages[channel].stage = AdsrStage.Sustain;
                                Stages[channel].time = 0;
                            }
                            break;
                        case AdsrStage.Sustain:
                            output[sample, channel] = Input[sample, channel] * sustain;
                            Stages[channel].time += deltaSustain;
                            if (Stages[channel].time > 1)
                            {
                                Stages[channel].stage = AdsrStage.Release;
                                Stages[channel].time = 0;
                            }
                            break;
                        case AdsrStage.Release:
                            var rc = DecayCurve(Stages[channel].time) * sustain;
                            output[sample, channel] = Input[sample, channel] * rc;
                            Stages[channel].time += deltaRelease;
                            if (Stages[channel].time > 1)
                            {
                                Stages[channel].stage = AdsrStage.Silent;
                                Stages[channel].time = 0;
                            }
                            break;
                    }
                }
            }
        }

        private class AdsrSignalSource : ISignalSource<double>
        {
            private Adsr _adsr;

            public AdsrSignalSource(Adsr adsr)
            {
                _adsr = adsr;
            }

            public void GetBlock(double[,] buffer, byte blockNr)
            {
                _adsr.GetBlock(buffer, blockNr);
            }
        }
    }
}
