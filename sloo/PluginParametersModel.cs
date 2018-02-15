using Jacobi.Vst.Framework;
using SynthFW;
using System;
using System.ComponentModel;
using System.Linq;

namespace sloo
{
    public class PluginParametersModel: VstParameters
    {       
        [SynthParameter("Source", 0, 1, 0.5f, "sin")]
        public float Sine { get; set; }
        public float Square { get;  set; }
        public float Sawtooth { get; set; }
        public int OctaveShift { get; set; }
        public float Blur { get; set; }
        public float Delay { get; set; }

      


        public PluginParametersModel() 
        {

        }


    }

   
}