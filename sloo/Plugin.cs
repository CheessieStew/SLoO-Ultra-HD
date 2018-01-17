namespace sloo
{
    using Jacobi.Vst.Core;
    using Jacobi.Vst.Core.Plugin;
    using Jacobi.Vst.Framework;
    using Jacobi.Vst.Framework.Plugin;
    using Jacobi.Vst.Samples.MidiNoteSampler;
    using sloo;
    using sloo;
    using System.Reflection;

    /// <summary>
    /// The Plugin root class that derives from the framework provided base class that also include the interface manager.
    /// </summary>
    public class Plugin : VstPluginWithInterfaceManagerBase
    {
        public PluginParameterFactory ParameterFactory { get; private set; }

        public float Volume { get; set; }
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public Plugin()
            : base("SLoO", 
                new VstProductInfo(@"__/SLoO\__/ULtRa\__/HD\__", "Cheessie Stew", 1000),
                VstPluginCategory.Synth, 
                VstPluginCapabilities.NoSoundInStop, 
                0, 
                36373435)
        {
            SampleManager = new SampleManager();
            ParameterFactory = new PluginParameterFactory();

        }

        /// <summary>
        /// Gets the sample manager.
        /// </summary>
        public SampleManager SampleManager { get; private set; }

        /// <summary>
        /// Creates a default instance and reuses that for all threads.
        /// </summary>
        /// <param name="instance">A reference to the default instance or null.</param>
        /// <returns>Returns the default instance.</returns>
        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            if (instance == null) return (p = new AudioProcessor(this));

            return base.CreateAudioProcessor(instance);
        }

        AudioProcessor p;

        /// <summary>
        /// Creates a default instance and reuses that for all threads.
        /// </summary>
        /// <param name="instance">A reference to the default instance or null.</param>
        /// <returns>Returns the default instance.</returns>
        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (instance == null) return new MidiProcessor(this);

            return base.CreateMidiProcessor(instance);
        }

        public float Asdf = 1;
        public override void Open(IVstHost host)
        {
            Asdf = 0;
            _hostStub = host.GetType().GetProperty("HostCommandStub").GetValue(host, null) as IVstHostCommandStub;
            base.Open(host);
        }

        public IVstHostCommandStub _hostStub;

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null) return new SuperEditor(this);

            return base.CreateEditor(instance);
        }
        


    }
}
