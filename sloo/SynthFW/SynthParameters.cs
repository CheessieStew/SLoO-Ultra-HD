using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public abstract class SynthParameters : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public abstract void SetUpParameter(PropertyInfo property, SynthParameter param);

        public SynthParameters()
        {
            foreach (var property in this.GetType().GetProperties())
            {
                var attrs = System.Attribute.GetCustomAttributes(property);
                var param = attrs.FirstOrDefault(attr => attr is SynthParameter) as SynthParameter;
                if (param != null)
                    SetUpParameter(property, param);
            }
        }
        
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SynthParameter : System.Attribute
    {
        public string Category { get; private set; }
        public string Name { get; private set; }
        public int Lo { get; private set; }
        public int Hi { get; private set; }
        public string ShortLabel { get; private set; }
        public float DefaultValue { get; private set; }
        public int IntStep { get; set; }
        public SynthParameter(string category, string name, int low, int high, float defaultValue, string label)
        {
            Name = name;
            Category = category;
            Lo = low;
            Hi = high;
            DefaultValue = defaultValue;
            ShortLabel = label;
        }
    }
}
