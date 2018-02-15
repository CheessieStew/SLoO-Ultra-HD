using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SynthFW
{
    public static class Utils
    {
        public static double MidiToFrequency(int idx) => Math.Pow(2, (idx - 49) / 12.0) * 440;

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
        {
            foreach(var t in enumerable)
            {
                func(t);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Func<T, object> func)
        {
            foreach (var t in enumerable)
            {
                func(t);
            }
        }
    }
}