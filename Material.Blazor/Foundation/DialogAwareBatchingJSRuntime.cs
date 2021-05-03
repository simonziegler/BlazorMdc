using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    internal class DialogAwareBatchingJSRuntime : IBatchingJSRuntime
    {
        private readonly IBatchingJSRuntime underlyingJSRuntime;
        private readonly MBDialog dialog;


        public DialogAwareBatchingJSRuntime(IBatchingJSRuntime underlyingJSRuntime, MBDialog dialog)
        {
            this.underlyingJSRuntime = underlyingJSRuntime;
            this.dialog = dialog;
        }


        /// <inheritdoc/>
        public async Task<T> InvokeAsync<T>(string identifier, params object[] args)
        {
            await dialog.Opened;
            return await underlyingJSRuntime.InvokeAsync<T>(identifier, args);
        }


        /// <inheritdoc/>
        public async Task InvokeVoidAsync(MBBatchingWrapper batchingWrapper, string identifier, params object[] args)
        {
            await dialog.Opened;
            await underlyingJSRuntime.InvokeVoidAsync(batchingWrapper, identifier, args);
        }


        /// <inheritdoc/>
        public Task FlushBatch(MBBatchingWrapper batchingWrapper)
        {
            return underlyingJSRuntime.FlushBatch(batchingWrapper);
        }
    }
}
