namespace BarPedidosFuncionarios.Models
{
    // Enum que define os estados possíveis de um pedido
    // Os valores numéricos facilitam a ordenação e serialização no Firebase
    public enum StatusPedido
    {
        Pendente = 0,
        EmPreparacao = 1,
        Pronto = 2,
        Entregue = 3,
        Pago = 4,
        Cancelado = 5
    }

    // Modelo que representa um pedido completo
    // Contém todas as informações necessárias para o barman gerir o pedido
    public class Pedido
    {
        public string Id { get; set; }

        public int NumeroMesa { get; set; }

        public string DeviceId { get; set; }

        public StatusPedido Status { get; set; }

        public DateTime DataHora { get; set; }

        public DateTime? DataPagamento { get; set; }

        public DateTime? DataCancelamento { get; set; }

        public string Observacoes { get; set; }

        public decimal Total { get; set; }

        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}
