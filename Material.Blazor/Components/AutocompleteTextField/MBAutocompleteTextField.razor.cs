using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// An autocomplete built using an <see cref="MBTextField"/> with the anchor and drop
    /// down list implementation from a Material Theme select.
    /// </summary>
    public partial class MBAutocompleteTextField : InputComponent<string>
    {
        private IEnumerable<string> selectItems;
        private IEnumerable<string> newSelectItems = null;

        private class SelectionInfo
        {
            public string SelectedText { get; set; }
            public string[] SelectList { get; set; }
            public bool FirstValueIsCustomValue { get; set; }
            public bool FullMatchFound => (SelectList.Length == 1) && SelectList.Contains(SelectedText);
        }


        private class SelectionItem
        {
            public string Item { get; set; }
            public bool IgnoreWhitespace { get; set; }
            public string SearchTarget => IgnoreWhitespace ? Regex.Replace(Item, @"\s+", string.Empty) : Item;
        }


#nullable enable annotations
        /// <summary>
        /// Helper text that is displayed either with focus or persistently with <see cref="HelperTextPersistent"/>.
        /// </summary>
        [Parameter] public string HelperText { get; set; } = "";


        /// <summary>
        /// Makes the <see cref="HelperText"/> persistent if true.
        /// </summary>
        [Parameter] public bool HelperTextPersistent { get; set; } = false;


        /// <summary>
        /// Delivers Material Theme validation methods from native Blazor validation. Either use this or
        /// the Blazor <code>ValidationMessage</code> component, but not both. This parameter takes the same input as
        /// <code>ValidationMessage</code>'s <code>For</code> parameter.
        /// </summary>
        [Parameter] public Expression<Func<object>> ValidationMessageFor { get; set; }


        /// <summary>
        /// The text input style.
        /// <para>Overrides <see cref="MBCascadingDefaults.TextInputStyle"/></para>
        /// </summary>
        [Parameter] public MBTextInputStyle? TextInputStyle { get; set; }


        /// <summary>
        /// The text alignment style.
        /// <para>Overrides <see cref="MBCascadingDefaults.TextAlignStyle"/></para>
        /// </summary>
        [Parameter] public MBTextAlignStyle? TextAlignStyle { get; set; }


        /// <summary>
        /// Field label.
        /// </summary>
        [Parameter] public string? Label { get; set; }


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
        /// <para><c>IconFoundry="IconHelper.MIIcon()"</c></para>
        /// <para><c>IconFoundry="IconHelper.FAIcon()"</c></para>
        /// <para><c>IconFoundry="IconHelper.OIIcon()"</c></para>
        /// <para>Overrides <see cref="MBCascadingDefaults.IconFoundryName"/></para>
        /// </summary>
        [Parameter] public IMBIconFoundry? IconFoundry { get; set; }


        /// <summary>
        /// The autcomplete field's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }


        /// <summary>
        /// Ignores whitespace when searching the items list.
        /// </summary>
        [Parameter] public bool IgnoreWhitespace { get; set; } = false;


        /// <summary>
        /// Allow unmatched results.
        /// </summary>
        [Parameter] public bool AllowBlankResult { get; set; } = false;


        /// <summary>
        /// Forces the search string to match only from the start of each select item.
        /// </summary>
        [Parameter] public bool MatchFromItemStart { get; set; } = false;


        /// <summary>
        /// List of items to select from.
        /// </summary>
        [Parameter]
        public IEnumerable<string> SelectItems
        {
            get => selectItems;
            set
            {
                if (IsOpen)
                {
                    newSelectItems = value;
                }
                else
                {
                    selectItems = value;
                }
            }
        }
#nullable restore annotations

        /// <summary>
        /// When set, the value that the user enters does not have to match any of the selectable items.
        /// </summary>
        [Parameter] public bool AllowCustomValue { get; set; }


        private bool IsOpen { get; set; } = false;
        private DotNetObjectReference<MBAutocompleteTextField> ObjectReference { get; set; }
        private bool MenuHasFocus { get; set; } = false;
        private ElementReference MenuReference { get; set; }
        private SelectionItem[] MySelectItems { get; set; }
        private SelectionInfo SelectInfo { get; set; } = new SelectionInfo();
        private MBTextField TextField { get; set; }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ObjectReference = DotNetObjectReference.Create(this);

            ForceShouldRenderToTrue = true;
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            SetParameters();
        }


        private void SetParameters()
        {
            MySelectItems = (from i in SelectItems
                             select new SelectionItem
                             {
                                 Item = i,
                                 IgnoreWhitespace = IgnoreWhitespace
                             }).ToArray();

            SelectInfo = BuildSelectList(ComponentValue);

            StateHasChanged();
        }


        private SelectionInfo BuildSelectList(string fieldText)
        {
            var regexOptions = RegexOptions.IgnoreCase | (IgnoreWhitespace ? RegexOptions.IgnorePatternWhitespace : 0);

            var fullMatchRegex = new Regex($"^{fieldText}$", regexOptions);
            var fullMatches = (from f in MySelectItems
                               where fullMatchRegex.Matches(f.SearchTarget).Count > 0
                               select f.Item).ToArray();

            if (fullMatches.Any())
            {
                return new SelectionInfo()
                {
                    SelectedText = fullMatches.FirstOrDefault(),
                    SelectList = fullMatches
                };
            }

            var startMatch = MatchFromItemStart ? "^" : "";
            var partialMatchRegex = new Regex($"{startMatch}{fieldText}", regexOptions);
            var partialMatches = (from f in MySelectItems
                                  where partialMatchRegex.Matches(f.SearchTarget).Count > 0
                                  select f.Item).ToArray();
            var firstValueIsCustomValue = AllowCustomValue && fieldText != null && !partialMatches.Contains(fieldText);
            if (firstValueIsCustomValue)
            {
                partialMatches = partialMatches.Prepend(fieldText).ToArray();
            }

            return new SelectionInfo()
            {
                SelectedText = fieldText,
                SelectList = partialMatches,
                FirstValueIsCustomValue = firstValueIsCustomValue
            };
        }


        private async Task OnInput(ChangeEventArgs args)
        {
            SelectInfo = BuildSelectList((string)args.Value);

            if (SelectInfo.FullMatchFound || (AllowBlankResult && string.IsNullOrWhiteSpace(SelectInfo.SelectedText)))
            {
                await CloseMenuAsync();
                ComponentValue = SelectInfo.SelectedText.Trim();
                SetParameters();
            }

            await OpenMenuAsync();
        }


        private async Task OnTextChangeAsync()
        {
            await CloseMenuAsync(true);

            if (!MenuHasFocus)
            {
                if (SelectInfo.FullMatchFound || (AllowBlankResult && string.IsNullOrWhiteSpace(SelectInfo.SelectedText)))
                {
                    ComponentValue = SelectInfo.SelectedText.Trim();
                }

                SetParameters();
            }
        }


        private async Task OnTextFocusOutAsync()
        {
            if (!SelectInfo.SelectList.Any())
            {
                await CloseMenuAsync();
            }
        }


        private void OnMenuFocusIn()
        {
            MenuHasFocus = true;
        }


        private void OnMenuFocusOut()
        {
            MenuHasFocus = false;
        }


        /// <summary>
        /// For Material Theme to notify when the drop down is closed via JS Interop.
        /// </summary>
        /// <returns></returns>
        [JSInvokable]
        public void NotifyClosed()
        {
            IsOpen = false;

            SelectInfo.SelectedText = Value;

            if (newSelectItems != null)
            {
                selectItems = newSelectItems;
                newSelectItems = null;
            }

            StateHasChanged();
        }


        /// <summary>
        /// For Material Theme to notify of menu item selection via JS Interop.
        /// </summary>
        /// <returns></returns>
        [JSInvokable]
        public void NotifySelected(string value)
        {
            ComponentValue = value;

            NotifyClosed();
        }


        private async Task OpenMenuAsync(bool forceOpen = false)
        {
            if (!IsOpen || forceOpen)
            {
                IsOpen = true;
                await InvokeVoidAsync("MaterialBlazor.MBAutoCompleteTextField.open", MenuReference);
            }
        }


        private async Task CloseMenuAsync(bool forceClose = false)
        {
            if (IsOpen || forceClose)
            {
                IsOpen = false;
                await InvokeVoidAsync("MaterialBlazor.MBAutoCompleteTextField.close", MenuReference);
            }
        }


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBAutoCompleteTextField.init", TextField.ElementReference, MenuReference, ObjectReference);


        /// <inheritdoc/>
        private protected override void DisposeMcwComponent() => ObjectReference?.Dispose();
    }
}
