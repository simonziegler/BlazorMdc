using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// A Material Theme segmented button orientated as a multi-select.
    /// </summary>
    public partial class MBChipsSelectMulti<TItem> : MultiSelectComponent<TItem, MBIconBearingSelectElement<TItem>>
    {
        /// <summary>
        /// If this component is rendered inside a single-select segmented button, add the "" class.
        /// </summary>
        [CascadingParameter] private MBChipsSelectSingle<TItem> ChipsSelectSingle { get; set; }

        /// <summary>
        /// Inclusion of touch target
        /// </summary>
        [Parameter] public bool? TouchTarget { get; set; }


        private bool AppliedTouchTarget => CascadingDefaults.AppliedTouchTarget(TouchTarget);
        private Dictionary<string, object>[] ChipAttributes { get; set; }
        private Dictionary<string, object>[] ChipIconAttributes { get; set; }
        private Dictionary<string, object>[] ChipSpanAttributes { get; set; }
        private ElementReference ChipsReference { get; set; }
        private MBIconBearingSelectElement<TItem>[] ItemsArray { get; set; }
        private bool IsSingleSelect { get; set; }
        private DotNetObjectReference<MBChipsSelectMulti<TItem>> ObjectReference { get; set; }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            IsSingleSelect = ChipsSelectSingle != null;

            ConditionalCssClasses
                .AddIf("mdc-chip-set--filter", () => !IsSingleSelect)
                .AddIf("mdc-chip-set--choice", () => IsSingleSelect);

            ItemsArray = Items.ToArray();

            ChipAttributes = new Dictionary<string, object>[ItemsArray.Length];
            ChipIconAttributes = new Dictionary<string, object>[ItemsArray.Length];
            ChipSpanAttributes = new Dictionary<string, object>[ItemsArray.Length];

            for (int i = 0; i < ItemsArray.Length; i++)
            {
                ChipAttributes[i] = new();
                ChipIconAttributes[i] = new();
                ChipSpanAttributes[i] = new();

                var selected = Value.Contains(ItemsArray[i].SelectedValue);

                ChipAttributes[i].Add("class", "mdc-chip mdc-chip--touch" + (selected ? " mdc-chip--selected" : ""));
                ChipAttributes[i].Add("role", "row");

                ChipIconAttributes[i].Add("class", "mdc-chip__icon mdc-chip__icon--leading" + (selected ? " mdc-chip__icon--leading-hidden" : ""));

                ChipSpanAttributes[i].Add("class", "mdc-chip__primary-action");
                ChipSpanAttributes[i].Add("tabindex", "0");
                ChipSpanAttributes[i].Add("aria-checked", selected.ToString().ToLower());

                if (IsSingleSelect)
                {
                    ChipSpanAttributes[i].Add("role", "button");
                }
                else
                {
                    ChipSpanAttributes[i].Add("role", "checkbox");
                }
            }

            SetComponentValue += OnValueSetCallback;
            OnDisabledSet += OnDisabledSetCallback;

            ObjectReference = DotNetObjectReference.Create(this);
        }


        /// <summary>
        /// For Material Theme to notify of menu item selection via JS Interop.
        /// </summary>
        [JSInvokable]
        public void NotifyMultiSelected(bool[] selected)
        {
            var selectedIndexes = Enumerable.Range(0, selected.Length).Where(i => selected[i]);
            ComponentValue = ItemsArray.Where((item, index) => selectedIndexes.Contains(index)).Select(x => x.SelectedValue).ToArray();
        }


        /// <summary>
        /// For Material Theme to notify of menu item selection via JS Interop.
        /// </summary>
        [JSInvokable]
        public void NotifySingleSelected(int index)
        {
            ComponentValue = new TItem[] { ItemsArray[index].SelectedValue };
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBChipsSelectMulti.setSelected", ChipsReference, Items.Select(x => Value.Contains(x.SelectedValue)).ToArray()));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBChipsSelectMulti.setDisabled", ChipsReference, AppliedDisabled));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBChipsSelectMulti.init", ChipsReference, IsSingleSelect, ObjectReference);


        /// <inheritdoc/>
        private protected override void DisposeMcwComponent() => ObjectReference?.Dispose();


        /// <summary>
        /// Used by <see cref="MBSegmentedButtonSingle{TItem}"/> to set the value.
        /// </summary>
        /// <param name="value"></param>
        internal void SetSingleSelectValue(TItem value)
        {
            Value = new TItem[] { value };
            OnValueSetCallback();
        }
    }
}
