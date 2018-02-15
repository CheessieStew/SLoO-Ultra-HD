using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public class SlooException : Exception
    {
        public SlooException(string name, string message) : base($"{name}: {message}")
        {

        }
        public SlooException(string name, string message, Exception inner) : base($"{name}: {message}", inner)
        {

        }
    }
}
