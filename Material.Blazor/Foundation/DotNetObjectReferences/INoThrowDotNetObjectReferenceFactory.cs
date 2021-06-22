namespace Material.Blazor.Internal
{
    /// <summary>
    /// Helper factory. To be used in place of <see cref="Microsoft.JSInterop.DotNetObjectReference"/>.
    /// </summary>
    public interface INoThrowDotNetObjectReferenceFactory
    {
        NoThrowDotNetObjectReference<TValue> Create<TValue>(TValue value) where TValue : class;
    }
}
