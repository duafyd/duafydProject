using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UpBot.Core;
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
            serviceCollection.AddSingleton<Bot>();
            serviceCollection.AddSingleton<AppDataService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            Logger.Info("Application Starting");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("Application Exiting");
            Logger.Close();

            base.OnExit(e);
        }
    }
}
