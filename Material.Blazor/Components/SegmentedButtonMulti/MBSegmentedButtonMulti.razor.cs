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
    public partial class MBSegmentedButtonMulti<TItem> : MultiSelectComponent<TItem, MBIconBearingSelectElement<TItem>>
    {
        /// <summary>
        /// If this component is rendered inside a single-select segmented button, add the "" class.
        /// </summary>
        [CascadingParameter] private MBSegmentedButtonSingle<TItem> SegmentedButtonSingle { get; set; }

        /// <summary>
        /// Inclusion of touch target
        /// </summary>
        [Parameter] public bool? TouchTarget { get; set; }


        private bool AppliedTouchTarget => CascadingDefaults.AppliedTouchTarget(TouchTarget);
        private MBIconBearingSelectElement<TItem>[] ItemsArray { get; set; }
        private bool IsSingleSelect { get; set; }
        private IDisposable ObjectReference { get; set; }
        private string GroupRole => (SegmentedButtonSingle == null) ? "group" : "radiogroup";
        private Dictionary<string, object>[] SegmentAttributes { get; set; }
        private ElementReference SegmentedButtonReference { get; set; }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            IsSingleSelect = SegmentedButtonSingle != null;

            ConditionalCssClasses
                .AddIf("mdc-segmented-button--single-select", () => IsSingleSelect);

            ItemsArray = Items.ToArray();

            SegmentAttributes = new Dictionary<string, object>[ItemsArray.Length];

            for (int i = 0; i < ItemsArray.Length; i++)
            {
                SegmentAttributes[i] = new();

                var selected = Value.Contains(ItemsArray[i].SelectedValue);

                SegmentAttributes[i].Add("class", "mdc-segmented-button__segment mdc-segmented-button--touch" + (selected ? " mdc-segmented-button__segment--selected" : ""));

                if (IsSingleSelect)
                {
                    SegmentAttributes[i].Add("role", "radio");
                    SegmentAttributes[i].Add("aria-checked", selected.ToString().ToLower());
                }
                else
                {
                    SegmentAttributes[i].Add("aria-pressed", selected.ToString().ToLower());
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
        protected void OnValueSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBSegmentedButtonMulti.setSelected", SegmentedButtonReference, Items.Select(x => Value.Contains(x.SelectedValue)).ToArray()));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBSegmentedButtonMulti.setDisabled", SegmentedButtonReference, AppliedDisabled));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBSegmentedButtonMulti.init", SegmentedButtonReference, IsSingleSelect, ObjectReference);


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
