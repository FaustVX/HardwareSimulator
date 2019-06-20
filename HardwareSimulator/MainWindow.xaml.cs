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

        public Dictionary<string, DataValue?> InputConnectors { get; }
        public Dictionary<string, DataValue?> OutputConnectors { get; }
        public Dictionary<string, DataValue?> InternalConnectors { get; }

        public MainWindow()
        {
            InputConnectors = new Dictionary<string, DataValue?>();
            OutputConnectors = new Dictionary<string, DataValue?>();
            InternalConnectors = new Dictionary<string, DataValue?>();
            InitializeComponent();

            Loaded += LoadGate_Click;
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
                    Title = System.IO.Path.GetFileName(dialog.FileName);
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
                case CheckBox box when box.Content is StackPanel stack:
                    InputConnectors[(stack.Children[0] as Button).Content.ToString()] = box.IsChecked;
                    break;
                case CheckBox box:
                    var context = ((int pos, int, bool value))box.DataContext;
                    var tag = (KeyValuePair<string, DataValue?>)box.Tag;
                    InputConnectors[tag.Key] = DataValue.SetAt(tag.Value ?? 0, context.pos, box.IsChecked ?? !context.value);
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
