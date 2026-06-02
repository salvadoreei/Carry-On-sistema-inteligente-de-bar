using BarPedidosFuncionarios.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BarPedidos.ViewModels
{
    // ViewModel da página de definições
    // Controla o tema da aplicação (claro/escuro)
    // Sincroniza o switch na interface com o ThemeService
    public partial class SettingsViewModel : ObservableObject
    {
        // Serviço que gere os temas da aplicação
        private readonly IThemeService _themeService;

        // Indica se o modo escuro está ativo
        [ObservableProperty]
        private bool isDarkMode;

        // Ícone do tema atual (🌙 ou ☀️)
        // Exibido ao lado do switch
        [ObservableProperty]
        private string themeIcon;

        // Texto do status do tema ("Modo Escuro Ativo" ou "Modo Claro Ativo")
        // Exibido abaixo do switch
        [ObservableProperty]
        private string themeStatusText;

        // Construtor: recebe ThemeService por injeção de dependências
        public SettingsViewModel(IThemeService themeService)
        {
            _themeService = themeService;

            // Inicializa com o tema atual guardado
            // Lê do ThemeService o tema que estava ativo
            IsDarkMode = _themeService.IsDarkMode;
            UpdateThemeInfo();

            // Regista evento para escutar mudanças de tema
            // Se o tema mudar noutro local, esta página atualiza automaticamente
            _themeService.ThemeChanged += OnThemeChanged;
        }

        // Método gerado automaticamente quando IsDarkMode muda
        // Executado quando o utilizador alterna o switch na interface
        partial void OnIsDarkModeChanged(bool value)
        {
            // Aplica o novo tema através do ThemeService
            // ThemeService atualiza cores e guarda preferência
            _themeService.SetTheme(value);
            UpdateThemeInfo();
        }

        // Método executado quando o ThemeService notifica mudança
        // Garante sincronização se o tema mudar noutro local da app
        private void OnThemeChanged(object sender, bool isDark)
        {
            // Se o switch estiver dessincronizado, atualiza
            if (IsDarkMode != isDark)
            {
                IsDarkMode = isDark;
            }
            UpdateThemeInfo();
        }

        // Atualiza ícone e texto baseado no tema atual
        // Chamado sempre que o tema muda
        private void UpdateThemeInfo()
        {
            ThemeIcon = IsDarkMode ? "🌙" : "☀️";
            //ThemeStatusText = IsDarkMode ? "Modo Escuro Ativo" : "Modo Claro Ativo";
        }
    }
}
