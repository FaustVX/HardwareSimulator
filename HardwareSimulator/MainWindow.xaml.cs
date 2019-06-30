using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HardwareSimulator.Core;
using Microsoft.Win32;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
#endif

namespace HardwareSimulator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static MainWindow()
        {
            ClearCache();
        }

        private static void ClearCache()
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

        public LambdaCommand ClockCommand { get; }

        private bool _autoExecute = true;
        public bool AutoExecute
        {
            get { return _autoExecute; }
            set
            {
                if (_autoExecute == value)
                    return;

                _autoExecute = value;
                OnPropertyChanged();
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

            ClockCommand = new LambdaCommand(() => SelectedGate is ExternalGate ext && ext.Clock != null, () =>
             {
                 var clock = SelectedGate is ExternalGate ext ? ext.Clock : null;
                 InputConnectors[clock] = true;
                 ExecuteGate();
                 InputConnectors[clock] = false;
                 ExecuteGate();
             });

            CommandBindings.Add(new CommandBinding(ClockCommand));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadGate_Click(object sender, RoutedEventArgs e)
            => LoadGate();

        private readonly OpenFileDialog _dialog = new OpenFileDialog
        {
            Filter = "HDL Files|*.hdl",
        };

        private void LoadGate()
        {
            if ((Keyboard.Modifiers == ModifierKeys.Shift && !string.IsNullOrEmpty(_dialog.FileName)) || (_dialog.ShowDialog(this) ?? false))
                try
                {
                    SelectedGate = ExternalGate.Parse(_dialog.FileName);
                    AutoExecute = !ClockCommand.LastExecute;
                    Title = System.IO.Path.GetFileName(_dialog.FileName);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, $"Can't open {System.IO.Path.GetFileName(_dialog.FileName)}", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void InputConnector_Click(object sender, RoutedEventArgs e)
        {
            switch (sender)
            {
                case CheckBox box when ((box.Content as StackPanel)?.Children[0] as Button)?.Content is string key:
                    InputConnectors[key] = box.IsChecked;
                    break;
                case CheckBox box when box.Tag is KeyValuePair<string, DataValue?> tag:
                    var context = ((int pos, InnerType, bool value))box.DataContext;
                    InputConnectors[tag.Key] = DataValue.SetAt(tag.Value ?? 0, context.pos, box.IsChecked ?? !context.value);
                    break;
                case Button button when button.Content is string key:
                    InputConnectors[key] ^= true;
                    break;
                default:
                    return;
            }
            if (AutoExecute)
                ExecuteGate();
            else
                ResetDataContext();
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

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            ClearCache();
            LoadGate();
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
            => ExecuteGate();
    }
}
