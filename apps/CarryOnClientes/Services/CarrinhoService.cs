using BarPedidos.Models;
using System.Collections.ObjectModel;

namespace BarPedidos.Services
{
    // Serviço que gere o carrinho de compras da aplicação
    // Implementa padrão Singleton: só existe UMA instância em toda a app
    // Guarda produtos escolhidos antes de criar o pedido final
    public class CarrinhoService
    {
        // Instância única do serviço (Singleton)
        private static CarrinhoService _instance;

        // Acesso público à instância única
        // Se não existir, cria automaticamente
        public static CarrinhoService Instance => _instance ??= new CarrinhoService();

        // Lista de itens no carrinho
        // ObservableCollection atualiza interface automaticamente quando muda
        public ObservableCollection<ItemCarrinho> Itens { get; private set; }

        // Calcula valor total do carrinho (soma de todos os subtotais)
        public decimal Total => Itens.Sum(i => i.Subtotal);

        // Calcula quantidade total de produtos no carrinho
        // Usado para mostrar badge no ícone do carrinho
        public int QuantidadeTotal => Itens.Sum(i => i.Quantidade);

        // Evento disparado sempre que carrinho é modificado
        // Permite que outras partes da app reajam a mudanças
        public event EventHandler CarrinhoAtualizado;

        // Construtor privado: garante que só Instance pode criar objetos
        private CarrinhoService()
        {
            Itens = new ObservableCollection<ItemCarrinho>();
        }

        // Adiciona produto ao carrinho
        // Se produto já existe, aumenta quantidade
        // Se é novo, cria item novo
        public void AdicionarItem(Produto produto, int quantidade = 1, string observacoes = "")
        {
            // Procura se produto já está no carrinho (pelo ID)
            var itemExistente = Itens.FirstOrDefault(i => i.Produto.Id == produto.Id);

            if (itemExistente != null)
            {
                // Produto já existe: aumenta quantidade
                itemExistente.Quantidade += quantidade;
            }
            else
            {
                // Produto novo: cria e adiciona ao carrinho
                var novoItem = new ItemCarrinho
                {
                    Produto = produto,
                    Quantidade = quantidade,
                    Observacoes = observacoes
                };
                Itens.Add(novoItem);
            }

            NotificarAtualizacao();
        }

        // Remove item do carrinho completamente
        public void RemoverItem(ItemCarrinho item)
        {
            Itens.Remove(item);
            NotificarAtualizacao();
        }

        // Atualiza quantidade de um item específico
        // Se quantidade <= 0, remove o item
        public void AtualizarQuantidade(ItemCarrinho item, int novaQuantidade)
        {
            if (novaQuantidade <= 0)
            {
                // Quantidade zero ou negativa: remove item
                RemoverItem(item);
            }
            else
            {
                // Atualiza quantidade
                item.Quantidade = novaQuantidade;
                NotificarAtualizacao();
            }
        }

        // Esvazia carrinho completamente
        // Usado após criar pedido com sucesso
        public void LimparCarrinho()
        {
            Itens.Clear();
            NotificarAtualizacao();
        }

        // Dispara evento CarrinhoAtualizado
        // Faz interface atualizar badge, total, etc
        private void NotificarAtualizacao()
        {
            CarrinhoAtualizado?.Invoke(this, EventArgs.Empty);
        }
    }
}
