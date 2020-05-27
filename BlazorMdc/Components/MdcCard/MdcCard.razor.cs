﻿using BBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BMdc
{
    /// <summary>
    /// A Material Theme card with three elements: primary, primary action buttons and action icons.
    /// </summary>
    public partial class MdcCard : BBase.ComponentBase
    {
        /// <summary>
        /// The card style - see <see cref="BlazorMdc.CardStyle"/>
        /// </summary>
        [Parameter] public BEnum.CardStyle? CardStyle { get; set; }


        /// <summary>
        /// Styles the primary and primary action sections with padding if set to True. This is optional
        /// and you can individually style the HTML in the primary and primary action render fragments if you prefer.
        /// </summary>
        [Parameter] public bool AutoStyled { get; set; }


        /// <summary>
        /// A render fragment for the primary section
        /// </summary>
        [Parameter] public RenderFragment Primary { get; set; }


        /// <summary>
        /// A render fragment for the primary action section, which responds to Blazor events such as @onclick.
        /// </summary>
        [Parameter] public RenderFragment PrimaryAction { get; set; }


        /// <summary>
        /// A render fragment where you place <see cref="MdcButton"/>s as action buttons.
        /// </summary>
        [Parameter] public RenderFragment ActionButtons { get; set; }


        /// <summary>
        /// A render fragment where you place <see cref="MdcIconButton"/>s as action icons.
        /// </summary>
        [Parameter] public RenderFragment ActionIcons { get; set; }


        private string PrimaryClass => AutoStyled ? "bmdc-card__primary" : "";
        private string PrimaryActionClass => AutoStyled ? "bmdc-card__primary-action" : "";
        private ElementReference PrimaryActionReference { get; set; }


        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ClassMapper
                .Add("mdc-card")
                .AddIf("mdc-card--outlined", () => CascadingDefaults.AppliedStyle(CardStyle) == BEnum.CardStyle.Outlined);
        }


        /// <summary>
        /// Overrides <see cref="OnAfterRenderAsync(bool)"/> because the method must only
        /// initiate Material Theme javascript if the <see cref="PrimaryAction"/> is not null.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && PrimaryAction != null)
            {
                await JsRuntime.InvokeAsync<object>("BlazorMdc.cardPrimaryAction.init", PrimaryActionReference);
            }
        }
    }
}
