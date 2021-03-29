using Moq;
using System;

namespace Material.Blazor.Test
{
    public static class MoqChainExtension
    {
        public static Mock<T> Chain<T>(this Mock<T> mock, Action<Mock<T>> action) where T : class
        {
            action(mock);
            return mock;
        }
    }
}
