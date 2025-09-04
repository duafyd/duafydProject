using Serilog;

namespace UpBot.Core;

public static class Logger
{
    static Logger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                encoding: System.Text.Encoding.UTF8
            )
            .CreateLogger();
    }

    public static void Debug(string message) => Log.Debug(message);
    public static void Info(string message) => Log.Information(message);
    public static void Warn(string message) => Log.Warning(message);
    public static void Error(string message, Exception? ex = null)
    {
        if (ex == null)
            Log.Error(message);
        else
            Log.Error(ex, message);
    }

    public static void Close() => Log.CloseAndFlush();
}
