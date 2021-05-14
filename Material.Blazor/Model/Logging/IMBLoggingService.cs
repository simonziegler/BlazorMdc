using Material.Blazor.Internal;

using Microsoft.Extensions.Logging;

namespace Material.Blazor
{
    /// <summary>
    /// Interface for the Material.Blazor logging level service
    /// 
    public interface IMBLoggingService
    {
        /// <summary>
        /// Logging service configuration
        /// </summary>
        MBLoggingServiceConfiguration Configuration { get; set; }

        public int CurrentLevel();

        public void LogCritical(string message);
        public void LogError(string message);
        public void LogWarning(string message);
        public void LogInformation(string message);
        public void LogDebug(string message);
        public void LogTrace(string message);

        public void SetLogger(ILogger<ComponentFoundation> Logger);
    }
}
