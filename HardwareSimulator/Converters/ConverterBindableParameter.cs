using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace HardwareSimulator.Converters
{
    public class ConverterBindableBinding : MarkupExtension
    {
        [ContentProperty(nameof(Converter))]
        private sealed class MultiValueConverterAdapter : IMultiValueConverter
        {
            public IValueConverter Converter { get; set; }

            private object lastParameter;

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values[0] == System.Windows.DependencyProperty.UnsetValue) return null;
                if (Converter == null) return values[0]; // Required for VS design-time
                if (values.Length > 1) lastParameter = values[1];
                return Converter.Convert(values[0], targetType, lastParameter, culture);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                if (Converter == null) return new object[] { value }; // Required for VS design-time

                return new object[] { Converter.ConvertBack(value, targetTypes[0], lastParameter, culture) };
            }
        }

        public Binding Binding { get; set; }
        public IValueConverter Converter { get; set; }
        public Binding ConverterParameterBinding { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(Binding);
            multiBinding.Bindings.Add(ConverterParameterBinding);
            multiBinding.Converter = new MultiValueConverterAdapter
            {
                Converter = Converter
            };
            return multiBinding.ProvideValue(serviceProvider);
        }
    }
}