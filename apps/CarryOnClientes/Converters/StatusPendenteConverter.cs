using BarPedidos.Models;
using System.Globalization;

namespace BarPedidos.Converters
{
    // Converter que verifica se um pedido está com status Pendente
    // Retorna true se Status = Pendente, false caso contrário
    // Usado para mostrar/esconder o botão "Cancelar Pedido"
    public class StatusPendenteConverter : IValueConverter
    {
        // Recebe um StatusPedido enum e retorna true se for Pendente
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusPedido status)
            {
                return status == StatusPedido.Pendente;
            }
            return false;
        }

        // ConvertBack não é necessário (converter unidirecional)
        // Lança exceção se tentarem usar binding bidirecional
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
