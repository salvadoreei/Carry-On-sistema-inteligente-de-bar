using BarPedidos.Models;
using BarPedidos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BarPedidos.ViewModels
{
    // ViewModel da página principal (catálogo de produtos)
    // Gere listagem, filtros, pesquisa e adição ao carrinho
    public partial class MainViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private readonly CarrinhoService _carrinhoService;

        // Lista completa de produtos carregados do Firebase
        [ObservableProperty]
        private ObservableCollection<Produto> produtos;

        // Lista filtrada exibida na interface (categoria + pesquisa)
        [ObservableProperty]
        private ObservableCollection<Produto> produtosFiltrados;

        // Categoria atualmente selecionada
        [ObservableProperty]
        private string categoriaSelecionada;

        // Quantidade total de itens no carrinho (badge no ícone)
        [ObservableProperty]
        private int quantidadeCarrinho;

        // Indica se está a carregar produtos (mostra loading)
        [ObservableProperty]
        private bool isLoading;

        // Texto na barra de pesquisa
        [ObservableProperty]
        private string textoPesquisa;

        // Lista de categorias disponíveis (botões de filtro)
        public ObservableCollection<string> Categorias { get; set; }

        public MainViewModel(DeviceService deviceService, FirebaseService firebaseService, CarrinhoService carrinhoService)
        {
            _firebaseService = firebaseService;
            _carrinhoService = carrinhoService;

            Produtos = new ObservableCollection<Produto>();
            ProdutosFiltrados = new ObservableCollection<Produto>();
            Categorias = new ObservableCollection<string> { "Bebidas", "Petiscos", "Refeições", "Sobremesas" };

            CategoriaSelecionada = "Bebidas";

            // Observa mudanças no carrinho para atualizar badge
            _carrinhoService.CarrinhoAtualizado += (s, e) =>
            {
                QuantidadeCarrinho = _carrinhoService.QuantidadeTotal;
            };
        }

        // Carrega produtos do Firebase e filtra por disponíveis
        [RelayCommand]
        public async Task CarregarProdutosAsync()
        {
            IsLoading = true;
            try
            {
                var produtosFirebase = await _firebaseService.ObterProdutosAsync();

                Produtos.Clear();

                // Carregar Firebase
                foreach (var produto in produtosFirebase)
                {
                    Produtos.Add(produto);
                }

                CategoriaSelecionada = "Bebidas";

                FiltrarProdutos();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar produtos: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Adiciona produto ao carrinho com animação de sucesso
        [RelayCommand]
        public async Task AdicionarAoCarrinho(Produto produto)
        {
            // Adiciona ao carrinho
            _carrinhoService.AdicionarItem(produto);

            // Anima o botão em paralelo com o alert
            _ = Task.Run(async () =>
            {
                await Task.Delay(50); // Pequeno delay para VisualState resetar
                var botao = await EncontrarBotaoDoContexto(produto);
                if (botao != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await AnimarBotaoSucesso(botao);
                    });
                }
            });

            await Shell.Current.DisplayAlert("Sucesso", $"{produto.Nome} adicionado ao carrinho!", "OK");
        }

        // Encontra o botão específico que foi clicado
        // Procura na árvore visual pelo botão cujo Frame tem o BindingContext correto
        private async Task<Button> EncontrarBotaoDoContexto(Produto produto)
        {
            try
            {
                var mainPage = Application.Current?.MainPage;
                if (mainPage == null) return null;

                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage == null) return null;

                // Obtém todos os botões com texto "Adicionar ao Carrinho"
                var botoes = currentPage.GetVisualTreeDescendants()
                    .OfType<Button>()
                    .Where(b => b.Text == "Adicionar ao Carrinho");

                // Encontra o botão cujo Frame pai tem o produto correto no BindingContext
                foreach (var botao in botoes)
                {
                    var frame = botao.Parent;
                    while (frame != null && frame is not Frame)
                    {
                        frame = frame.Parent;
                    }

                    if (frame?.BindingContext == produto)
                    {
                        return botao;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Anima o botão com efeito de sucesso (verde)
        private async Task AnimarBotaoSucesso(Button botao)
        {
            try
            {
                var corOriginal = botao.BackgroundColor;

                // Fase 1: aumenta tamanho e muda para verde
                await Task.WhenAll(
                    botao.ScaleTo(1.15, 200, Easing.CubicOut),
                    Task.Run(async () =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            botao.BackgroundColor = Color.FromArgb("#10B981"); // Verde
                        });
                    })
                );

                // Fase 2: volta ao tamanho normal com efeito bounce
                await botao.ScaleTo(1, 300, Easing.BounceOut);

                await Task.Delay(200);

                // Volta à cor original
                botao.BackgroundColor = corOriginal;
            }
            catch
            {
                // Ignora erros de animação (não é crítico)
            }
        }

        // Navega para a página do carrinho
        [RelayCommand]
        public async Task IrParaCarrinhoAsync()
        {
            await Shell.Current.GoToAsync("//CarrinhoPage");
        }

        // Filtra produtos pela categoria selecionada
        [RelayCommand]
        public void FiltrarPorCategoria(string categoria)
        {
            CategoriaSelecionada = categoria;
            FiltrarProdutos();
        }

        // Método gerado automaticamente quando TextoPesquisa muda
        // Refiltra produtos sempre que o utilizador digita
        partial void OnTextoPesquisaChanged(string value)
        {
            FiltrarProdutos();
        }

        // Aplica filtros de categoria e pesquisa aos produtos
        // Atualiza ProdutosFiltrados que é exibida na interface
        private void FiltrarProdutos()
        {
            ProdutosFiltrados.Clear();

            var produtosFiltro = Produtos.AsEnumerable();

            // Filtro 1: Categoria
            if (!string.IsNullOrEmpty(CategoriaSelecionada))
            {
                produtosFiltro = produtosFiltro.Where(p => p.Categoria == CategoriaSelecionada);
            }

            // Filtro 2: Pesquisa (procura em nome e descrição, case-insensitive)
            if (!string.IsNullOrEmpty(TextoPesquisa))
            {
                produtosFiltro = produtosFiltro.Where(p =>
                    p.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                    p.Descricao.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase));
            }

            // Adiciona produtos filtrados à coleção exibida
            foreach (var produto in produtosFiltro)
            {
                ProdutosFiltrados.Add(produto);
            }
        }

        // Limpa o texto da pesquisa
        [RelayCommand]
        private void LimparPesquisa()
        {
            TextoPesquisa = string.Empty;
        }
    }
}
