using Microsoft.Extensions.DependencyInjection;

namespace UpBot.Services.Apis;

public class ApiBase
{
    private ApiService _api;
    public ApiService Api => _api ?? InitApi();
    public ApiBase()
    {
       
    }

    private ApiService InitApi()
    {
        _api = App.ServiceProvider.GetRequiredService<ApiService>();
        return _api;
    }
}
