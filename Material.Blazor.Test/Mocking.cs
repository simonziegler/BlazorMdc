using Bunit;
using Material.Blazor;
using Material.Blazor.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Material.Blazor.Test
{
    public class Mocking
    {
        private TestContext ctx;
        private void InjectMockedServices()
        {
            ctx = new();
            _ = ctx.Services
                .AddSingleton(new Mock<IBatchingJSRuntime>().Object)
                .AddSingleton(new Mock<IMBTooltipService>().Object)
                .AddSingleton(new Mock<IMBToastService>().Object)
                .AddSingleton(new Mock<IMBSnackbarService>().Object)
                .AddSingleton(new Mock<ILogger<ComponentFoundation>>().Object)
                .AddSingleton(new Mock<IMBAnimatedNavigationManager>()
                    .Chain(m => m.SetupGet(anm => anm.Configuration).Returns(new MBAnimatedNavigationManagerServiceConfiguration()))
                    .Object)
                .AddSingleton(new Mock<IMBDialog>().Object)
                .AddSingleton(new Mock<IMBIcon>().Object)
                .AddSingleton(new Mock<IMBIconFoundry>().Object);
        }
        [Fact]
        public void TryRenderMBAnimatedNavigation()
        {
            InjectMockedServices();
            var cut = ctx.RenderComponent<MBAnimatedNavigation>();
            cut.MarkupMatches("");
        }
        [Fact]
        public void TryRenderMBAnchor()
        {
            InjectMockedServices();
            var cut = ctx.RenderComponent<MBAnchor>();
            cut.MarkupMatches("");
        }
        [Fact]
        public void TryRenderMBIcon()
        {
            InjectMockedServices();
            var cut = ctx.RenderComponent<MBIcon>((nameof(MBIcon.IconName), "alarm"));
            cut.MarkupMatches("<i class=\"material-icons\">alarm</i>");
        }
        [Fact]
        public void TryRenderMBDialog()
        {
            InjectMockedServices();
            var cut = ctx.RenderComponent<MBDialog>();
            cut.MarkupMatches("");
        }
    }
}
