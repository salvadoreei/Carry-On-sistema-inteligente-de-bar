using BarPedidosFuncionarios.Models;
using System.Globalization;

namespace BarPedidosFuncionarios.Converters
{
    // Usado nos badges de status para exibir o nome do estado em português
    // Exemplo: StatusPedido.EmPreparacao → "Em Preparação"
    public class StatusToTextConverter : IValueConverter
    {
        // Recebe StatusPedido enum e retorna o texto formatado
        // Converte nomes técnicos do código para português legível
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusPedido status)
            {
                return status switch
                {
                    StatusPedido.Pendente => "Pendente",
                    StatusPedido.EmPreparacao => "Em Preparação",  // Adiciona espaço e acento
                    StatusPedido.Pronto => "Pronto",
                    StatusPedido.Entregue => "Entregue",
                    StatusPedido.Pago => "Pago",
                    StatusPedido.Cancelado => "Cancelado",
                    _ => status.ToString()  // Fallback: usa nome do enum se não reconhecido
                };
            }
            return value?.ToString() ?? "Desconhecido";
        }

        // ConvertBack não é necessário (converter unidirecional)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
