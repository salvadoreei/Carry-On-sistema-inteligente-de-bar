namespace BarPedidos.Models
{
    // Representa um pedido completo feito pelo cliente
    public class Pedido
    {
        public string Id { get; set; }

        public string DeviceId { get; set; }

        public int NumeroMesa { get; set; }

        public List<ItemCarrinho> Itens { get; set; }

        public decimal Total { get; set; }

        public DateTime DataHora { get; set; }

        public StatusPedido Status { get; set; }

        public string Observacoes { get; set; }

        public DateTime? DataPagamento { get; set; }

        public DateTime? DataCancelamento { get; set; }

        public Pedido()
        {
            // Gera ID único automaticamente
            Id = Guid.NewGuid().ToString();

            // Inicializa lista vazia de itens
            Itens = new List<ItemCarrinho>();

            // Marca hora atual
            DataHora = DateTime.Now;

            Status = StatusPedido.Pendente;
        }
    }

    // Estados possíveis de um pedido
    public enum StatusPedido
    {
        Pendente,       // Cliente acabou de fazer pedido, barman ainda não viu
        EmPreparacao,   // Barman está a preparar o pedido
        Pronto,         // Pedido pronto, aguarda entrega à mesa
        Entregue,       // Pedido já foi entregue ao cliente
        Pago,           // Cliente já pagou a conta
        Cancelado       // Pedido foi cancelado (por cliente ou barman)
    }
}
