using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme menu.
    /// </summary>
    public partial class MBMenu : ComponentFoundation
    {
        /// <summary>
        /// A render fragement as a set of <see cref="MBListItem"/>s.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }


        /// <summary>
        /// Regular, fullwidth or fixed positioning/width.
        /// </summary>
        [Parameter] public MBMenuSurfacePositioning MenuSurfacePositioning { get; set; } = MBMenuSurfacePositioning.Regular;


        private DotNetObjectReference<MBMenu> ObjectReference { get; set; }
        private ElementReference ElementReference { get; set; }
        private bool IsOpen { get; set; } = false;


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf(GetMenuSurfacePositioningClass(MenuSurfacePositioning), () => MenuSurfacePositioning != MBMenuSurfacePositioning.Regular);

            ObjectReference = DotNetObjectReference.Create(this);
        }


        /// <summary>
        /// For Material Theme to notify of menu closure via JS Interop.
        /// </summary>
        [JSInvokable]
        public void NotifyClosed()
        {
            IsOpen = false;
        }


        /// <summary>
        /// Toggles the menu open and closed. NEED TO RETURN <code>Task</code> RATHER THAN <code>Task&lt;string&gt;</code> IN VERSION 2.0.0
        /// </summary>
        /// <returns></returns>
        public async Task ToggleAsync()
        {
            if (IsOpen)
            {
                await InvokeVoidAsync("MaterialBlazor.MBMenu.hide", ElementReference);
                IsOpen = false;
            }
            else
            {
                await InvokeVoidAsync("MaterialBlazor.MBMenu.show", ElementReference);
                IsOpen = true;
            }
        }


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBMenu.init", ElementReference, ObjectReference);


        /// <inheritdoc/>
        private protected override void DisposeMcwComponent() => ObjectReference?.Dispose();


        /// <summary>
        /// Returns a menu surface class determined by the parameter.
        /// </summary>
        /// <param name="surfacePositioning"></param>
        /// <returns></returns>
        internal static string GetMenuSurfacePositioningClass(MBMenuSurfacePositioning surfacePositioning) =>
            surfacePositioning switch
            {
                MBMenuSurfacePositioning.FullWidth => "mdc-menu-surface--fullwidth",
                MBMenuSurfacePositioning.Fixed => "mdc-menu-surface--fixed",
                _ => ""
            };
    }
}
