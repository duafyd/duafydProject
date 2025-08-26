namespace UpBot.Models.Api;

public class UpbitApiResponseBase
{
    public UpbitApiErrorResponse? Error { get; set; }
}

public class UpbitApiErrorResponse
{
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
