using Material.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Material.Blazor
{
    public partial class MBBatchingWrapper : ComponentBase
    {
        [Inject] private IBatchingJSRuntime InjectedJsRuntime { get; set; }
        protected IBatchingJSRuntime BatchingJsRuntime { get; set; }
        [CascadingParameter] private MBDialog ParentDialog { get; set; }


        /// <summary>
        /// Gets a value for the component's 'id' attribute.
        /// </summary>
        internal readonly string CrossReferenceId = Utilities.GenerateUniqueElementName();


        /// <summary>
        /// The child content containing Material.Blazor components whose JS Interop calls are to be batched.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }


        /// <summary>
        /// Invokes SHC.
        /// </summary>
        /// <returns></returns>
        internal void InvokeStateHasChanged()
        {
            //return InvokeAsync(StateHasChanged);
            StateHasChanged();
        }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            BatchingJsRuntime = ParentDialog == null ? InjectedJsRuntime : new DialogAwareBatchingJSRuntime(InjectedJsRuntime, ParentDialog);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            await BatchingJsRuntime.FlushBatch(this);
        }
    }
}
