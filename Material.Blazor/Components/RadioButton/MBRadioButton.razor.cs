using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;

using System;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme radio button. Accepts a generic class TItem
    /// and displays as checked if <see cref="InputComponent{T}.Value"/> equals <see cref="TargetCheckedValue"/>.
    /// </summary>
    public partial class MBRadioButton<TItem> : InputComponent<TItem>
    {
        /// <summary>
        /// <see cref="InputComponent{T}.Value"/> is set to this when the 
        /// radio button is clicked. If the consumer sets <see cref="InputComponent{T}.Value"/>
        /// to this the radio state will change to checked, or cleared for any other value.
        /// </summary>
        [Parameter] public TItem TargetCheckedValue { get; set; }


        /// <summary>
        /// The radio button label.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// The radio button's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        /// <summary>
        /// The radio button group name. This is applied to a group of radio buttons
        /// so Material Theme can identify them as representing different values of the
        /// same property.
        /// </summary>
        [Parameter] public string RadioGroupName { get; set; }


        private readonly string radioId = Utilities.GenerateUniqueElementName();


        private ElementReference FormReference { get; set; }
        private bool InputDisabled { get; set; }
        private ElementReference RadioButtonReference { get; set; }
        private string DisabledClass { get; set; } = "";

        private MBCascadingDefaults.DensityInfo DensityInfo => CascadingDefaults.GetDensityCssClass(CascadingDefaults.AppliedRadioButtonDensity(Density));


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass)
                .AddIf("mdc-checkbox--disabled", () => AppliedDisabled);

            InputDisabled = AppliedDisabled;

            ForceShouldRenderToTrue = true;

            SetComponentValue += OnValueSetCallback;
            OnDisabledSet += OnDisabledSetCallback;
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (string.IsNullOrEmpty(RadioGroupName))
            {
                throw new ArgumentException("RadioGroupName is a required parameter in MBRadioButton.");
            }

            if (AppliedDisabled)
            {
                DisabledClass = "mdc-radio--disabled";
            }
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBRadioButton.setChecked", RadioButtonReference, Value.Equals(TargetCheckedValue)).ConfigureAwait(false));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBRadioButton.setDisabled", RadioButtonReference, AppliedDisabled));


        private void OnInternalItemClick()
        {
            ComponentValue = TargetCheckedValue;
        }


        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBRadioButton.init", RadioButtonReference, FormReference, Value?.Equals(TargetCheckedValue) ?? false);
    }
}
