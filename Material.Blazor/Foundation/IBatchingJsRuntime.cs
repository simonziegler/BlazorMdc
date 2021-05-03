using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// A service that wraps IJSRuntime, batching calls to InvokeVoidAsync().
    /// </summary>
    public interface IBatchingJSRuntime
    {
        /// <summary>
        /// Same as <see cref="JSRuntimeExtensions.InvokeVoidAsync(IJSRuntime, string, object[])"/>, except calls are batched in 10ms intervals.
        /// </summary>
        /// <param name="batchingWrapper"></param>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task InvokeVoidAsync(MBBatchingWrapper batchingWrapper, string identifier, params object[] args);


        /// <summary>
        /// Same as <see cref="JSRuntimeExtensions.InvokeAsync{TValue}(IJSRuntime, string, object?[]?)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>        
        Task<T> InvokeAsync<T>(string identifier, params object[] args);


        /// <summary>
        /// Flushes the batch associated with the supplied batching wrapper.
        /// </summary>
        /// <param name="batchingWrapper"></param>
        /// <returns></returns>
        Task FlushBatch(MBBatchingWrapper batchingWrapper);
    }
}
