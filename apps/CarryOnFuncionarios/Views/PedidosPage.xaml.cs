using BarPedidosFuncionarios.ViewModels;

namespace BarPedidosFuncionarios.Views
{
    // Code-behind da página de gestão de pedidos (barman)
    // Responsável por inicializar a página, carregar pedidos e aplicar animações
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

            // Carrega todos os pedidos ativos do Firebase
            // Executado sempre que barman volta a esta página para garantir lista atualizada
            if (BindingContext is PedidosViewModel vm)
            {
                vm.CarregarPedidosCommand.Execute(null);
            }

            // Animação de fade in para transição suave (0 → 1 em 300ms)
            this.Opacity = 0;
            await this.FadeTo(1, 300);
        }
    }
}
