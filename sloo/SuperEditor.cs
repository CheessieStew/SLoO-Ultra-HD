using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Samples.MidiNoteSampler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace sloo
{
    class SuperEditor : IVstPluginEditor
    {
        private WpfControlWrapper<EditorControl> _editorCtrl = new WpfControlWrapper<EditorControl>(300, 300);
        private Plugin _plugin;
        public SuperEditor(Plugin p)
        {
            _plugin = p;
        }
        public VstKnobMode KnobMode { get => VstKnobMode.CircularMode; set { } }

        public Rectangle Bounds
        { get
            {
                _editorCtrl.GetBounds(out var rect);
                return rect;
        }
        }

        public void Close()
        {
            _editorCtrl.Close();
        }

        public bool KeyDown(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return true;
        }

        public bool KeyUp(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return true;
        }

        public void Open(IntPtr hWnd)
        {

            _editorCtrl.Open(hWnd);
            _editorCtrl.Instance.Host = _plugin._hostStub;
            _editorCtrl.Instance.Plugin = _plugin;
        }

        public void ProcessIdle()
        {
        }
    }
}
