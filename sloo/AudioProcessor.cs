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
        private double[,] _scratchBufferChannels;

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
            _scratchBufferChannels = new double[this.BlockSize, NoteChannels];

            _blockSize = BlockSize;
            _sampleRate = SampleRate;

            var square1 = new Oscillator(NoteChannels, this.SampleRate) { Shape = Oscillator.WaveShape.AntiAliasedSquare };


            var sine1 = new Oscillator(NoteChannels, this.SampleRate) { Shape = Oscillator.WaveShape.Sine };
            sine1.Frequency = _midiManager.ChannelFrequencies; 
            sine1.Out.Buffer = _scratchBufferChannels;

            var sine1out = new Multiply("sineGain");
            sine1out.Input = sine1.Out;
            sine1out.Multiplier = new ConstantSignal<double>(() => _plugin.Model.Sine);
            sine1out.Buffer = _scratchBufferChannels;

            var adsr = new Adsr(NoteChannels, this.SampleRate);

            adsr.ReleaseLength = new ConstantSignal<double>(() => 0.2);
            adsr.AttackLength = new ConstantSignal<double>(() => 0.1);
            adsr.DecayLength = new ConstantSignal<double>(() => 0.3);
            adsr.Attack = new ConstantSignal<double>(() => 0.7);
            adsr.Sustain = new ConstantSignal<double>(() => 0.4);
            adsr.Triggers = _midiManager.ChannelTriggers;

            adsr.Input = sine1out;
            adsr.Out.Buffer = _scratchBufferChannels;

            var delay = new Delay() { DelayLength = 2000 };

            delay.Input = adsr.Out;
            delay.PlaybackSpeed = new ConstantSignal<double>(() => _plugin.Model.Blur);
            delay.Out.Buffer = _scratchBuffer1;

            var _softClip = new ApplyFunction(sample => sample / (1 + Math.Abs(sample)), "SoftClip");
            _softClip.Buffer = _scratchBuffer1;
            _softClip.Input = adsr.Out;

            _out = _softClip;

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
