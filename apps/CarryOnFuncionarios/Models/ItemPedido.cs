namespace BarPedidosFuncionarios.Models
{
    // Modelo que representa um item individual dentro de um pedido
    // Associa um produto à quantidade pedida e calcula o subtotal
    public class ItemPedido
    {
        public Produto Produto { get; set; }

        public int Quantidade { get; set; }

        public decimal Subtotal { get; set; }
    }
}
