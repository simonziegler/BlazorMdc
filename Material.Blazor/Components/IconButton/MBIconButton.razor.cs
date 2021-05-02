using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme icon button, with provision for standard MB styling, leading 
    /// and trailing icons and all standard Blazor events. Adds the "mdc-card__action--icon" class when 
    /// placed inside an <see cref="MBCard"/>.
    /// </summary>
    public partial class MBIconButton : ComponentFoundation
    {
        [CascadingParameter] private MBCard Card { get; set; }


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
#nullable restore annotations


        /// <summary>
        /// The button's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        private ElementReference ElementReference { get; set; }

        private MBCascadingDefaults.DensityInfo DensityInfo => CascadingDefaults.GetDensityCssClass(CascadingDefaults.AppliedIconButtonDensity(Density));


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass)
                .AddIf("mdc-card__action mdc-card__action--icon", () => (Card != null));
        }


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBIconButton.init", ElementReference);
    }
}
