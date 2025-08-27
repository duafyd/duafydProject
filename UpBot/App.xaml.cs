using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UpBot.Services;

namespace UpBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            // 싱글턴 서비스 등록
            serviceCollection.AddSingleton<ApiService>();
            serviceCollection.AddSingleton<DatabaseService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}
