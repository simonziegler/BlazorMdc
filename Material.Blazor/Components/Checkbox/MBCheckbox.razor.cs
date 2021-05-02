using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme check box accepting a boolean as a bound value. This
    /// check box does not implement indeteriminate state.
    /// </summary>
    public partial class MBCheckbox : InputComponent<bool>
    {
        /// <summary>
        /// The checkbox's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        private bool _isIndetermimate;
        /// <summary>
        /// Set to True if the checkbox is indeterminate.
        /// </summary>
        [Parameter]
        public bool IsIndeterminate
        {
            get => _isIndetermimate;
            set
            {
                if (value != _isIndetermimate)
                {
                    _isIndetermimate = value;
                    if (HasInstantiated)
                    {
                        _ = UpdateIndeterminateStateAsync();
                    }
                }
            }
        }


        /// <summary>
        /// The check box label.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// Inclusion of touch target
        /// </summary>
        [Parameter] public bool? TouchTarget { get; set; }

        private bool AppliedTouchTarget => CascadingDefaults.AppliedTouchTarget(TouchTarget);

        private Task UpdateIndeterminateStateAsync()
        {
            return InvokeVoidAsync("MaterialBlazor.MBCheckbox.setIndeterminate", ElementReference, IsIndeterminate);
        }

        /// <summary>
        /// Gets or sets a callback that updates the bound value.
        /// </summary>
        [Parameter] public EventCallback<bool> IsIndeterminateChanged { get; set; }


        private bool CheckedValue
        {
            get => ComponentValue;
            set
            {
                _isIndetermimate = false;
                IsIndeterminateChanged.InvokeAsync(false);
                ComponentValue = value;
            }
        }

        private ElementReference ElementReference { get; set; }
        private ElementReference FormReference { get; set; }

        private MBCascadingDefaults.DensityInfo DensityInfo => CascadingDefaults.GetDensityCssClass(CascadingDefaults.AppliedCheckboxDensity(Density));


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass)
                .AddIf("mdc-checkbox--selected", () => Value)
                .AddIf("mdc-checkbox--disabled", () => AppliedDisabled);

            SetComponentValue += OnValueSetCallback;
            OnDisabledSet += OnDisabledSetCallback;
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBCheckbox.setChecked", ElementReference, Value));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBCheckbox.setDisabled", ElementReference, AppliedDisabled));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBCheckbox.init", ElementReference, FormReference, ComponentValue, IsIndeterminate);
    }
}
