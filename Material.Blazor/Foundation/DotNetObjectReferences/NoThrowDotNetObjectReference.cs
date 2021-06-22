using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// This is a wrapper around <see cref="DotNetObjectReference{TValue}"/> which - surprise - doesn't throw, when disposed. This is batching-friendly.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class NoThrowDotNetObjectReference<TValue> : IDisposable where TValue : class
    {
        public long __dotNetObject { get; set; }
        private readonly DotNetObjectReference<TValue> dotNetObjectReference;
        [JsonConstructor, Obsolete("This constructor may not be used. It is a JsonConstructor only.", error: true)]
        public NoThrowDotNetObjectReference()
        {
        }
        public NoThrowDotNetObjectReference(JsonSerializerOptions options, DotNetObjectReference<TValue> dotNetObjectReference)
        {
            this.dotNetObjectReference = dotNetObjectReference;
            var serialized = JsonSerializer.Serialize(dotNetObjectReference, options);
            __dotNetObject = JsonSerializer.Deserialize<NoThrowDotNetObjectReference<TValue>>(serialized).__dotNetObject;

        }
        public void Dispose()
        {
            // we dispose the dotNetObjectReference as normal. This doesn't mess with the no-throw behavior, because what gets serialized is this instance, which just contains the __dotNetObject ID property.
            dotNetObjectReference.Dispose();
        }
    }
}
