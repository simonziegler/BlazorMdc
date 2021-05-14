using Microsoft.Extensions.Logging;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// The internal implementation of <see cref="IMBLoggingService"/>.
    /// </summary>
    internal class LoggingService : IMBLoggingService
    {
        public MBLoggingServiceConfiguration Configuration { get; set; }
        private ILogger<ComponentFoundation> Logger { get; set; } = null;

        public LoggingService(MBLoggingServiceConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// CurrentLevel returns the logging level (as an int to make comparisons easier)
        /// </summary>
        /// <returns></returns>
        public int CurrentLevel()
        {
            return (int)Configuration.LoggingLevel;
        }

        /// <summary>
        /// Logs a Critical message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogCritical(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Critical) && (Logger != null))
            {
                Logger.LogCritical("Material.Blazor Critical: " + message);
            }
        }

        /// <summary>
        /// Logs an Error message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogError(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Error) && (Logger != null))
            {
                Logger.LogError("Material.Blazor Error: " + message);
            }
        }

        /// <summary>
        /// Logs a Warning message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogWarning(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Warning) && (Logger != null))
            {
                Logger.LogWarning("Material.Blazor Warning: " + message);
            }
        }

        /// <summary>
        /// Logs an Information message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogInformation(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Information) && (Logger != null))
            {
                Logger.LogInformation("Material.Blazor Information: " + message);
            }
        }

        /// <summary>
        /// Logs a Debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogDebug(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Debug) && (Logger != null))
            {
                Logger.LogDebug("Material.Blazor Debug: " + message);
            }
        }

        /// <summary>
        /// Logs a Trace message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Log message</param>
        public void LogTrace(string message)
        {
            if ((CurrentLevel() <= (int)MBLoggingLevel.Trace) && (Logger != null))
            {
                Logger.LogTrace("Material.Blazor Trace: " + message);
            }
        }


        public void SetLogger(ILogger<ComponentFoundation> logger)
        {
            if (Logger == null)
            {
                Logger = logger;
            }
        }
    }
}
