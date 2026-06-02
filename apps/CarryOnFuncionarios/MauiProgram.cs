using BarPedidos.ViewModels;
using BarPedidosFuncionarios.Services;
using BarPedidosFuncionarios.ViewModels;
using BarPedidosFuncionarios.Views;
using Microsoft.Extensions.Logging;

namespace BarPedidosFuncionarios
{
    // Classe de configuração da aplicação MAUI de funcionários
    // Responsável por configurar Dependency Injection e registar serviços, ViewModels e Views
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // Regista fontes personalizadas usadas na app
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            // Adiciona logging para debug (apenas em modo desenvolvimento)
            builder.Logging.AddDebug();
#endif

            // Registar Serviços como Singleton (uma única instância partilhada)
            // FirebaseService: comunica com Firebase Realtime Database
            builder.Services.AddSingleton<FirebaseService>();
            // ThemeService: gere tema claro/escuro da aplicação
            builder.Services.AddSingleton<IThemeService, ThemeService>();

            // Registar ViewModels como Transient (nova instância criada sempre que necessário)
            // PedidosViewModel: gere lista de todos os pedidos ativos
            builder.Services.AddTransient<PedidosViewModel>();
            // SettingsViewModel: gere definições do tema
            builder.Services.AddTransient<SettingsViewModel>();

            // Registar Views como Transient (nova instância criada sempre que necessário)
            // Permite Dependency Injection nos construtores das Views
            builder.Services.AddTransient<PedidosPage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}
