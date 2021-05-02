using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;

using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a Material Theme FAB or floating action button, with regular, mini or extended variants,
    /// application of the "mdc-fab--exited" animated class and the choice of leading or trailing icons for
    /// the extended variant.
    /// </summary>
    public partial class MBFloatingActionButton : ComponentFoundation
    {
        [CascadingParameter] private MBCard Card { get; set; }

        /// <summary>
        /// Inclusion of touch target
        /// </summary>
        [Parameter] public bool? TouchTarget { get; set; }


#nullable enable annotations
        /// <summary>
        /// The icon's name.
        /// </summary>
        [Parameter] public string Icon { get; set; }


        /// <summary>
        /// The foundry to use for both leading and trailing icons.
        /// <para><c>IconFoundry="IconHelper.MIIcon()"</c></para>
        /// <para><c>IconFoundry="IconHelper.FAIcon()"</c></para>
        /// <para><c>IconFoundry="IconHelper.OIIcon()"</c></para>
        /// <para>Overrides <see cref="MBCascadingDefaults.IconFoundryName"/></para>
        /// </summary>
        [Parameter] public IMBIconFoundry? IconFoundry { get; set; }


        /// <summary>
        /// Sets the FAB to be regular or the mini or extended variants.
        /// </summary>
        [Parameter] public MBFloatingActionButtonType Type { get; set; }


        /// <summary>
        /// Sets the label, which is ignored for anything other than the extended variant.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// Leading icon if false, otherwise trailine, and only applied to the extended variant.
        /// </summary>
        [Parameter] public bool IconTrailsLabel { get; set; }


        private bool AppliedTouchTarget => CascadingDefaults.AppliedTouchTarget(TouchTarget);

        private bool exited;
        /// <summary>
        /// Leading icon if false, otherwise trailine, and only applied to the extended variant.
        /// </summary>
        [Parameter]
        public bool Exited
        {
            get => exited;
            set
            {
                if (value != exited)
                {
                    exited = value;

                    if (HasInstantiated)
                    {
                        InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBFloatingActionButton.setExited", ElementReference, Exited));
                    }
                }
            }
        }
#nullable restore annotations


        private ElementReference ElementReference { get; set; }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf("mdc-fab--mini mdc-fab--touch", () => Type == MBFloatingActionButtonType.Mini)
                .AddIf("mdc-fab--extended", () => Type == MBFloatingActionButtonType.Extended)
                .AddIf("mdc-fab--exited", () => Exited);
        }


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBFloatingActionButton.init", ElementReference, Exited);
    }
}
