using BarPedidos.Models;
using System.Globalization;

namespace BarPedidos.Converters
{
    // Converter que transforma StatusPedido enum em cores
    // Usado em cada status de pedido para mostrar uma cor diferente na interface
    // Cada status tem uma cor específica para identificação visual rápida
    public class StatusToColorConverter : IValueConverter
    {
        // Recebe StatusPedido enum e retorna a cor correspondente
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusPedido status)
            {
                return status switch
                {
                    StatusPedido.Pendente => Color.FromArgb("#FF9800"),      // Laranja - pedido aguardando
                    StatusPedido.EmPreparacao => Color.FromArgb("#2196F3"),  // Azul - a ser preparado
                    StatusPedido.Pronto => Color.FromArgb("#9C27B0"),        // Roxo - pronto para servir 
                    StatusPedido.Entregue => Color.FromArgb("#4CAF50"),      // Verde - entregue ao cliente
                    StatusPedido.Pago => Color.FromArgb("#4CAF50"),          // Verde - pedido pago
                    StatusPedido.Cancelado => Color.FromArgb("#F44336"),     // Vermelho - cancelado
                    _ => Colors.Gray                                          // Cinzento - fallback para status desconhecido
                };
            }
            return Colors.Gray;
        }

        // ConvertBack não é necessário (converter unidirecional)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
