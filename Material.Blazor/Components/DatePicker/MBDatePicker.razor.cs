using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;

using System;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// A date picker styled to match the Material Theme date picker specification, using
    /// a modfied Material Theme select input as also applied in <see cref="MBSelect{TItem}"/>.
    /// </summary>
    public partial class MBDatePicker : InputComponent<DateTime>
    {
        private static readonly DateTime MinAllowableDate = DateTime.MinValue.AddMonths(1);
        private static readonly DateTime MaxAllowableDate = DateTime.MaxValue.AddMonths(-1);

        /// <summary>
        /// The select style.
        /// <para>Overrides <see cref="MBCascadingDefaults.SelectInputStyle"/></para>
        /// </summary>
        [Parameter] public MBSelectInputStyle? SelectInputStyle { get; set; }


        /// <summary>
        /// The label.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// Date selection criteria
        /// </summary>
        [Parameter] public MBDateSelectionCriteria? DateSelectionCriteria { get; set; }


#nullable enable annotations
        internal static readonly Func<DateTime, bool> DateIsSelectableNotUsed = (_) => true;
        /// <summary>
        /// Control whether a date is selectable by evaluating the method.
        /// </summary>
        [Parameter] public Func<DateTime, bool>? DateIsSelectable { get; set; } = DateIsSelectableNotUsed;
#nullable restore annotations


        /// <summary>
        /// Minimum date set by the consumer -- We cannot use the MinDate here as we go back up to a month
        /// </summary>
        [Parameter] public DateTime MinDate { get; set; } = MinAllowableDate;


        /// <summary>
        /// Maximum date set by the consumer -- Again, same reason, can't use max date
        /// </summary>
        [Parameter] public DateTime MaxDate { get; set; } = MaxAllowableDate;


        /// <summary>
        /// Specification for date format
        /// </summary>
        [Parameter] public string DateFormat { get; set; }


        /// <summary>
        /// The select's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        /// <summary>
        /// Regular, fullwidth or fixed positioning/width. Overrides a value of <see cref="MBMenuSurfacePositioning.FullWidth"/>
        /// with <see cref="MBMenuSurfacePositioning.Regular"/>.
        /// </summary>
        [Parameter] public MBMenuSurfacePositioning MenuSurfacePositioning { get; set; } = MBMenuSurfacePositioning.Regular;


        private MBDensity AppliedDensity => CascadingDefaults.AppliedSelectDensity(Density);
        private string AppliedDateFormat => CascadingDefaults.AppliedDateFormat(DateFormat);
        private MBSelectInputStyle AppliedInputStyle => CascadingDefaults.AppliedStyle(SelectInputStyle);
        private ElementReference ElementReference { get; set; }
        private bool IsOpen { get; set; } = false;
        private string MenuClass => MBMenu.GetMenuSurfacePositioningClass(MenuSurfacePositioning == MBMenuSurfacePositioning.Fixed ? MBMenuSurfacePositioning.Fixed : MBMenuSurfacePositioning.Regular) + ((Panel?.ShowYearPad ?? true) ? " mb-dp-menu__day-menu" : " mb-dp-menu__year-menu");
        private InternalDatePickerPanel Panel { get; set; }
        private bool ShowLabel => !string.IsNullOrWhiteSpace(Label);


        private MBCascadingDefaults.DensityInfo DensityInfo
        {
            get
            {
                var d = CascadingDefaults.GetDensityCssClass(AppliedDensity);

                d.CssClassName += AppliedInputStyle == MBSelectInputStyle.Filled ? "--filled" : "--outlined";

                return d;
            }
        }


        private readonly string labelId = Utilities.GenerateUniqueElementName();
        private readonly string listboxId = Utilities.GenerateUniqueElementName();
        private readonly string selectedTextId = Utilities.GenerateUniqueElementName();


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (MinDate < MinAllowableDate)
            {
                throw new ArgumentOutOfRangeException($"MinDate cannot be before {MinAllowableDate.ToShortDateString()}");
            }

            if (MaxDate > MaxAllowableDate)
            {
                throw new ArgumentOutOfRangeException($"MaxDate cannot be after {MaxAllowableDate.ToShortDateString()}");
            }

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass)
                .AddIf("mdc-select--filled", () => AppliedInputStyle == MBSelectInputStyle.Filled)
                .AddIf("mdc-select--outlined", () => AppliedInputStyle == MBSelectInputStyle.Outlined)
                .AddIf("mdc-select--no-label", () => string.IsNullOrWhiteSpace(Label))
                .AddIf("mdc-select--disabled", () => AppliedDisabled);

            SetComponentValue += OnValueSetCallback;

            OnDisabledSet += OnDisabledSetCallback;
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback()
        {
            Panel.SetParameters(true, Value);
            InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBDatePicker.listItemClick", Panel.ListItemReference, Utilities.DateToString(Value, AppliedDateFormat)).ConfigureAwait(false));
        }


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBDatePicker.setDisabled", ElementReference, AppliedDisabled));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBDatePicker.init", ElementReference);
    }
}
