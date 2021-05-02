using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Material.Blazor.Internal
{
    /// <inheritdoc/>
    internal class BatchingJSRuntime : IBatchingJSRuntime
    {
        /// <summary>
        /// A javascript call represented by its identifier and arguments
        /// </summary>
        private class Call
        {
            public string CrossReferenceId { get; private init; }

            public string Identifier { get; private init; }

            public object[] Args { get; private init; }

            public TaskCompletionSource TaskCompletionSource { get; private init; }

            public Task Task => TaskCompletionSource.Task;


            public Call(ComponentFoundation callingComponent, string identifier, object[] args)
            {
                CrossReferenceId = callingComponent.CrossReferenceId;
                Identifier = identifier;
                Args = args;
                TaskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }


        private readonly IJSRuntime js;
        private readonly ConcurrentDictionary<string, Call> queuedCalls = new();
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private readonly System.Timers.Timer timer = new(10);
        private static readonly Architecture Architecture = RuntimeInformation.OSArchitecture;


        public BatchingJSRuntime(IJSRuntime js)
        {
            this.js = js;
            timer.Elapsed += Timer_Elapsed;
        }


        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<Call> batch = new();

            await semaphoreSlim.WaitAsync();

            try
            { 
                foreach (var key in queuedCalls.Keys)
                {
                    if (queuedCalls.TryRemove(key, out var call))
                    {
                        batch.Add(call);
                    }
                }
                //while (queuedCalls.TryRemove(out var call))
                //{
                //    batch.Add(call);
                //}

                if (!batch.Any())
                {
                    return;
                }
            
                var errors = await js.InvokeAsync<string[]>("MaterialBlazor.Batching.apply", batch);
                foreach (var (call, error) in batch.Zip(errors))
                {
                    if (error != null)
                    {
                        call.TaskCompletionSource.SetException(new JSException(error));
                    }
                    else
                    {
                        call.TaskCompletionSource.SetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var call in batch)
                {
                    if (!call.TaskCompletionSource.Task.IsCompleted)
                    {
                        call.TaskCompletionSource.SetException(ex);
                    }
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }


        /// <inheritdoc/>
        public Task InvokeVoidAsync(ComponentFoundation callingComponent, string identifier, params object[] args)
        {
            if (Architecture == Architecture.Wasm)
            {
                return js.InvokeVoidAsync(identifier, args).AsTask();
            }

            var call = new Call(callingComponent, identifier, args);
            queuedCalls.TryAdd(call.CrossReferenceId, call);
            timer.Start();
            return call.Task;
        }


        /// <inheritdoc/>
        public async Task<T> InvokeAsync<T>(string identifier, params object[] args)
        {
            return await js.InvokeAsync<T>(identifier, args);
        }


        /// <inheritdoc/>
        public async Task SemaphoreDispose(ComponentFoundation callingComponent, Func<Task> disposeAsync, Action dispose)
        {
            if (Architecture == Architecture.Wasm)
            {
                dispose();
                await disposeAsync();
                return;
            }

            await semaphoreSlim.WaitAsync();

            try
            {
                queuedCalls.TryRemove(callingComponent.CrossReferenceId, out var call);

                dispose();
                await disposeAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
