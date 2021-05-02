using Material.Blazor.Internal;

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Material.Blazor
{
    /// <summary>
    /// This is a general purpose Material Theme list implementing one and two line MB web component
    /// standards. It also implements a Material.Blazor interpretation of the specification for a three line
    /// list item.
    /// </summary>
    public partial class MBList<TItem> : ComponentFoundation
    {
        /// <summary>
        /// The list style.
        /// <para>Overrides <see cref="MBCascadingDefaults.ListStyle"/></para>
        /// </summary>
        [Parameter] public MBListStyle? ListStyle { get; set; }


        /// <summary>
        /// The list type.
        /// <para>Overrides <see cref="MBCascadingDefaults.ListType"/></para>
        /// </summary>
        [Parameter] public MBListType? ListType { get; set; }


        /// <summary>
        /// A function delegate to return the parameters for <c>@key</c> attributes. If unused
        /// "fake" keys set to GUIDs will be used.
        /// </summary>
        [Parameter] public Func<TItem, object> GetKeysFunc { get; set; }


        /// <summary>
        /// The items to display in the list.
        /// </summary>
        [Parameter] public IEnumerable<TItem> Items { get; set; }


        /// <summary>
        /// The icon render fragment to use if <c>!<see cref="SuppressIcons"/></c>.
        /// Note that you will be expected to render your own icon, and can use <see cref="MBIcon"/>.
        /// </summary>
        [Parameter] public RenderFragment<TItem> Icon { get; set; }


        /// <summary>
        /// The title line render fragment.
        /// </summary>
        [Parameter] public RenderFragment<TItem> Title { get; set; }


        /// <summary>
        /// The "line two" render fragment.
        /// </summary>
        [Parameter] public RenderFragment<TItem> LineTwo { get; set; }


        /// <summary>
        /// The "line three" render fragment.
        /// </summary>
        [Parameter] public RenderFragment<TItem> LineThree { get; set; }


        /// <summary>
        /// The actions render fragment.
        /// </summary>
        [Parameter] public RenderFragment<TItem> Actions { get; set; }


        /// <summary>
        /// An @onclick event handler returning the index of the relevant list item.
        /// </summary>
        [Parameter] public EventCallback<int> OnClick { get; set; }


        /// <summary>
        /// An @onmousedown event handler returning the index of the relevant list item.
        /// </summary>
        [Parameter] public EventCallback<int> OnMouseDown { get; set; }


        /// <summary>
        /// An @onkeydown event handler returning the index of the relevant list item.
        /// </summary>
        [Parameter] public EventCallback<int> OnKeyDown { get; set; }


        /// <summary>
        /// An @ontouchstart event handler returning the index of the relevant list item.
        /// </summary>
        [Parameter] public EventCallback<int> OnTouchStart { get; set; }


        /// <summary>
        /// Shows a <see cref="MBListDivider"/> between list items if True. Defaults to False.
        /// </summary>
        [Parameter] public bool ShowSeparators { get; set; } = false;


        /// <summary>
        /// Allows keyboard interactions if True. Defaults to False.
        /// </summary>
        [Parameter] public bool KeyboardInteractions { get; set; } = false;


        /// <summary>
        /// Applies ripple to the list item if True. Defaults to False.
        /// </summary>
        [Parameter] public bool Ripple { get; set; } = false;


        /// <summary>
        /// Sets the non interative Material Theme class if True. Defaults to False.
        /// </summary>
        [Parameter] public bool NonInteractive { get; set; }


        /// <summary>
        /// Suppresses icon display if True. Defaults to False.
        /// </summary>
        [Parameter] public bool SuppressIcons { get; set; }


        /// <summary>
        /// The lists's density if it is a single line list. Ignored if <see cref="ListType"/> is <see cref="MBListType.Dense"/>
        /// </summary>
        [Parameter] public MBDensity? SingleLineDensity { get; set; }


        /// <summary>
        /// Hides "line two" if True. Defaults to False.
        /// </summary>
        [Parameter] public bool HideLineTwo { get; set; } = false;


        /// <summary>
        /// Hides "line three" if True. Defaults to False.
        /// </summary>
        [Parameter] public bool HideLineThree { get; set; } = false;



        private MBListStyle AppliedListStyle => CascadingDefaults.AppliedStyle(ListStyle);
        private MBListType AppliedListType => CascadingDefaults.AppliedType(ListType);
        private ElementReference ElementReference { get; set; }
        private MBCascadingDefaults.DensityInfo DensityInfo => CascadingDefaults.GetDensityCssClass(CascadingDefaults.AppliedListSingleLineDensity(SingleLineDensity));
        private int NumberOfLines { get; set; }
        private bool HasLineTwo { get; set; }
        private bool HasLineThree { get; set; }
        private Func<TItem, object> KeyGenerator { get; set; }
        private string TitleClass { get; set; }
        private string LineTwoClass { get; set; }
        private string LineThreeClass { get; set; }
        private string ListItemClass => "mdc-list-item__text mb-full-width" + (AppliedDisabled ? " mdc-list-item--disabled" : "");


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ConditionalCssClasses
                .AddIf(DensityInfo.CssClassName, () => DensityInfo.ApplyCssClass && NumberOfLines == 1 && AppliedListType != MBListType.Dense)
                .AddIf("mdc-card--outlined", () => (CascadingDefaults.AppliedStyle(AppliedListStyle) == MBListStyle.Outlined))
                .AddIf("mdc-list--two-line", () => (NumberOfLines == 2))
                .AddIf("mb-list--three-line", () => (NumberOfLines == 3))
                .AddIf("mdc-list--non-interactive", () => NonInteractive)
                .AddIf("mdc-list--dense", () => AppliedListType == MBListType.Dense)
                .AddIf("mdc-list--avatar-list", () => AppliedListType == MBListType.Avatar);
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            HasLineTwo = LineTwo != null && !HideLineTwo;
            HasLineThree = LineThree != null && !HideLineThree;

            NumberOfLines = 1;
            if (HasLineTwo)
            {
                NumberOfLines++;
            }

            if (HasLineThree)
            {
                NumberOfLines++;
            }

            TitleClass = (NumberOfLines == 1) ? "" : "mdc-list-item__primary-text";
            LineTwoClass = "mdc-list-item__secondary-text mb-full-width";
            LineThreeClass = "mdc-list-item__secondary-text" + ((NumberOfLines == 3) ? " line-three" : "") + " mb-full-width";

            KeyGenerator = GetKeysFunc ?? delegate (TItem item) { return item; };
        }


        private async Task OnItemClickAsync(int index)
        {
            if (KeyboardInteractions && !AppliedDisabled)
            {
                await OnClick.InvokeAsync(index);
            }
        }

        private async Task OnItemMouseDownAsync(int index)
        {
            if (KeyboardInteractions && !AppliedDisabled)
            {
                await OnMouseDown.InvokeAsync(index);
            }
        }

        private async Task OnItemKeyDownAsync(int index)
        {
            if (KeyboardInteractions && !AppliedDisabled)
            {
                await OnKeyDown.InvokeAsync(index);
            }
        }

        private async Task OnItemTouchStartAsync(int index)
        {
            if (KeyboardInteractions && !AppliedDisabled)
            {
                await OnTouchStart.InvokeAsync(index);
            }
        }


        /// <summary>
        /// Callback for value the Disabled value setter. MBList is a special case where Material.Blazor re-renders the component when Disabled is set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback() => InvokeAsync(() => InvokeVoidAsync("MaterialBlazor.MBList.init", ElementReference, KeyboardInteractions && !AppliedDisabled, Ripple));


        /// <inheritdoc/>
        private protected override Task InstantiateMcwComponentAsync() => InvokeVoidAsync("MaterialBlazor.MBList.init", ElementReference, KeyboardInteractions && !AppliedDisabled, Ripple);
    }
}
