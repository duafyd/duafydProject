using CommunityToolkit.Mvvm.ComponentModel;
using UpBot.Services;

namespace UpBot.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(DatabaseService database)
    {
        database.Init();
    }
}