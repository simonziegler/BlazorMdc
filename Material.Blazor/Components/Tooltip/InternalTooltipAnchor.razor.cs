using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// An anchor component that displays tooltips that you add, and using 
    /// <see cref="IMBTooltipService"/>.
    /// Place this component at the top of either App.razor or MainLayout.razor.
    /// </summary>
    public partial class InternalTooltipAnchor : ComponentFoundation
    {
        private ConcurrentQueue<KeyValuePair<long, TooltipInstance>> NewTooltips { get; } = new();
        private ConcurrentQueue<long> OldTooltips { get; } = new();
        private Dictionary<long, TooltipInstance> Tooltips { get; } = new();


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            TooltipService.OnAddRenderFragment += AddTooltipRenderFragment;
            TooltipService.OnAddMarkupString += AddTooltipMarkupString;
            TooltipService.OnRemove += RemoveTooltip;
        }



        /// <summary>
        /// Adds a tooltip to the anchor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        private void AddTooltipRenderFragment(long id, RenderFragment content)
        {
            NewTooltips.Enqueue(new KeyValuePair<long, TooltipInstance>(id, new TooltipInstance
            {
                RenderFragmentContent = content,
                Initiated = false
            }));
            _ = InvokeAsync(StateHasChanged);
        }



        /// <summary>
        /// Adds a tooltip to the anchor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        private void AddTooltipMarkupString(long id, MarkupString content)
        {
            NewTooltips.Enqueue(new KeyValuePair<long, TooltipInstance>(id, new TooltipInstance
            {
                MarkupStringContent = content,
                Initiated = false
            }));
            _ = InvokeAsync(StateHasChanged);
        }


        /// <summary>
        /// Removes a tooltip from the anchor.
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveTooltip(long id)
        {
            try
            {
                OldTooltips.Enqueue(id);
                _ = InvokeAsync(StateHasChanged);
            }
            catch (ObjectDisposedException ex)
            {
                // Ignore ObjectDisposedException to avoid exceptions being thrown when the user closes browser and tooltips are showing.
            }
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var refs = (from tooltip in Tooltips.Values
                        where !tooltip.Initiated &&
                              !string.IsNullOrWhiteSpace(tooltip.ElementReference.Id)
                        select tooltip).ToArray();

            if (refs.Length > 0)
            {
                await InvokeVoidAsync("MaterialBlazor.MBTooltip.init", refs.Select(r => r.ElementReference));

                foreach (var item in refs)
                {
                    item.Initiated = true;
                }
            }
        }


        /// <summary>
        /// Before we render any tooltip, let's update the list of tooltips that need to be rendered.
        /// </summary>
        private void OnBeforeRender()
        {
            while (NewTooltips.TryDequeue(out var kvp))
            {
                var (key, tooltip) = kvp;
                Tooltips.Add(key, tooltip);
            }
            while (OldTooltips.TryDequeue(out var key))
            {
                Tooltips.Remove(key);
            }
        }
    }
}
