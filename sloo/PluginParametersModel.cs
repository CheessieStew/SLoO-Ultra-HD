using Jacobi.Vst.Framework;
using System;
using System.ComponentModel;

namespace sloo
{
    public class PluginParametersModel: INotifyPropertyChanged
    {
        public PluginParameterFactory ParameterFactory;

        public float Volume { get; set; }
        public float Frequency { get;  set; }

        public VstParameterManager _volumeManager;
        public VstParameterManager _frequencyManager;


        public PluginParametersModel(PluginParameterFactory parameterFactory)
        {
            ParameterFactory = parameterFactory;
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
            _volumeManager = new VstParameterManager(paramInfo);
            _volumeManager.PropertyChanged += _volumeManager_PropertyChanged;
            ParameterFactory.ParameterInfos.Add(paramInfo);
            paramInfo = new VstParameterInfo();
            paramInfo.CanBeAutomated = true;
            paramInfo.Name = "freq";
            paramInfo.Label = "Frequency";
            paramInfo.ShortLabel = "frq:";
            paramInfo.MaxInteger = 2000;
            paramInfo.MinInteger = 0;
            paramInfo.LargeStepFloat = 2.0f;
            paramInfo.SmallStepFloat = 0.5f;
            paramInfo.StepFloat = 10.0f;
            paramInfo.DefaultValue = 2;
            VstParameterNormalizationInfo.AttachTo(paramInfo);
            _frequencyManager = new VstParameterManager(paramInfo);
            _frequencyManager.PropertyChanged += _frequencyManager_PropertyChanged;
            ParameterFactory.ParameterInfos.Add(paramInfo);
        }

        private void _frequencyManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                VstParameterManager paramMgr = (VstParameterManager)sender;
                Frequency = (paramMgr.CurrentValue);
                OnPropertyChanged(nameof(Frequency));
            }
        }

        private void _volumeManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
        public event PropertyChangedEventHandler PropertyChanged;

    }
}