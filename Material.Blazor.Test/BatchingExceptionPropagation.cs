using Material.Blazor.Internal;
using Microsoft.JSInterop;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Material.Blazor.Test
{
    public class BatchingExceptionPropagation
    {
        [Fact]
        public async Task Message()
        {
            var js_runtime = new Mock<IJSRuntime>()
                .Chain(m => m.Setup(js => js.InvokeAsync<string[]>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(new[] { "failed because of XYZ" }))
                .Object;
            var batching_js_runtime = new BatchingJSRuntime(js_runtime);
            var exception = await Assert.ThrowsAsync<JSException>(() => batching_js_runtime.InvokeVoidAsync("foo"));
            Assert.Contains("failed because of XYZ", exception.Message);
        }
        [Fact]
        public void PropagateCancelledTask()
        {
            var js_runtime = new Mock<IJSRuntime>()
                .Chain(m => m.Setup(js => js.InvokeAsync<string[]>(It.IsAny<string>(), It.IsAny<object[]>())).ThrowsAsync(new TaskCanceledException()))
                .Object;
            var batching_js_runtime = new BatchingJSRuntime(js_runtime);
            Assert.ThrowsAsync<TaskCanceledException>(() => batching_js_runtime.InvokeVoidAsync("foo"));
        }
    }
}
