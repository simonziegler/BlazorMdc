﻿using BBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BMdc
{
    /// <summary>
    /// This is a general purpose Material Theme button, with provision for standard MT styling, leading 
    /// and trailing icons and all standard Blazor events. Adds the "mdc-card__action--button" class when 
    /// placed inside an <see cref="MdcCard"/>.
    /// </summary>
    public partial class MdcButton : BBase.ComponentBase
    {
        [CascadingParameter] private MdcCard Card { get; set; }

        [CascadingParameter] private MdcDialog Dialog { get; set; }


#nullable enable annotations
        /// <summary>
        /// The button's Material Theme style - see <see cref="BlazorMdc.ButtonStyle"/>.
        /// </summary>
        [Parameter] public BEnum.ButtonStyle? ButtonStyle { get; set; }


        /// <summary>
        /// The button's label.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// The leading icon's name. No leading icon shown if not set.
        /// </summary>
        [Parameter] public string? LeadingIcon { get; set; }


        /// <summary>
        /// The trailing icon's name. No leading icon shown if not set.
        /// </summary>
        [Parameter] public string? TrailingIcon { get; set; }


        /// <summary>
        /// The foundry to use for both leading and trailing icons.
        /// <para><c>IconFoundry="BModel.IconHelper.MIIcon()"</c></para>
        /// <para><c>IconFoundry="BModel.IconHelper.FAIcon()"</c></para>
        /// <para><c>IconFoundry="BModel.IconHelper.OIIcon()"</c></para>
        /// </summary>
        [Parameter] public BModel.IIconFoundry? IconFoundry { get; set; }


        /// <summary>
        /// A string value to return from an <see cref="MdcDialog"/> when this button is pressed.
        /// </summary>
        [Parameter] public string DialogAction { get; set; }
#nullable restore annotations


        private ElementReference ElementReference { get; set; }


        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ClassMapper
                .Add("mdc-button")
                .AddIf("mdc-button--raised", () => CascadingDefaults.AppliedStyle(ButtonStyle, Card, Dialog) == BEnum.ButtonStyle.ContainedRaised)
                .AddIf("mdc-button--unelevated", () => CascadingDefaults.AppliedStyle(ButtonStyle, Card, Dialog) == BEnum.ButtonStyle.ContainedUnelevated)
                .AddIf("mdc-button--outlined", () => CascadingDefaults.AppliedStyle(ButtonStyle, Card, Dialog) == BEnum.ButtonStyle.Outlined)
                .AddIf("mdc-card__action mdc-card__action--button", () => (Card != null));
        }


        /// <inheritdoc/>
        private protected override async Task InitializeMdcComponent() => await JsRuntime.InvokeAsync<object>("BlazorMdc.button.init", ElementReference);
    }
}
