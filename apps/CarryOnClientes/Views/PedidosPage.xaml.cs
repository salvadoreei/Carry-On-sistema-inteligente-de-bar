using BarPedidos.ViewModels;

namespace BarPedidos.Views
{
    // Code-behind da página de pedidos
    // Responsável por inicializar a página, carregar pedidos do Firebase e aplicar animações
    public partial class PedidosPage : ContentPage
    {
        private readonly PedidosViewModel _viewModel;

        // Construtor recebe o ViewModel via Dependency Injection
        // Configura o BindingContext para ligar a View ao ViewModel
        public PedidosPage(PedidosViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        // Executado sempre que a página aparece no ecrã
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carrega pedidos do Firebase sempre que volta a esta página
            // Garante que a lista está atualizada com o status mais recente
            await _viewModel.CarregarPedidosAsync();

            // Animação de fade in para transição suave (0 → 1 em 300ms)
            this.Opacity = 0;
            await this.FadeTo(1, 300);
        }
    }
}