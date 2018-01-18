﻿namespace Jacobi.Vst.Samples.MidiNoteSampler
{
    using Jacobi.Vst.Core;
    using Jacobi.Vst.Framework;
    using Jacobi.Vst.Framework.Plugin;
    using sloo;
    using System;

    /// <summary>
    /// Implements the audio processing of the plugin using the <see cref="SampleManager"/>.
    /// </summary>
    internal class AudioProcessor : VstPluginAudioProcessorBase
    {
        private Plugin _plugin;
        private Random _rng = new Random();




        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0)
        {
            _plugin = plugin;


        }
        private Oscillator _osc = new Oscillator();

        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {

            _osc.Frequency = _plugin.Model.Frequency;
            _osc.CurrentSampleRate = this.SampleRate;
            VstAudioBuffer input = inChannels[0];
            VstAudioBuffer output = outChannels[0];
            
            
            for (int index = 0; index < output.SampleCount; index++)
            {
                output[index] = (float)(_osc++.State() + input[index]) * _plugin.Model.Volume / 2;
            }

            input = inChannels[1];
            output = outChannels[1];

            for (int index = 0; index < output.SampleCount; index++)
            {
                output[index] = _plugin.Model.Volume *  (float)(input[index] * (0.8 + 0.4 * _rng.NextDouble()));
            }
        }
    }
}
