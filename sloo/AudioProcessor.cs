namespace sloo
{
    using Jacobi.Vst.Core;
    using Jacobi.Vst.Framework;
    using Jacobi.Vst.Framework.Plugin;
    using sloo;
    using SynthFW;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AudioProcessor : VstPluginAudioProcessorBase
    {
        public const int NoteChannels = 5;
        private Plugin _plugin;
        private Random _rng = new Random();
        private int _blockSize;
        private float _sampleRate;
        private double[,] _scratchBuffer1;
        private double[,] _scratchBuffer2;
        private double[,] _scratchBuffer3;

        private double[,] _scratchBufferChannels1;
        private double[,] _scratchBufferChannels2;
        private double[,] _scratchBufferChannels3;

        private Signal<double> _out;

        private uint CurrentSample;
        private byte _blockNr;
        private int _timeOut;
        private MidiManager _midiManager;

        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0)
        {
            _plugin = plugin;


        }

        public void SetUp()
        {
            _out = new ConstantSignal<double>(() => 0.0);
            if (_midiManager != null)
            {
                _plugin.MidiProcessor.NoteOff -= _midiManager.NoteOff;
                _plugin.MidiProcessor.NoteOn -= _midiManager.NoteOn;
            }
            _midiManager = new MidiManager(NoteChannels);
            _plugin.MidiProcessor.NoteOff += _midiManager.NoteOff;
            _plugin.MidiProcessor.NoteOn += _midiManager.NoteOn;

            _scratchBuffer1 = new double[this.BlockSize, 1];
            _scratchBuffer2 = new double[this.BlockSize, 1];
            _scratchBuffer3 = new double[this.BlockSize, 1];

            _scratchBufferChannels1 = new double[this.BlockSize, NoteChannels];
            _scratchBufferChannels2 = new double[this.BlockSize, NoteChannels];
            _scratchBufferChannels3 = new double[this.BlockSize, NoteChannels];

            _blockSize = BlockSize;
            _sampleRate = SampleRate;



            var sine1 = new Oscillator(NoteChannels, this.SampleRate)
            {
                Shape = Oscillator.WaveShape.Sine,
                Frequency = _midiManager.ChannelFrequencies,
                Amplitude = new ConstantSignal<double>(() => _plugin.Model.Sine)
            };
            sine1.Out.Buffer = _scratchBufferChannels1;

            var square1 = new Oscillator(NoteChannels, this.SampleRate)
            {
                Shape = Oscillator.WaveShape.Square,
                Frequency = _midiManager.ChannelFrequencies,
                Amplitude = new ConstantSignal<double>(() => _plugin.Model.Square)
            };
            square1.Out.Buffer = _scratchBufferChannels2;
            
            var triangle1 = new Oscillator(NoteChannels, this.SampleRate)
            {
                Shape = Oscillator.WaveShape.Square,
                Frequency = _midiManager.ChannelFrequencies,
                TriangleSkew = new ConstantSignal<double>(() => _plugin.Model.TriangleSkew),
                Amplitude = new ConstantSignal<double>(() => _plugin.Model.Triangle)
            };
            triangle1.Out.Buffer = _scratchBufferChannels3;

            var sourceSum = new Sum("sourceSum");
            sourceSum.Inputs.Add(sine1.Out);
            sourceSum.Inputs.Add(square1.Out);
            sourceSum.Inputs.Add(triangle1.Out);
            sourceSum.Buffer = _scratchBufferChannels1;

            var adsr = new Adsr(NoteChannels, this.SampleRate)
            {
                ReleaseDuration = new ConstantSignal<double>(() => _plugin.Model.ReleaseDuration/5),
                AttackDuration = new ConstantSignal<double>(() => _plugin.Model.AttackDuration/10),
                DecayDuration = new ConstantSignal<double>(() => _plugin.Model.DecayDuration/10),
                Attack = new ConstantSignal<double>(() => _plugin.Model.AttackStrength),
                Sustain = new ConstantSignal<double>(() => _plugin.Model.SustainStrength),
                Triggers = _midiManager.ChannelTriggers,
                Input = sourceSum
            };
            adsr.Out.Buffer = _scratchBufferChannels1;


            var delayOsc = new Oscillator(1, SampleRate)
            {
                Shape = Oscillator.WaveShape.Triangle,
                Frequency = new ConstantSignal<double>(() => _plugin.Model.Delay1PlaybackSpeedFreq),
                Amplitude = new ConstantSignal<double>(() => _plugin.Model.Delay1PlaybackSpeedMod / 2),
                Base = new ConstantSignal<double>(() => 1)
            };
            delayOsc.Out.Buffer = _scratchBuffer2;



            var delay = new Delay()
            {
                DelayLength = new ConstantSignal<int>(() => _plugin.Model.Delay1Length),
                PlaybackSpeed = delayOsc.Out,
                Gain = new ConstantSignal<double>(() => _plugin.Model.Delay1Gain)
            };
            delay.Out.Buffer = _scratchBuffer1;

            var combine = new Mix("Combine");
            combine.Inputs.Add(delay.Out);
            combine.Inputs.Add(adsr.Out.Flat((sum,cur) => sum+cur, 0d));
            combine.Strengths.Add(new ConstantSignal<double>(() => _plugin.Model.Wet1));
            combine.Strengths.Add(new ConstantSignal<double>(() => 1-_plugin.Model.Wet1));
            combine.Buffer = _scratchBuffer3;

            var lowPass1 = new LowPass(1)
            {
                Scale = new ConstantSignal<double>(() => _plugin.Model.Filter1);
            }


            delay.Input = combine;
            
            var softClip = new ApplyFunction(sample => sample / (1 + Math.Abs(sample)), "SoftClip")
            {
                Buffer = _scratchBuffer1,
                Input = delay.Out,
            };

            _out = softClip;
        }



        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            if (_timeOut > 0)
            {
                _timeOut--;
                return;
            }
            try
            {
                if (_blockSize != BlockSize || _sampleRate != SampleRate)
                    SetUp();
                if (_out == null)
                    throw new SlooException("Process", "null output");
                VstAudioBuffer input = inChannels[0];
                VstAudioBuffer output = outChannels[0];
                _out.NextBlock(++_blockNr);
                for (int index = 0; index < output.SampleCount; index++)
                {
                    output[index] = 0;
                    for (int c = 0; c < _out.Channels; c++)
                        output[index] += (float)_out[index, c];
                }

                for (int channel = 1; channel < outChannels.Length; channel++)
                    for (int index = 0; index < output.SampleCount; index++)
                        outChannels[channel][index] = output[index];

                CurrentSample += (uint)this.BlockSize;
            }
            catch(Exception e)
            {
                var msg = new StringBuilder(e.Message);
                msg.AppendLine(e.StackTrace);
                msg.Append("INNER: ");
                msg.AppendLine(e?.InnerException?.Message ?? "");
                msg.AppendLine(e?.InnerException?.StackTrace ?? "");
                LogLine(msg.ToString());
                _timeOut = 50;
            }
        }
              

        private void LogLine(string s) => Logger.LogLine(s);

    }
}
