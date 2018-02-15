using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public static class Logger
    {
        public static event Action<string> UpdateLog;

        private static string _logMsg = "";
        private static int _lines;

        public static void LogLine(string s)
        {
            if (_lines > 300)
            {
                _lines = 0;
                ClearLog();
            }
            _lines++;
            _logMsg = $"[{DateTime.Now.ToLongTimeString()}]{s}\n{_logMsg}";
            UpdateLog?.Invoke(_logMsg);
        }

        public static void ClearLog()
        {
            _logMsg = "";
            UpdateLog(_logMsg);
        }
    }
}
