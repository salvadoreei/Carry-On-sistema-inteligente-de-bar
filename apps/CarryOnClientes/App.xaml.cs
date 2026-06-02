using BarPedidos.Services;

namespace BarPedidos
{
    // Classe principal da aplicação
    // Responsável por inicializar serviços globais (tema e limpeza automática de pedidos)
    public partial class App : Application
    {
        private readonly PedidoCleanupService _cleanupService;

        // Construtor recebe serviços via Dependency Injection
        public App(PedidoCleanupService cleanupService, IThemeService themeService)
        {
            InitializeComponent();

            _cleanupService = cleanupService;

            // Carrega tema guardado nas preferências (claro ou escuro)
            themeService.InitializeTheme();

            // Define AppShell como página principal (navegação com tabs)
            MainPage = new AppShell();

            // Inicia serviço de limpeza automática (verifica a cada 30 segundos)
            _cleanupService.Start();
        }

        // Executado quando app vai para segundo plano (minimizada)
        // Para o serviço de limpeza para poupar recursos
        protected override void OnSleep()
        {
            base.OnSleep();
            _cleanupService.Stop();
        }

        // Executado quando app volta a primeiro plano (reaberta)
        // Reinicia o serviço de limpeza
        protected override void OnResume()
        {
            base.OnResume();
            _cleanupService.Start();
        }
    }
}
