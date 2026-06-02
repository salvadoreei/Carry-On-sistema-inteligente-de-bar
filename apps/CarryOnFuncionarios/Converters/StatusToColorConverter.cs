using BarPedidosFuncionarios.Models;
using System.Globalization;

namespace BarPedidosFuncionarios.Converters
{
    // Converter que transforma StatusPedido enum em cores
    // Usado nos badges de status para dar cor de fundo conforme o estado do pedido
    // Cada status tem uma cor específica para identificação visual rápida pelo barman
    public class StatusToColorConverter : IValueConverter
    {
        // Recebe StatusPedido enum e retorna a cor correspondente
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusPedido status)
            {
                return status switch
                {
                    StatusPedido.Pendente => Color.FromArgb("#FF9800"),      // Laranja - pedido aguardando preparação
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
