using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// The base class for all Material.Blazor components.
    /// </summary>
    public abstract class ComponentFoundation : ComponentBase, IDisposable
    {
        #region members
        /// <summary>
        /// A list of unmatched attributes that are used by and therefore essential for Material.Blazor. Works with 
        /// <see cref="MBCascadingDefaults.ConstrainSplattableAttributes"/> and <see cref="MBCascadingDefaults.AllowedSplattableAttributes"/>.
        /// </summary>
        /// <remarks>
        /// Includes "formnovalidate", "max", "min", "role", "step", "tabindex", "type", "data-prev-page" 
        /// </remarks>
        private static readonly ImmutableArray<string> EssentialSplattableAttributes = ImmutableArray.Create("formnovalidate", "max", "min", "role", "step", "tabindex", "type", "data-prev-page");
        private bool? disabled = null;

        [Inject] protected INoThrowDotNetObjectReferenceFactory NoThrowDotNetObjectReferenceFactory { get; set; }
        [Inject] private IBatchingJSRuntime InjectedJsRuntime { get; set; }
        protected IBatchingJSRuntime JsRuntime { get; set; }
        [CascadingParameter] private MBDialog ParentDialog { get; set; }
        [Inject] private protected ILogger<ComponentFoundation> Logger { get; set; }
        [Inject] private protected IMBTooltipService TooltipService { get; set; }
        [Inject] private protected IMBLoggingService LoggingService { get; set; }

        /// <summary>
        /// The current logging level. This is set in OnParametersSet
        /// </summary>
        private protected int LoggingLevel { get; set; }



        /// <summary>
        /// Gets a value for the component's 'id' attribute.
        /// </summary>
        private protected string CrossReferenceId { get; set; } = Utilities.GenerateUniqueElementName();


        /// <summary>
        /// Tooltip id for aria-describedby attribute.
        /// </summary>
        private long? TooltipId { get; set; }


        /// <summary>
        /// Determines whether to apply the disabled attribute.
        /// </summary>
        internal bool AppliedDisabled => CascadingDefaults.AppliedDisabled(Disabled);


        /// <summary>
        /// True if the component has been instantiated with a Material Components Web JSInterop call.
        /// </summary>
        private protected bool HasInstantiated { get; set; }


        /// <summary>
        /// Derived components can use this to get a callback from the <see cref="AppliedDisabled"/> setter when the consumer changes the value.
        /// This allows a component to take action with Material Theme js to update the DOM to reflect the data change visually. 
        /// </summary>
        private protected event Action OnDisabledSet;


        /// <summary>
        /// Allows a component to build or map out a group of CSS classes to be applied to the component. Use this in <see cref="ComponentBase.OnInitialized()"/>, <see cref="OnParametersSet()"/> or their asynchronous counterparts.
        /// </summary>
        private protected ConditionalCssClasses ConditionalCssClasses { get; } = new ConditionalCssClasses();


        /// <summary>
        /// Components should override this with a function to be called when Material.Blazor wants to run Material Components Web instantiation via JS Interop - always gets called from <see cref="OnAfterRenderAsync(bool)"/>, which should not be overridden.
        /// </summary>
        private protected virtual Task InstantiateMcwComponent() => Task.CompletedTask;


        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && TooltipId != null)
            {
                TooltipService.RemoveTooltip(TooltipId.Value);
                TooltipId = null;
            }

            _disposed = true;
        }

        #endregion

        #region parameters

        [CascadingParameter] protected MBCascadingDefaults CascadingDefaults { get; set; } = new MBCascadingDefaults();


        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> UnmatchedAttributes { get; set; }


        /// <summary>
        /// Indicates whether the component is disabled.
        /// </summary>
        [Parameter] public bool? Disabled
        {
            get => disabled;
            set
            {
                if (disabled != value)
                {
                    disabled = value;

                    if (HasInstantiated)
                    {
                        OnDisabledSet?.Invoke();
                    }
                }
            }
        }


        /// <summary>
        /// The HTML id attribute is used to specify a unique id for an HTML element.
        ///
        /// You cannot have more than one element with the same id in an HTML document.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [Parameter] public string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles


        /// <summary>
        /// Additional CSS classes for the component.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [Parameter] public string @class { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        protected string ActiveConditionalClasses => ConditionalCssClasses.ToString();


        /// <summary>
        /// Additional CSS style for the component.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [Parameter] public string style { get; set; }
#pragma warning restore IDE1006 // Naming Styles


        /// <summary>
        /// A markup capable tooltip.
        /// </summary>
        [Parameter] public string Tooltip { get; set; }

        #endregion

        #region AddTooltip

        /// <summary>
        /// Adds a tooltip if tooltip text has been provided.
        /// </summary>
        private protected void AddTooltip()
        {
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                TooltipService.AddTooltip(TooltipId.Value, (MarkupString)Tooltip);
            }
        }

        #endregion

        # region AttributesToSplat

        /// <summary>
        /// Attributes ready for splatting in components. Guaranteed not null, unlike UnmatchedAttributes.
        /// </summary>
        internal IEnumerable<KeyValuePair<string, object>> AttributesToSplat()
        {
            foreach (var attribute in UnmatchedAttributes ?? new Dictionary<string, object>())
            {
                yield return attribute;
            }

            if (AppliedDisabled)
            {
                yield return new KeyValuePair<string, object>("disabled", AppliedDisabled);
            }
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                yield return new KeyValuePair<string, object>("aria-describedby", $"mb-tooltip-{TooltipId.Value}");
            }
        }
        internal IEnumerable<KeyValuePair<string, object>> OtherAttributesToSplat()
        {
            foreach (var attribute in UnmatchedAttributes ?? new Dictionary<string, object>())
            {
                if (attribute.Key.StartsWith("on")) // this is only a heuristic, as it's not technically guaranteed that all attributes that are non-event do not start with "on". However, it is impossible to list all event names, as with .net 6, the list of event names is not limited anymore.
                {
                    continue;
                }
                yield return attribute;
            }

            if (AppliedDisabled)
            {
                yield return new KeyValuePair<string, object>("disabled", AppliedDisabled);
            }
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                yield return new KeyValuePair<string, object>("aria-describedby", $"mb-tooltip-{TooltipId.Value}");
            }
        }

        internal IEnumerable<KeyValuePair<string, object>> EventAttributesToSplat()
        {
            return UnmatchedAttributes ?? new Dictionary<string, object>()
                .Where(kvp => kvp.Key.StartsWith("on")); // this is only a heuristic, as it's not technically guaranteed that all attributes that are non-event do not start with "on". However, it is impossible to list all event names, as with .net 6, the list of event names is not limited anymore.
        }

        #endregion

        #region CheckAttributeValidity

        /// <summary>
        /// Material.Blazor allows a user to limit unmatched attributes that will be splatted to a defined list in <see cref="MBCascadingDefaults"/>.
        /// This method checks validity against that list.
        /// </summary>
        private void CheckAttributeValidity()
        {
            if (UnmatchedAttributes is null)
            {
                return;
            }

            if (UnmatchedAttributes.Keys.Contains("disabled"))
            {
                throw new ArgumentException(
                    $"Material.Blazor: You cannot use 'disabled' attribute in {Utilities.GetTypeName(GetType())}. Material.Blazor reserves the disabled attribute for internal use; use the 'Disabled' parameter instead");
            }

            if (!CascadingDefaults.ConstrainSplattableAttributes)
            {
                // nothing to check, as the ConstrainSplattableAttributes feature is disabled.
                return;
            }

            var forbidden =
                UnmatchedAttributes.Keys
                    .Where(n => !n.StartsWith("on"))         // heuristic: filter event attributes. Unlikely that other attributes will start with "on" as well.
                    .Where(n => !n.StartsWith("aria-"))      // heuristic: filter aria attributes. Unlikely that other attributes will start with "aria-" as well.
                    .Where(n => !n.StartsWith("__internal")) // heuristic: filter .NET __internal_stopPropagation_onclick and similar generated attribute names.
                    .Except(EssentialSplattableAttributes)   // filter common attribute names
                    .Except(CascadingDefaults.AllowedSplattableAttributes, StringComparer.InvariantCultureIgnoreCase); // filter user-specified attribute names, ignoring case

            if (forbidden.Any())
            {
                var message = $"You cannot use {string.Join(", ", forbidden.Select(x => $"'{x}'"))} attribute(s) in {Utilities.GetTypeName(GetType())}. Either remove the attribute or change 'ConstrainSplattableAttributes' or 'AllowedSplattableAttributes' in your MBCascadingDefaults";

                throw new ArgumentException($"Material.Blazor: {message}");
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region OnAfterRender

        /// <summary>
        /// Material.Blazor components descending from <see cref="ComponentFoundation"/> _*must not*_ override <see cref="ComponentBase.OnAfterRender(bool)"/>.
        /// </summary>
        protected sealed override void OnAfterRender(bool firstRender)
        {
            // for consistency, we only ever use OnAfterRenderAsync. To prevent ourselves from using OnAfterRender accidentally, we seal this method from here on.
        }

        #endregion

        #region OnAfterRenderAsync

        /// <summary>
        /// Material.Blazor components generally *should not* override this because it handles the case where components need
        /// to be adjusted when inside an <c>MBDialog</c> or <c>MBCard</c>. 
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await InstantiateMcwComponent().ConfigureAwait(false);
                    HasInstantiated = true;
                    AddTooltip();
                }
                catch (Exception e)
                {
                    LoggingService.LogError("Instantiating a component failed with exception " + e.ToString());
                }
            }
        }

        #endregion

        #region OnInitialized

        /// <summary>
        /// Material.Blazor components use <see cref="OnInitializedAsync()"/> only.
        /// </summary>
        protected sealed override void OnInitialized()
        {
            JsRuntime = ParentDialog == null ? InjectedJsRuntime : new DialogAwareBatchingJSRuntime(InjectedJsRuntime, ParentDialog);
            // For consistency, we only ever use OnInitializedAsync. To prevent ourselves from using OnInitialized accidentally, we seal this method from here on.

            // the only thing we do here, is creating an ID for the tooltip, if we have one
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                TooltipId = TooltipIdProvider.NextId();
            }

            LoggingService.SetLogger(Logger);
        }

        #endregion

        #region OnParametersSet

        /// <summary>
        /// Material.Blazor components use <see cref="OnParametersSetAsync()"/> only.
        /// </summary>
        protected sealed override void OnParametersSet()
        {
            // For consistency, we only ever use OnParametersSetAsync. To prevent ourselves from using OnParametersSet accidentally, we seal this method from here on.
        }

        #endregion

        #region OnParametersSetAsync

        /// <summary>
        /// When overriding this, call <c>await base.OnParametersSetAsync();</c> before any user code unless there is a very good reason not to.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            CheckAttributeValidity();

            LoggingLevel = LoggingService.CurrentLevel();
        }

        #endregion

    }
}
