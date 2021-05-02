using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme menu.
    /// </summary>
    public partial class MBMenuSurface : ComponentFoundation
    {
        /// <summary>
        /// A render fragement as a set of <see cref="MBListItem"/>s.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }


        /// <summary>
        /// Regular, fullwidth or fixed positioning/width.
        /// </summary>
        [Parameter] public MBMenuSurfacePositioning MenuSurfacePositioning { get; set; } = MBMenuSurfacePositioning.Regular;


        private DotNetObjectReference<MBMenuSurface> ObjectReference { get; set; }
        private ElementReference ElementReference { get; set; }
        private bool IsOpen { get; set; } = false;


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _ = ConditionalCssClasses
                .AddIf(GetMenuSurfacePositioningClass(MenuSurfacePositioning), () => MenuSurfacePositioning != MBMenuSurfacePositioning.Regular);

            ObjectReference = DotNetObjectReference.Create(this);
        }


        /// <summary>
        /// For Material Theme to notify of menu closure via JS Interop.
        /// </summary>
        [JSInvokable()]
        public void NotifyClosed()
        {
            IsOpen = false;
        }


        /// <summary>
        /// Toggles the menu open and closed.
        /// </summary>
        /// <returns></returns>
        public async Task ToggleAsync()
        {
            if (IsOpen)
            {
                await InvokeVoidAsync("MaterialBlazor.MBMenuSurface.hide", ElementReference);
                IsOpen = false;
            }
            else
            {
                await InvokeVoidAsync("MaterialBlazor.MBMenuSurface.show", ElementReference);
                IsOpen = true;
            }
        }


        /// <inheritdoc/>
        private protected override async Task InstantiateMcwComponentAsync() => await InvokeVoidAsync("MaterialBlazor.MBMenuSurface.init", ElementReference, ObjectReference);


        /// <inheritdoc/>
        private protected override void DisposeMcwComponent() => ObjectReference?.Dispose();


        /// <summary>
        /// Returns a menu surface class determined by the parameter.
        /// </summary>
        /// <param name="surfacePositioning"></param>
        /// <returns></returns>
        private static string GetMenuSurfacePositioningClass(MBMenuSurfacePositioning surfacePositioning) =>
            surfacePositioning switch
            {
                MBMenuSurfacePositioning.FullWidth => "mdc-menu-surface--fullwidth",
                MBMenuSurfacePositioning.Fixed => "mdc-menu-surface--fixed",
                _ => ""
            };
    }
}
