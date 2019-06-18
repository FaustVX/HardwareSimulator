using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HardwareSimulator.Core;

namespace HardwareSimulator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static MainWindow()
        {
            Gate.ClearGates();
            Gate.RegisterGate<Nand>();
            Gate.RegisterGate<And>();
            Gate.RegisterGate<Nor>();
            Gate.RegisterGate<XNor>();
            Gate.RegisterGate<Not>();
            Gate.RegisterGate<Or>();
            Gate.RegisterGate<Xor>();
            Gate.RegisterGate<SR_Latch>();
        }

        public IEnumerable<string> GatesName { get; } = Gate.GatesName;

        private Gate _selectedGate;
        public Gate SelectedGate
        {
            get { return _selectedGate; }
            set
            {
                if (value is null)
                    return;
                _selectedGate = value;
                SetGate();
            }
        }

        private void SetGate()
        {
            SelectedName = SelectedGate.Name;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedName));

            SetConnectors(InputConnectors, SelectedGate.Inputs, false);
            SetConnectors(OutputConnectors, SelectedGate.Outputs, false);

            ExecuteGate();

            void SetConnectors<TKey, TValue>(IDictionary<TKey, TValue> connectors, IEnumerable<TKey> keys, TValue value)
            {
                connectors.Clear();
                foreach (var connector in keys)
                    connectors[connector] = value;
            }
        }

        private string _selectedName;
        public string SelectedName
        {
            get { return _selectedName; }
            set
            {
                if (_selectedName == value)
                    return;
                _selectedName = value;
                Gate.TryGetGate(SelectedName, out var gate);
                SelectedGate = gate;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGate));
            }
        }

        public Dictionary<string, bool?> InputConnectors { get; }
        public Dictionary<string, bool?> OutputConnectors { get; }
        public Dictionary<string, bool?> InternalConnectors { get; }

        public MainWindow()
        {
            InputConnectors = new Dictionary<string, bool?>();
            OutputConnectors = new Dictionary<string, bool?>();
            InternalConnectors = new Dictionary<string, bool?>();
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadGate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HDL Files|*.hdl",
            };

            if (dialog.ShowDialog(this) ?? false)
                try
                {
                    SelectedGate = ExternalGate.Parse(dialog.FileName);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, $"Can't open {System.IO.Path.GetFileName(dialog.FileName)}", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void InputConnector_Click(object sender, RoutedEventArgs e)
        {
            switch (sender)
            {
                case CheckBox btn:
                    InputConnectors[(btn.Content as Button).Content.ToString()] = btn.IsChecked;
                    break;
                case Button button:
                    InputConnectors[button.Content.ToString()] ^= true;
                    break;
                default:
                    return;
            }
            ExecuteGate();
            e.Handled = true;
        }

        private void ExecuteGate()
        {
            InternalConnectors.Clear();
            foreach (var r in SelectedGate.Execute(InputConnectors.Select(i => (i.Key, i.Value)).ToArray()))
                if (OutputConnectors.ContainsKey(r.Key))
                    OutputConnectors[r.Key] = r.Value;
                else if (!InputConnectors.ContainsKey(r.Key))
                    InternalConnectors[r.Key] = r.Value;
            ResetDataContext();
        }

        private void ResetDataContext()
        {
            var context = DataContext;
            DataContext = null;
            DataContext = context;
        }
    }
}
