using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme icon button toggle, with provision for standard MB styling, leading 
    /// and trailing icons and all standard Blazor events. Adds the "mdc-card__action--icon" class when 
    /// placed inside an <see cref="MBCard"/>.
    /// </summary>
    public partial class MBIconButtonToggle : InputComponent<bool>
    {
        [CascadingParameter] private MBCard Card { get; set; }


#nullable enable annotations
        /// <summary>
        /// The on-state icon's name.
        /// </summary>
        [Parameter] public string IconOn { get; set; }


        /// <summary>
        /// The off-state icon's name.
        /// </summary>
        [Parameter] public string IconOff { get; set; }


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
                .AddIf("mdc-card__action mdc-card__action--icon", () => (Card != null))
                .AddIf("mdc-icon-button--on", () => Value);

            SetComponentValue += OnValueSetCallback;
            OnDisabledSet += OnDisabledSetCallback;
        }


        /// <summary>
        /// Toggles Value when the button is clicked.
        /// </summary>
        private void ToggleOnClick()
        {
            ComponentValue = !ComponentValue;
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBIconButtonToggle.setOn", ElementReference, Value));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(AllowNextShouldRender);


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBIconButtonToggle.init", ElementReference);
    }
}
