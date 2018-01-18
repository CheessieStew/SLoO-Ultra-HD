using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sloo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    
    
    class Oscillator
    {
        private double _angleDelta;

        private double _frequency;
        
        public double Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    UpdateAngleDelta();
                }

            }
        }

        private double _currentSampleRate;
        public double CurrentSampleRate
        {
            get => _currentSampleRate;
            set
            {
                if (_currentSampleRate != value)
                {
                    _currentSampleRate = value;
                    UpdateAngleDelta();
                }
            }
        }


        private void UpdateAngleDelta()
        {
            double cyclesPerSample = _frequency / _currentSampleRate; // [2]
            _angleDelta = cyclesPerSample * 2.0 * Math.PI;                                // [3]
        }

        private double _angle;

        public double State()
        {
            while (_angle > 10)
            {
                _angle -= 2.0 * Math.PI;
            }
            return Math.Sin(_angle);
        }

        public static Oscillator operator++(Oscillator osc)
        {
            osc._angle += osc._angleDelta;
            return osc;
        }
        
    }

}
