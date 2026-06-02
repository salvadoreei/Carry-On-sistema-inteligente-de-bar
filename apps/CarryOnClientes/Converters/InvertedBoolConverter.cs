using System.Globalization;

namespace BarPedidos.Converters
{
    // Converter que inverte valores booleanos
    // Usado para inverter propriedades de visibilidade
    // Exemplo: Se CarrinhoVazio = true, IsVisible com este converter = false
    public class InvertedBoolConverter : IValueConverter
    {
        // Converte o valor booleano para o seu oposto
        // true → false, false → true
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        // ConvertBack também inverte (permite binding bidirecional)
        // Raramente usado, mas implementado para compatibilidade
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
