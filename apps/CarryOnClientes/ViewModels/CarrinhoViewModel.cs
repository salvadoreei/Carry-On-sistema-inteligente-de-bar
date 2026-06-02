using BarPedidos.Models;
using BarPedidos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BarPedidos.ViewModels
{
    // ViewModel que controla a página do carrinho de compras
    public partial class CarrinhoViewModel : ObservableObject
    {
        // Serviço que gere o carrinho (singleton partilhado)
        private readonly CarrinhoService _carrinhoService;

        // Serviço para comunicar com Firebase
        private readonly FirebaseService _firebaseService;

        // ObservableCollection: atualiza automaticamente a interface quando muda
        // Ligada diretamente à lista de itens na página XAML
        [ObservableProperty]
        private ObservableCollection<ItemCarrinho> itensCarrinho;

        // Total do carrinho em euros
        [ObservableProperty]
        private decimal totalCarrinho;

        // Número da mesa inserido pelo utilizador (texto)
        [ObservableProperty]
        private string numeroMesaTexto = "";

        // Converte o texto da mesa para número inteiro
        // Retorna 0 se não for um número válido
        private int NumeroMesa
        {
            get
            {
                if (int.TryParse(numeroMesaTexto, out int numero))
                    return numero;
                return 0;
            }
        }

        // Observações opcionais do pedido
        [ObservableProperty]
        private string observacoesPedido;

        // Flag que indica se o carrinho está vazio
        // Usada para mostrar/ocultar mensagem "Carrinho vazio"
        [ObservableProperty]
        private bool carrinhoVazio;

        // Eventos para controlar animações na interface
        // Disparados antes de remover itens ou limpar carrinho
        public event EventHandler<ItemCarrinho> ItemRemovendo;
        public event EventHandler CarrinhoLimpando;

        // Construtor: recebe serviços por injeção de dependências
        public CarrinhoViewModel(DeviceService deviceService, FirebaseService firebaseService, CarrinhoService carrinhoService)
        {
            _carrinhoService = carrinhoService;
            _firebaseService = firebaseService;

            // Usa a mesma ObservableCollection do serviço (singleton)
            // Assim, mudanças em qualquer lugar atualizam automaticamente
            ItensCarrinho = _carrinhoService.Itens;

            // Calcula o total inicial
            AtualizarTotal();

            //Recalcula total quando carrinho muda
            _carrinhoService.CarrinhoAtualizado += (s, e) => AtualizarTotal();
        }

        // Aumenta a quantidade de um item em 1 unidade
        // RelayCommand: permite ligar este método a botões no XAML
        [RelayCommand]
        public void AumentarQuantidade(ItemCarrinho item)
        {
            _carrinhoService.AtualizarQuantidade(item, item.Quantidade + 1);
        }

        // Diminui a quantidade de um item em 1 unidade
        // Se quantidade chegar a 0, o item é removido automaticamente
        [RelayCommand]
        public void DiminuirQuantidade(ItemCarrinho item)
        {
            _carrinhoService.AtualizarQuantidade(item, item.Quantidade - 1);
        }

        // Remove um item do carrinho após confirmação
        // Dispara animação antes de remover
        [RelayCommand]
        public async Task RemoverItemAsync(ItemCarrinho item)
        {
            // Pede confirmação ao utilizador
            bool confirmar = await Shell.Current.DisplayAlert(
                "Confirmar",
                $"Remover {item.Produto.Nome} do carrinho?",
                "Sim",
                "Não");

            if (!confirmar)
                return;

            // Dispara evento para animação na interface (fade out)
            ItemRemovendo?.Invoke(this, item);

            // Aguarda 500ms para a animação completar
            await Task.Delay(500);

            // Remove o item do serviço
            _carrinhoService.RemoverItem(item);
            AtualizarTotal();
        }

        // Finaliza o pedido e envia para Firebase
        // Valida dados, cria objeto Pedido e envia para base de dados
        [RelayCommand]
        public async Task FinalizarPedidoAsync()
        {
            // Validação 1: verifica se o carrinho tem itens
            if (ItensCarrinho.Count == 0)
            {
                await Shell.Current.DisplayAlert("Aviso", "O carrinho está vazio!", "OK");
                return;
            }

            // Validação 2: verifica se número da mesa foi informado e é válido
            if (string.IsNullOrWhiteSpace(NumeroMesaTexto) || NumeroMesa <= 0)
            {
                await Shell.Current.DisplayAlert("Aviso", "Por favor, informe o número da mesa!", "OK");
                return;
            }

            // Pede confirmação final com resumo do pedido
            bool confirmar = await Shell.Current.DisplayAlert(
                "Confirmar Pedido",
                $"Finalizar pedido no valor de {TotalCarrinho:F2}€ para a mesa {NumeroMesa}?",
                "Sim",
                "Não");

            if (!confirmar) return;

            try
            {
                // Cria objeto Pedido com todos os dados
                var pedido = new Pedido
                {
                    NumeroMesa = NumeroMesa,
                    Observacoes = ObservacoesPedido,
                    Itens = ItensCarrinho.ToList(), // Converte para lista fixa
                    Total = TotalCarrinho,
                    DataHora = DateTime.Now,
                    Status = StatusPedido.Pendente // Status inicial
                };

                // Envia pedido para Firebase
                // DeviceId é adicionado automaticamente no FirebaseService
                bool sucesso = await _firebaseService.CriarPedidoAsync(pedido);

                if (sucesso)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Pedido realizado com sucesso!", "OK");

                    // Limpa carrinho após pedido bem-sucedido
                    _carrinhoService.LimparCarrinho();
                    LimparFormulario();
                    AtualizarTotal();

                    // Volta para a página principal (menu)
                    await Shell.Current.GoToAsync("//PedidosPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Erro ao realizar pedido. Tente novamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao finalizar pedido: {ex.Message}", "OK");
            }
        }

        // Limpa todos os itens do carrinho após confirmação
        // Dispara animação antes de limpar
        [RelayCommand]
        public async Task LimparCarrinhoAsync()
        {
            if (ItensCarrinho.Count == 0) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Confirmar",
                "Limpar todos os itens do carrinho?",
                "Sim",
                "Não");

            if (!confirmar)
                return;

            // Dispara evento para animação de limpeza
            CarrinhoLimpando?.Invoke(this, EventArgs.Empty);
            await Task.Delay(600);

            // Limpa carrinho, formulário e recalcula total
            _carrinhoService.LimparCarrinho();
            LimparFormulario();
            AtualizarTotal();
        }

        // Recalcula o total do carrinho e atualiza flag de vazio
        // Chamado sempre que o carrinho muda
        private void AtualizarTotal()
        {
            TotalCarrinho = _carrinhoService.Total;
            CarrinhoVazio = ItensCarrinho == null || ItensCarrinho.Count == 0;
        }

        // Limpa os campos do formulário (mesa e observações)
        private void LimparFormulario()
        {
            NumeroMesaTexto = "";
            ObservacoesPedido = string.Empty;
        }
    }
}
