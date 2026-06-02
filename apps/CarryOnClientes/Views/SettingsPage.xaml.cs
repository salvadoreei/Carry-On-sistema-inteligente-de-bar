using BarPedidos.ViewModels;

namespace BarPedidos.Views
{
    // Code-behind da página de definições
    // Responsável por inicializar a página e aplicar animações
    public partial class SettingsPage : ContentPage
    {
        // Construtor recebe o ViewModel via Dependency Injection
        // Configura o BindingContext para ligar a View ao ViewModel
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Executado sempre que a página aparece no ecrã
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Animação de fade in para transição suave (0 → 1 em 300ms)
            this.Opacity = 0;
            await this.FadeTo(1, 300);
        }
    }
}
