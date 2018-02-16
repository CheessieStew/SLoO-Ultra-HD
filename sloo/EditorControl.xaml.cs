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
using Jacobi.Vst.Core;
using Jacobi.Vst.Samples.MidiNoteSampler;
using SynthFW;

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
                DataContext = value.Model;
                _plugin = value;
                Logger.UpdateLog += str => Dispatcher.Invoke(() => UpdateLog(str));
                Logger.LogLine("");
            }
        }

        private void UpdateLog(string s)
        {
            LogBox.Text = s;
        }

        public float Volume { get; set; }
        public EditorControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.ClearLog();
        }

        
    }
}
