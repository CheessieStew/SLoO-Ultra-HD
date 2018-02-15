using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynthFW
{
    public class Oscillator
    {
        public string Name = "Oscillator";
        public enum WaveShape
        {
            Sine,
            Square,
            AntiAliasedSquare,
            Triangle
        }
        public Signal<double> Frequency;
        public Signal<double> TriangleSkew = new ConstantSignal<double>(() => 0.5);
        public ConstantSignal<double> Amplitude = new ConstantSignal<double>(() => 1);
        public ConstantSignal<double> Base = new ConstantSignal<double>(() => 0);

        private MultiOscillatorSignalSource _out;
        public DynamicSignal<double> Out { get; private set; }

        private int Channels;
        private SimpleOscillator[] _guts;

        private float _sampleRate;
        public float SampleRate
        {
            get => _sampleRate;
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    _guts.ForEach(g => g.SampleRate = _sampleRate);
                }
            }
        }

        private WaveShape _shape;
        public WaveShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                _guts.ForEach(g => g.Shape = value);
            }
        }



        public Oscillator(int channels, float sampleRate)
        {
            _guts = new SimpleOscillator[channels];
            Enumerable.Range(0, channels).ForEach(i => _guts[i] = new SimpleOscillator());
            SampleRate = sampleRate;
            Channels = channels;
            _out = new MultiOscillatorSignalSource(this);
            Out = new DynamicSignal<double>(_out, Name+"_out");
        }

        private void Tick()
        {
            _guts.ForEach(g => g.Tick());
        }

        private void GetBlock(double[,] buffer, byte blockNr)
        {
            Frequency.NextBlock(blockNr);
            TriangleSkew.NextBlock(blockNr);
            Base.NextBlock(blockNr);
            Amplitude.NextBlock(blockNr);
            for (int sample = 0; sample < buffer.GetLength(0); sample++)
            {
                for (int channel = 0; channel < Channels; channel++)
                {
                    _guts[channel].Frequency = Frequency[sample, channel];
                    _guts[channel].TriangleSkew = TriangleSkew[sample, channel];
                    buffer[sample, channel] = _guts[channel].State * Amplitude[sample,channel] + Base[sample,channel];
                    Tick();
                }
            }
        }

        private class MultiOscillatorSignalSource : ISignalSource<double>
        {
            private Oscillator _multiOscillator;
            
            public MultiOscillatorSignalSource(Oscillator multiOscillator)
            {
                _multiOscillator = multiOscillator;
            }

            public void GetBlock(double[,] buffer, byte blockNr)
            {
                _multiOscillator.GetBlock(buffer, blockNr);
            }
        }



        private class SimpleOscillator
        {
            public double TriangleSkew;
            public WaveShape Shape;
            private double _angleDelta;
            private float _currentSampleRate;
            private double _frequency;
            public double Angle { get; private set; }

            public double Frequency
            {
                get => _frequency;
                set
                {
                    if (_frequency != value && value != 0)
                    {
                        _frequency = value;
                        UpdateAngleDelta();
                    }

                }
            }

            public float SampleRate
            {
                get => _currentSampleRate;
                set
                {
                    if (_currentSampleRate != value)
                    {
                        _currentSampleRate = value;
                        UpdateAngleDelta();
                    }
                }
            }


            private void UpdateAngleDelta()
            {
                double cyclesPerSample = _frequency / _currentSampleRate; // [2]
                _angleDelta = cyclesPerSample * 2.0 * Math.PI;                                // [3]
            }

            public double State
            {
                get
                {
                    if (_frequency <= 0)
                        return 0;
                    switch (Shape)
                    {
                        case WaveShape.Sine:
                            return Math.Sin(Angle);
                        case WaveShape.AntiAliasedSquare:
                            return (Math.Sin(Angle) + Math.Sin(Angle * 3) + Math.Sin(Angle * 5) + Math.Sin(Angle * 7) + Math.Sin(Angle * 9)) / 5;
                        case WaveShape.Square:
                            return (int)(Angle / Math.PI) % 2 == 0 ? 1 : -1;
                        case WaveShape.Triangle:
                            var period = (int)(Angle / TwoPi);
                            var res = (Angle / TwoPi - period * TwoPi) - TriangleSkew;
                            res /= res < 0 ? TriangleSkew : (1 - TriangleSkew);
                            return 2*Math.Abs(res) - 1;
                    }
                    throw new NotImplementedException();
                }
            }

            private const double TwoPi = Math.PI * 2;

            public void Tick()
            {
                Angle += _angleDelta;
                if (Angle > float.MaxValue / 2)
                    Angle = 0;
            }
        }
    }

}
