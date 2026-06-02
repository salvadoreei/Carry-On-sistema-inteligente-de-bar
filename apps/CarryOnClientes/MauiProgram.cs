using BarPedidos.Services;
using BarPedidos.ViewModels;
using BarPedidos.Views;
using Microsoft.Extensions.Logging;

namespace BarPedidos
{
    // Classe de configuração da aplicação MAUI
    // Responsável por configurar Dependency Injection e registar todos os serviços, ViewModels e Views
    // Executado uma vez ao iniciar a aplicação
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
            builder.Services.AddSingleton<DeviceService>();
            builder.Services.AddSingleton<FirebaseService>();
            builder.Services.AddSingleton<PedidoCleanupService>();
            builder.Services.AddSingleton<CarrinhoService>(CarrinhoService.Instance);
            builder.Services.AddSingleton<IThemeService, ThemeService>();

            // Registar ViewModels como Transient (nova instância criada sempre que necessário)
            // Cada navegação para uma página cria um ViewModel novo
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<CarrinhoViewModel>();
            builder.Services.AddTransient<PedidosViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Registar Views como Transient (nova instância criada sempre que necessário)
            // Permite Dependency Injection nos construtores das Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CarrinhoPage>();
            builder.Services.AddTransient<PedidosPage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}
