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
        public ConstantSignal<double> Amplitude = new ConstantSignal<double>(() => 1);
        public ConstantSignal<double> Base = new ConstantSignal<double>(() => 0);
        public ConstantSignal<int> OctaveShift = new ConstantSignal<int>(() => 0);

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
            Base.NextBlock(blockNr);
            Amplitude.NextBlock(blockNr);
            OctaveShift.NextBlock(blockNr);
            _guts.ForEach(g => g.OctaveShift = OctaveShift[0, 0]);
            for (int sample = 0; sample < buffer.GetLength(0); sample++)
            {
                for (int channel = 0; channel < Channels; channel++)
                {
                    _guts[channel].Frequency = Frequency[sample, channel];
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
            public WaveShape Shape;
            private double _angleDelta;
            private float _currentSampleRate;
            private double _shiftedFrequency;
            private double _frequency;
            public double Angle { get; private set; }

            public int OctaveShift;
            public double Frequency
            {
                get => _frequency;
                set
                {
                    if (_frequency != value && value != 0)
                    {
                        _frequency = value;
                        _shiftedFrequency = value * Math.Pow(2, OctaveShift);
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
                double cyclesPerSample = _shiftedFrequency / _currentSampleRate; // [2]
                _angleDelta = cyclesPerSample * 2.0 * Math.PI;                                // [3]
            }

            public double State
            {
                get
                {
                    if (_shiftedFrequency <= 0)
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
                            return (Angle - period * TwoPi) / Math.PI - 1;
                            //return 2*Math.Abs(res) - 1;
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
