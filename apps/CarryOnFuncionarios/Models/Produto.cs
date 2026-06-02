namespace BarPedidosFuncionarios.Models
{

    // Usado dentro dos ItemPedido para mostrar detalhes dos produtos pedidos
    public class Produto
    {
  
        public string Id { get; set; }

        public string Nome { get; set; }

        public string Descricao { get; set; }

        public decimal Preco { get; set; }

        public string Categoria { get; set; }

        public bool Disponivel { get; set; }

        public string ImagemUrl { get; set; }

        // Construtor inicializa valores padrão
        public Produto()
        {
            Id = Guid.NewGuid().ToString();
            Disponivel = true;
        }
    }
}
