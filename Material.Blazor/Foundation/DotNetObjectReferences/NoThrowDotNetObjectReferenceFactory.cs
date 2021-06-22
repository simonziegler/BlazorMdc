using Microsoft.JSInterop;
using System.Text.Json;

namespace Material.Blazor.Internal
{
    /// <inheritdoc/>
    internal class NoThrowDotNetObjectReferenceFactory : INoThrowDotNetObjectReferenceFactory
    {
        private readonly JsonSerializerOptions options;
        public NoThrowDotNetObjectReferenceFactory(IJSRuntime jSRuntime)
        {
            options = new JsonSerializerOptions
            {
                Converters =
                {
                    new DotNetObjectReferenceJsonConverterFactory(jSRuntime),
                }
            };
        }
        public NoThrowDotNetObjectReference<TValue> Create<TValue>(TValue value) where TValue : class
        {
            return new NoThrowDotNetObjectReference<TValue>(options, DotNetObjectReference.Create(value));
        }
    }
}
