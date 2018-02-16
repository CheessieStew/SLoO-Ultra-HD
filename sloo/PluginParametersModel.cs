using Jacobi.Vst.Framework;
using SynthFW;
using System;
using System.ComponentModel;
using System.Linq;

namespace sloo
{
    public class PluginParametersModel: VstParameters
    {       
        [SynthParameter("Source","Sine", 0, 1, 0.5f, "sin")]
        public double Sine { get; set; }
        [SynthParameter("Source","Square", 0, 1, 0.5f, "sqr")]
        public double Square { get;  set; }
        [SynthParameter("Source","Triangle", 0, 1, 0.5f, "tri")]
        public double Triangle { get; set; }




        [SynthParameter("Envelope", "DecayDur", 0, 2, 0.1f, "dcy", IntStep = 1)]
        public double DecayDuration { get; set; }
        [SynthParameter("Envelope", "RlsDur", 0, 2, 0.5f, "rls", IntStep = 1)]
        public double ReleaseDuration { get; set; }
        [SynthParameter("Envelope", "AtkDur", 0, 2, 0.5f, "atk", IntStep = 1)]
        public double AttackDuration { get; set; }
        [SynthParameter("Envelope", "SusStr", 0, 2, 0.5f, "suss")]
        public double SustainStrength { get; internal set; }
        [SynthParameter("Envelope", "AtkStr", 0, 2, 0.5f, "atks")]
        public double AttackStrength { get; internal set; }


        [SynthParameter("Path1", "Octave1", -5, 5, 0, "o1", IntStep = 1)]
        public int Path1OctaveShift { get; set; }
        [SynthParameter("Path1", "Dl1SpdMd", 0, 1, 0, "dlsm")]
        public double Delay1PlaybackSpeedMod { get; set; }
        [SynthParameter("Path1", "Dl1SpdFr", 0, 100, 0, "dlsf")]
        public double Delay1PlaybackSpeedFreq { get; set; }
        [SynthParameter("Path1", "Dl1Gain", 0, 2, 0, "dlgn")]
        public double Delay1Gain { get; set; }
        [SynthParameter("Path1", "P1Wet", 0, 1, 0, "wet")]
        public double Wet1 { get; set; }
        [SynthParameter("Path1", "Dl1Len", 1000, 50000, 10000, "dlen")]
        public int Delay1Length { get; set; }
        [SynthParameter("Path1", "Filter1", 1, 100, 10, "fltr1")]
        public int Filter1 { get; set; }

        [SynthParameter("Path1", "Gain1", 0, 3, 1, "p1g")]
        public double Gain1 { get; set; }

        [SynthParameter("Path2", "Octave2", -5, 5, 0, "o2", IntStep = 1)]
        public int Path2OctaveShift { get; set; }
        
        [SynthParameter("Path2", "Gain2", 0, 3, 1, "p2g")]
        public double Gain2 { get; set; }
    }

   
}