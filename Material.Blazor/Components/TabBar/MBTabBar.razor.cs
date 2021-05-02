using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme tab bar.
    /// </summary>
    public partial class MBTabBar<TItem> : InputComponent<int>
    {
        /// <summary>
        /// Stack icons vertically if True, otherwise icons are leading.
        /// </summary>
        [Parameter] public bool StackIcons { get; set; }


        /// <summary>
        /// A function delegate to return the parameters for <c>@key</c> attributes. If unused
        /// "fake" keys set to GUIDs will be used.
        /// </summary>
        [Parameter] public Func<TItem, object> GetKeysFunc { get; set; }


        /// <summary>
        /// The list of items for the tab bar.
        /// </summary>
        [Parameter] public IEnumerable<TItem> Items { get; set; }


        /// <summary>
        /// Label render fragments.
        /// </summary>
        [Parameter] public RenderFragment<TItem> Label { get; set; }


        /// <summary>
        /// Icon render fragments requiring correct icon markup including the "mdc-tab__icon"
        /// CSS class. Note that Material Icons always render properly, while some wider Font Awesome
        /// icons for instance render too close to the tab text.
        /// </summary>
        [Parameter] public RenderFragment<TItem> Icon { get; set; }


        /// <summary>
        /// The tab bar's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        private ElementReference ElementReference { get; set; }
        private DotNetObjectReference<MBTabBar<TItem>> ObjectReference { get; set; }
        private Func<TItem, object> KeyGenerator { get; set; }
        private string StackClass => StackIcons ? "mdc-tab--stacked" : "";

        private MBCascadingDefaults.DensityInfo DensityInfo
        {
            get
            {
                var d = CascadingDefaults.GetDensityCssClass(CascadingDefaults.AppliedTabBarDensity(Density));

                d.CssClassName += StackIcons ? "--stacked" : "--unstacked";

                return d;
            }
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ObjectReference = DotNetObjectReference.Create(this);

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass);

            SetComponentValue += OnValueSetCallback;
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            KeyGenerator = GetKeysFunc ?? delegate (TItem item) { return item; };
        }


        /// <summary>
        /// For Material Theme to notify when a tab is clicked via JS Interop.
        /// </summary>
        /// <returns></returns>
        [JSInvokable]
        public void NotifyActivated(int index)
        {
            ComponentValue = index;
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBTabBar.activateTab", ElementReference, Value));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBTabBar.init", ElementReference, ObjectReference);


        /// <inheritdoc/>
        private protected override void DisposeMcwComponent() => ObjectReference?.Dispose();
    }
}
