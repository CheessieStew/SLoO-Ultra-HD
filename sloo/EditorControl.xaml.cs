using Jacobi.Vst.Core.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Windows;
using System.Windows.Controls;

using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Plugin;
using Jacobi.Vst.Samples.MidiNoteSampler;

namespace sloo
{
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
    {
        public IVstHostCommandStub Host { get; set; }
        private Plugin _plugin;
        public Plugin Plugin
        {
            get => _plugin;
            set
            {
                DataContext = value;
                _plugin = value;
            }
        }
        public float Volume { get; set; }
        public EditorControl()
        {
            InitializeComponent();
        }
    }
}
