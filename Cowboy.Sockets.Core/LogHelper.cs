using Serilog;

namespace Cowboy.Sockets.Core
{
    public class LogHelper
    {
        public static ILogger Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}