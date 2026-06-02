using System.Globalization;

namespace BarPedidos.Converters
{
    // Converter que verifica se uma string tem conteúdo
    // Retorna true se string não está vazia/nula, false caso contrário
    // Usado para mostrar/esconder o botão "X" de limpar pesquisa (só aparece quando há texto)
    public class StringNotEmptyConverter : IValueConverter
    {
        // Recebe uma string e retorna true se tiver conteúdo
        // Verifica null, vazio ("") e apenas espaços em branco
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        // ConvertBack não é necessário (converter unidirecional)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
