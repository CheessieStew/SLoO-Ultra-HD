namespace sloo
{
    using Jacobi.Vst.Core;
    using Jacobi.Vst.Core.Plugin;
    using Jacobi.Vst.Framework;
    using Jacobi.Vst.Framework.Plugin;
    using Jacobi.Vst.Samples.MidiNoteSampler;
    using sloo;
    using sloo;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// The Plugin root class that derives from the framework provided base class that also include the interface manager.
    /// </summary>
    public class Plugin : VstPluginWithInterfaceManagerBase, INotifyPropertyChanged
    {
        public PluginParameterFactory ParameterFactory { get; private set; }

        private VstParameterManager _volumeMngr;
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

            VstParameterInfo paramInfo = new VstParameterInfo();
            paramInfo.CanBeAutomated = true;
            paramInfo.Name = "volume";
            paramInfo.Label = "Volume";
            paramInfo.ShortLabel = "vlm:";
            paramInfo.MaxInteger = 10;
            paramInfo.MinInteger = 0;
            paramInfo.LargeStepFloat = 2.0f;
            paramInfo.SmallStepFloat = 0.5f;
            paramInfo.StepFloat = 10.0f;
            paramInfo.DefaultValue = 2;
            VstParameterNormalizationInfo.AttachTo(paramInfo);
            
            _volumeMngr = new VstParameterManager(paramInfo);
            _volumeMngr.PropertyChanged += _volumeMngr_PropertyChanged;
            ParameterFactory.ParameterInfos.Add(paramInfo);

            
        }

        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null) return new PluginPrograms(this);

            return instance;    // reuse initial instance
        }

        /// <summary>

        private void _volumeMngr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                VstParameterManager paramMgr = (VstParameterManager)sender;
                Volume = (paramMgr.CurrentValue);
                OnPropertyChanged(nameof(Volume));
            }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null) return new SuperEditor(this);

            return base.CreateEditor(instance);
        }
        


    }
}
