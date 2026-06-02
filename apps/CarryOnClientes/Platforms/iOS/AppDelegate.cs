using Foundation;

namespace bar_cliente
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => BarPedidos.MauiProgram.CreateMauiApp();
    }
}
