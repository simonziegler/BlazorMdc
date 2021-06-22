using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// This is a utility type to facilitate extracting ID information from <see cref="DotNetObjectReference{TValue}"/> instances.
    /// We need it because <see cref="DotNetObjectReference{TValue}"/> throws on serialization, which messes up batching.
    /// This code is based on https://github.com/dotnet/aspnetcore/blob/014a198bccaa1d8e2f8db454e704ad19fff5e47e/src/JSInterop/Microsoft.JSInterop/src/Infrastructure/DotNetObjectReferenceJsonConverterFactory.cs
    /// Copyright (c) .NET Foundation. All rights reserved.
    /// Licensed under the Apache License, Version 2.0. See https://github.com/dotnet/aspnetcore for details.
    /// 
    /// Since  <see cref="Microsoft.JSInterop.Infrastructure.DotNetObjectReferenceJsonConverter{TValue}"/> is internal, we have to get that converter via the assembly.
    /// </summary>
    internal sealed class DotNetObjectReferenceJsonConverterFactory : JsonConverterFactory
    {
        private static readonly Type converter;
        public DotNetObjectReferenceJsonConverterFactory(IJSRuntime jsRuntime)
        {
            JSRuntime = jsRuntime;
        }
        static DotNetObjectReferenceJsonConverterFactory()
        {
            converter = typeof(DotNetObjectReference<>).Assembly.GetType("Microsoft.JSInterop.Infrastructure.DotNetObjectReferenceJsonConverter`1");
        }

        public IJSRuntime JSRuntime { get; }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(DotNetObjectReference<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            // System.Text.Json handles caching the converters per type on our behalf. No caching is required here.
            var instanceType = typeToConvert.GetGenericArguments()[0];
            var converterType = converter.MakeGenericType(instanceType);

            return (JsonConverter)Activator.CreateInstance(converterType, JSRuntime)!;
        }
    }
}
