using Bunit;
using Material.Blazor;
using Material.Blazor.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Testing
{
    public class Mocking
    {
        private Bunit.TestContext ctx;
        [SetUp]
        public void InjectMockedServices()
        {
            ctx = new();
            _ = ctx.Services
                .AddSingleton(new Mock<IBatchingJSRuntime>().Object)
                .AddSingleton(new Mock<INoThrowDotNetObjectReferenceFactory>().Object)
                .AddSingleton(new Mock<IMBLoggingService>().Object)
                .AddSingleton(new Mock<IMBTooltipService>().Object)
                .AddSingleton(new Mock<IMBToastService>().Object)
                .AddSingleton(new Mock<IMBSnackbarService>().Object)
                .AddSingleton(new Mock<ILogger<ComponentFoundation>>().Object)
                .AddSingleton(new Mock<IMBAnimatedNavigationManager>()
                    .Chain(m => m.SetupGet(anm => anm.Configuration).Returns(new MBAnimatedNavigationManagerServiceConfiguration()))
                    .Object)
                .AddSingleton(new Mock<IMBIcon>().Object)
                .AddSingleton(new Mock<IMBIconFoundry>().Object);
        }
        [Test]
        public void TryRenderMBAnimatedNavigation()
        {
            var cut = ctx.RenderComponent<MBAnimatedNavigation>();
            cut.MarkupMatches("");
        }
        [Test]
        public void TryRenderMBAnchor()
        {
            var cut = ctx.RenderComponent<MBAnchor>();
            cut.MarkupMatches("");
        }
        [Test]
        public void TryRenderMBIcon()
        {
            var cut = ctx.RenderComponent<MBIcon>((nameof(MBIcon.IconName), "alarm"));
            cut.MarkupMatches("<i class=\"material-icons\">alarm</i>");
        }
        [Test]
        public void TryRenderMBDialog()
        {
            var cut = ctx.RenderComponent<MBDialog>();
            cut.MarkupMatches(@"
<div class=""mdc-dialog"" aria-modal=""true"">
    <div class=""mdc-dialog__container""></div>
    <div class=""mdc-dialog__scrim""></div>
</div>");
        }
    }
}
