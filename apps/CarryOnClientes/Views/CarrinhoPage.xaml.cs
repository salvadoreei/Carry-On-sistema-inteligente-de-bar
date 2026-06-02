using BarPedidos.ViewModels;

namespace BarPedidos.Views
{
    // Code-behind da página do carrinho
    // Responsável por inicializar a página e aplicar animações de entrada
    public partial class CarrinhoPage : ContentPage
    {
        private readonly CarrinhoViewModel _viewModel;

        // Construtor recebe o ViewModel via Dependency Injection
        // Configura o BindingContext para ligar a View ao ViewModel
        public CarrinhoPage(CarrinhoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        // Executado sempre que a página aparece no ecrã
        // Aplica animação de fade in suave (0 → 1 em 300ms)
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Animação de fade in para transição suave
            this.Opacity = 0;
            await this.FadeTo(1, 300);
        }
    }
}
