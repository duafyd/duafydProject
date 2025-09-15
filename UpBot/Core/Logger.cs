using System;
using System.IO;
using System.Text;
using Serilog;
using Serilog.Events;

namespace UpBot.Core;

public static class Logger
{
    static Logger()
    {
        const string logRoot = @"D:\Upbot\logs";
        Directory.CreateDirectory(logRoot);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: Path.Combine(logRoot, "log-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                encoding: Encoding.UTF8
            )
            .WriteTo.File(
                path: Path.Combine(logRoot, "log-err-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 60,
                shared: true,
                encoding: Encoding.UTF8,
                restrictedToMinimumLevel: LogEventLevel.Error
            )
            .CreateLogger();
    }

    public static void Debug(string message)
    {
        Log.Debug(message);
        System.Diagnostics.Debug.WriteLine(message);
    }
    public static void Info(string message) 
    {
        Log.Information(message);
        System.Diagnostics.Debug.WriteLine(message);
    }
    public static void Warn(string message) 
    {
        Log.Warning(message);
        System.Diagnostics.Debug.WriteLine(message);
    }
    public static void Error(string message, Exception? ex = null)
    {
        if (ex == null)
            Log.Error(message);
        else
            Log.Error(ex, message);

        System.Diagnostics.Debug.WriteLine(message);
    }

    public static void Close() => Log.CloseAndFlush();
}
