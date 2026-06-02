using BarPedidos.ViewModels;

namespace BarPedidos.Views
{
    // Code-behind da página principal (catálogo de produtos)
    // Responsável por inicializar a página, carregar produtos e aplicar animações
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        // Flag que controla se é a primeira vez que a página aparece
        // Evita recarregar produtos do Firebase sempre que navega de volta
        private bool _primeiraVez = true;

        // Construtor recebe o ViewModel via Dependency Injection
        // Configura o BindingContext para ligar a View ao ViewModel
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        // Executado sempre que a página aparece no ecrã
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carrega produtos do Firebase apenas na primeira abertura da app
            // Nas próximas vezes que voltar a esta página, mantém produtos em memória
            if (_primeiraVez)
            {
                await _viewModel.CarregarProdutosAsync();
                _primeiraVez = false;
            }

            // Animação de fade in para transição suave (0 → 1 em 300ms)
            this.Opacity = 0;
            await this.FadeTo(1, 300);
        }
    }
}
