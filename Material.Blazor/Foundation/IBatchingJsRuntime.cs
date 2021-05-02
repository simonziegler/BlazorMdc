using Microsoft.JSInterop;
using System;
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
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task InvokeVoidAsync(ComponentFoundation callingComponent, string identifier, params object[] args);


        /// <summary>
        /// Same as <see cref="JSRuntimeExtensions.InvokeAsync{TValue}(IJSRuntime, string, object?[]?)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>        
        Task<T> InvokeAsync<T>(string identifier, params object[] args);


        /// <summary>
        /// Called by <see cref="ComponentFoundation"/> with component dispose actions. These must be managed by batching js
        /// to avoid race conditions between disposal and delayed/timer instantiation.
        /// </summary>
        /// <param name="disposeAsync"></param>
        /// <param name="dispose"></param>
        /// <returns></returns>
        Task SemaphoreDispose(ComponentFoundation callingComponent, Func<Task> disposeAsync, Action dispose);
    }
}
