using BarPedidosFuncionarios.Services;

namespace BarPedidosFuncionarios
{
    // Classe principal da aplicação de funcionários
    // Responsável por inicializar o tema e a estrutura de navegação
    public partial class App : Application
    {
        // Construtor recebe ThemeService via Dependency Injection
        // themeService: gere tema claro/escuro
        public App(IThemeService themeService)
        {
            InitializeComponent();

            // Carrega tema guardado nas preferências (claro ou escuro)
            themeService.InitializeTheme();

            // Define AppShell como página principal (navegação com tabs)
            MainPage = new AppShell();
        }
    }
}
