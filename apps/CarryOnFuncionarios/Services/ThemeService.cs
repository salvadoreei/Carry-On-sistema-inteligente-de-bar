namespace BarPedidosFuncionarios.Services
{
    // Interface que define o contrato do ThemeService
    // Garante que o serviço implemente os métodos necessários
    public interface IThemeService
    {
        bool IsDarkMode { get; }
        event EventHandler<bool> ThemeChanged;
        void SetTheme(bool isDarkMode);
        void ToggleTheme();
        void InitializeTheme();
    }

    // Serviço que gere os temas da aplicação (claro/escuro)
    // Controla cores da interface e guarda a preferência do utilizador
    public class ThemeService : IThemeService
    {
        private bool _isDarkMode;

        // Propriedade pública que expõe o estado atual do tema
        public bool IsDarkMode => _isDarkMode;

        // Evento que notifica outras partes da app quando o tema muda
        public event EventHandler<bool> ThemeChanged;

        // Construtor: carrega automaticamente o tema guardado
        public ThemeService()
        {
            InitializeTheme();
        }

        // Inicializa o tema ao abrir a aplicação
        // Lê a preferência guardada ou usa "Light" como padrão
        public void InitializeTheme()
        {
            var savedTheme = Preferences.Get("AppTheme", "Light");
            SetTheme(savedTheme == "Dark");
        }

        // Define o tema da aplicação (claro ou escuro)
        public void SetTheme(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;

            if (Application.Current != null)
            {
                // Define o tema do sistema operativo
                Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

                // Obtém o dicionário de recursos onde ficam guardadas as cores
                var resources = Application.Current.Resources;

                // TEMA ESCURO: aplica cores escuras
                if (isDarkMode)
                {
                    resources["PageBackground"] = Color.FromArgb("#1C1C1E");
                    resources["CardBackground"] = Color.FromArgb("#3A3A3C");
                    resources["TextPrimary"] = Color.FromArgb("#FFFFFF");
                    resources["TextSecondary"] = Color.FromArgb("#AEAEB2");
                    resources["BorderColor"] = Color.FromArgb("#48484A");
                }
                // TEMA CLARO: aplica cores claras
                else
                {
                    resources["PageBackground"] = Color.FromArgb("#FFFFFF");
                    resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                    resources["TextPrimary"] = Color.FromArgb("#212529");
                    resources["TextSecondary"] = Color.FromArgb("#6C757D");
                    resources["BorderColor"] = Color.FromArgb("#DEE2E6");
                }
            }

            // Guarda a preferência no dispositivo para manter entre sessões
            Preferences.Set("AppTheme", isDarkMode ? "Dark" : "Light");

            // Dispara evento para notificar mudança (ex: atualizar switch)
            ThemeChanged?.Invoke(this, isDarkMode);
        }

        // Alterna entre tema claro e escuro (inverte o estado atual)
        public void ToggleTheme()
        {
            SetTheme(!_isDarkMode);
        }
    }
}
