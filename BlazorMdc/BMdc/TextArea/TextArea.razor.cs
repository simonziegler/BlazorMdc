﻿using BMdcBase;

using BMdcModel;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System.Threading.Tasks;

namespace BMdc
{
    /// <summary>
    /// A Material Theme text field.
    /// </summary>
    public partial class TextArea : BMdcInputComponentBase<string>
    {
        /// <summary>
        /// The text input style.
        /// </summary>
        [Parameter] public TextInputStyle? TextInputStyle { get; set; }


        /// <summary>
        /// The text alignment style.
        /// </summary>
        [Parameter] public TextAlignStyle? TextAlignStyle { get; set; }


        /// <summary>
        /// Field label.
        /// </summary>
        [Parameter] public string Label { get; set; } = "";


        /// <summary>
        /// Hides the label if True. Defaults to False.
        /// </summary>
        [Parameter] public bool NoLabel { get; set; } = false;


        /// <summary>
        /// The number of rows to show when first rendered.
        /// </summary>
        [Parameter] public int Rows { get; set; }


        /// <summary>
        /// The number of columns to show when first rendered.
        /// </summary>
        [Parameter] public int Cols { get; set; }


        private ElementReference ElementReference { get; set; }
        private BMdcModel.TextInputStyle AppliedTextInputStyle => CascadingDefaults.AppliedStyle(TextInputStyle);
        private string AppliedTextInputStyleClass => BMdcBase.Utilities.GetTextAlignClass(CascadingDefaults.AppliedStyle(TextAlignStyle));
        private string FloatingLabelClass { get; set; }

        private readonly string id = BMdcBase.Utilities.GenerateUniqueElementName();
        private readonly string labelId = BMdcBase.Utilities.GenerateUniqueElementName();


        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ClassMapper
                .Add("mdc-text-field mdc-text-field--textarea")
                .AddIf(FieldClass, () => !string.IsNullOrWhiteSpace(FieldClass))
                .AddIf("mdc-text-field--outlined", () => (AppliedTextInputStyle == BMdcModel.TextInputStyle.Outlined))
                .AddIf("mdc-text-field--fullwidth", () => (AppliedTextInputStyle == BMdcModel.TextInputStyle.FullWidth))
                .AddIf("mdc-text-field--no-label", () => NoLabel)
                .AddIf("mdc-text-field--disabled", () => Disabled);

            ForceShouldRenderToTrue = true;
        }


        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            FloatingLabelClass = string.IsNullOrEmpty(ReportingValue) ? "" : "mdc-floating-label--float-above";
        }


        /// <inheritdoc/>
        private protected override async Task InitializeMdcComponent() => await JsRuntime.InvokeAsync<object>("BlazorMdc.textField.init", ElementReference);
    }
}
