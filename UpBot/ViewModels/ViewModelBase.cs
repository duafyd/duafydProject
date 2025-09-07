using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using UpBot.Services;

namespace UpBot.ViewModels;

public class ViewModelBase : ObservableRecipient
{
    public ApiService Api { get; }
    public Bot Bot { get; }
    public AppDataService AppData { get; }

    public ViewModelBase()
    {
        Api = App.ServiceProvider.GetRequiredService<ApiService>();
        Bot = App.ServiceProvider.GetRequiredService<Bot>();
        AppData = App.ServiceProvider.GetRequiredService<AppDataService>();
    }
}
