using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jacobi.Vst.Framework;
using SynthFW;

namespace sloo
{
    public class VstParameters : SynthFW.SynthParameters
    {

        public override void SetUpParameter(PropertyInfo property, SynthParameter param)
        {
            var catName = param.Category ?? "Default";
            if (!(Categories.Contains(catName)))
                Categories.Add(new VstParameterCategory() { Name = catName });
            var category = Categories[catName];

            var paramInfo = new VstParameterInfo();
            paramInfo.Category = category;
            paramInfo.CanBeAutomated = true;
            paramInfo.Name = param.Name;
            paramInfo.Label = property.Name;
            paramInfo.ShortLabel = param.ShortLabel;
            paramInfo.MinInteger = param.Lo;
            paramInfo.MaxInteger = param.Hi;
            paramInfo.DefaultValue = param.DefaultValue;
            VstParameterNormalizationInfo.AttachTo(paramInfo);
            var manager = new VstParameterManager(paramInfo);
            _managers.Add(manager);
            if (property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                manager.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                {
                    if (e.PropertyName == "CurrentValue")
                    {
                        VstParameterManager paramMgr = (VstParameterManager)sender;
                        property.SetValue(this, paramMgr.CurrentValue);
                        OnPropertyChanged(property.Name);
                    }
                };
            else if (property.PropertyType == typeof(int))
                manager.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                {
                    if (e.PropertyName == "CurrentValue")
                    {
                        VstParameterManager paramMgr = (VstParameterManager)sender;
                        property.SetValue(this, (int)(paramMgr.CurrentValue + 0.5f));
                        OnPropertyChanged(property.Name);
                    }
                };
            else
                throw new InvalidOperationException($"Parameter type {property.PropertyType} is not supported (yet?)");
            ParameterInfos.Add(paramInfo);
        }

        private HashSet<VstParameterManager> _managers = new HashSet<VstParameterManager>();

        public VstParameterCategoryCollection Categories = new VstParameterCategoryCollection();

        public VstParameterInfoCollection ParameterInfos = new VstParameterInfoCollection();

        public void CreateParameters(VstParameterCollection parameters)
        {
            foreach (VstParameterInfo paramInfo in ParameterInfos)
            {
                VstParameter param = new VstParameter(paramInfo);
                parameters.Add(param);
            }
        }
    }
}
