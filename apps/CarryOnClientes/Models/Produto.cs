namespace BarPedidos.Models
{
    // Representa um produto do catálogo (bebida, comida, sobremesa, etc)
    public class Produto
    {
        public string Id { get; set; }

        public string Nome { get; set; }

        public string Descricao { get; set; }

        public decimal Preco { get; set; }

        public string Categoria { get; set; }

        public string ImagemUrl { get; set; }

        public Produto()
        {
            // Gera ID único automaticamente ao criar produto novo
            Id = Guid.NewGuid().ToString();
        }

        // Preço formatado para mostrar na interface (ex: "€2,50")
        public string PrecoFormatado => $"€{Preco:F2}";
    }
}
