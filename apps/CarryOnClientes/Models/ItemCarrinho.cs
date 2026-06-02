using System.ComponentModel;

namespace BarPedidos.Models
{
    // Representa um item dentro do carrinho de compras
    // Contém produto, quantidade e observações do cliente
    // Implementa INotifyPropertyChanged para atualizar interface automaticamente
    public class ItemCarrinho : INotifyPropertyChanged
    {
        private int _quantidade;

        public string Id { get; set; }

        public Produto Produto { get; set; }

        // Quantidade de unidades deste produto
        public int Quantidade
        {
            get => _quantidade;
            set
            {
                if (_quantidade != value)
                {
                    _quantidade = value;
                    // Avisa que Quantidade mudou
                    OnPropertyChanged(nameof(Quantidade));
                    // Avisa que Subtotal também mudou por causa da quantidade
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        // Calcula preço total deste item (Preço x Quantidade)
        public decimal Subtotal => Produto?.Preco * Quantidade ?? 0;

        public string Observacoes { get; set; }

        // Evento que avisa a interface quando propriedades mudam
        public event PropertyChangedEventHandler PropertyChanged;

        // Dispara evento para atualizar interface
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ItemCarrinho()
        {
            // Gera ID único para este item
            Id = Guid.NewGuid().ToString();

            Quantidade = 1;
        }
    }
}
