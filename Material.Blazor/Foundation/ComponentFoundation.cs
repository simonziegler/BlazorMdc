using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
    public abstract class ComponentFoundation : ComponentBase, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// A list of unmatched attributes that are used by and therefore essential for Material.Blazor. Works with 
        /// <see cref="MBCascadingDefaults.ConstrainSplattableAttributes"/> and <see cref="MBCascadingDefaults.AllowedSplattableAttributes"/>.
        /// </summary>
        /// <remarks>
        /// Includes "formnovalidate", "max", "min", "role", "step", "tabindex", "type", "data-prev-page" 
        /// </remarks>
        private static readonly ImmutableArray<string> EssentialSplattableAttributes = ImmutableArray.Create("formnovalidate", "max", "min", "role", "step", "tabindex", "type", "data-prev-page");
        private bool? disabled = null;


        [Inject] private IBatchingJSRuntime InjectedBatchingJSRuntime { get; set; }
        private protected IBatchingJSRuntime BatchingJSRuntime { get; set; }
        [CascadingParameter] private MBDialog ParentDialog { get; set; }
        [Inject] private protected IMBTooltipService TooltipService { get; set; }
        [Inject] private protected ILogger<ComponentFoundation> Logger { get; set; }


        [CascadingParameter] protected MBCascadingDefaults CascadingDefaults { get; set; } = new MBCascadingDefaults();


        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> UnmatchedAttributes { get; set; }


        /// <summary>
        /// Indicates whether the component is disabled.
        /// </summary>
        [Parameter]
        public bool? Disabled
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


        /// <summary>
        /// Gets a value for the component's 'id' attribute.
        /// </summary>
        internal string CrossReferenceId { get; set; } = Utilities.GenerateUniqueElementName();


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
        private protected virtual Task InstantiateMcwComponentAsync() => Task.CompletedTask;


        /// <summary>
        /// Components should override this for any async dispose operation.
        /// </summary>
        private protected virtual Task DisposeMcwComponentAsync() => Task.CompletedTask;


        /// <summary>
        /// Components should override this for any synchronous dispose operation.
        /// </summary>
        private protected virtual void DisposeMcwComponent() { }


        private bool _disposed;
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing && TooltipId != null)
            {
                TooltipService.RemoveTooltip(TooltipId.Value);
                TooltipId = null;

                _disposed = true;
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            // Perform async cleanup.
            await DisposeAsyncCore();

            // Dispose of unmanaged resources.
            Dispose(disposing: false);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }


        /// <inheritdoc/>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await BatchingJSRuntime.SemaphoreDispose(this, DisposeMcwComponentAsync, DisposeMcwComponent);
        }


        /// <summary>
        /// Local implementation of InvokeVoidAsync that passes this component to the batching js runtime which needs to track
        /// cross reference ids.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private protected async Task InvokeVoidAsync(string identifier, params object[] args)
        {
            await BatchingJSRuntime.InvokeVoidAsync(this, identifier, args);
        }


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


        /// <summary>
        /// Material.Blazor components use <see cref="OnParametersSetAsync()"/> only.
        /// </summary>
        protected sealed override void OnParametersSet()
        {
            // For consistency, we only ever use OnParametersSetAsync. To prevent ourselves from using OnParametersSet accidentally, we seal this method from here on.
        }


        /// <summary>
        /// Material.Blazor components use <see cref="OnInitializedAsync()"/> only.
        /// </summary>
        protected sealed override void OnInitialized()
        {
            BatchingJSRuntime = ParentDialog == null ? InjectedBatchingJSRuntime : new DialogAwareBatchingJSRuntime(InjectedBatchingJSRuntime, ParentDialog);
            // For consistency, we only ever use OnInitializedAsync. To prevent ourselves from using OnInitialized accidentally, we seal this method from here on.

            // the only thing we do here, is creating an ID for the tooltip, if we have one
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                TooltipId = TooltipIdProvider.NextId();
            }
        }


        /// <summary>
        /// When overriding this, call <c>await base.OnParametersSetAsync();</c> before any user code unless there is a very good reason not to.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            CheckAttributeValidity();
        }


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


        /// <summary>
        /// Material.Blazor components descending from <see cref="ComponentFoundation"/> _*must not*_ override <see cref="ComponentBase.OnAfterRender(bool)"/>.
        /// </summary>
        protected sealed override void OnAfterRender(bool firstRender)
        {
            // for consistency, we only ever use OnAfterRenderAsync. To prevent ourselves from using OnAfterRender accidentally, we seal this method from here on.
        }


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
                    await InstantiateMcwComponentAsync().ConfigureAwait(false);
                    HasInstantiated = true;
                    AddTooltip();
                }
                catch (Exception e)
                {
                    LogMBError(e, "Instantiating a component failed.");
                }
            }
        }


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


        /// <summary>
        /// Logs a critical message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBCritical(EventId eventId, string message, params object[] args) => Logger.LogCritical(eventId, $"MATERIAL.BLAZOR CRITICAL - {message}", args);


        /// <summary>
        /// Logs a critical message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBCritical(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogCritical(eventId, exception, $"MATERIAL.BLAZOR CRITICAL - {message}", args);


        /// <summary>
        /// Logs a critical message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBCritical(string message, params object[] args) => Logger.LogCritical($"MATERIAL.BLAZOR CRITICAL - {message}", args);


        /// <summary>
        /// Logs a critical message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBCritical(Exception exception, string message, params object[] args) => Logger.LogCritical(exception, $"MATERIAL.BLAZOR CRITICAL - {message}", args);


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if Logging
        protected void LogMBDebug(EventId eventId, string message, params object[] args) => Logger.LogDebug(eventId, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebug(EventId eventId, string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if Logging
        protected void LogMBDebug(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogDebug(eventId, exception, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebug(EventId eventId, Exception exception, string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if Logging
        protected void LogMBDebug(string message, params object[] args) => Logger.LogDebug($"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebug(string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if Logging
        protected void LogMBDebug(Exception exception, string message, params object[] args) => Logger.LogDebug(exception, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebug(Exception exception, string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if LoggingVerbose
        protected void LogMBDebugVerbose(EventId eventId, string message, params object[] args) => Logger.LogDebug(eventId, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebugVerbose(EventId eventId, string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if LoggingVerbose
        protected void LogMBDebugVerbose(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogDebug(eventId, exception, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebugVerbose(EventId eventId, Exception exception, string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if LoggingVerbose
        protected void LogMBDebugVerbose(string message, params object[] args) => Logger.LogDebug($"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebugVerbose(string message, params object[] args) { }
#endif


        /// <summary>
        /// Logs a debug message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
#if LoggingVerbose
        protected void LogMBDebugVerbose(Exception exception, string message, params object[] args) => Logger.LogDebug(exception, $"MATERIAL.BLAZOR DEBUG - {message}", args);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Null logging function")]
        protected void LogMBDebugVerbose(Exception exception, string message, params object[] args) { }
#endif

        /// <summary>
        /// Logs a error message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBError(EventId eventId, string message, params object[] args) => Logger.LogError(eventId, $"MATERIAL.BLAZOR ERROR - {message}", args);


        /// <summary>
        /// Logs a error message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBError(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogError(eventId, exception, $"MATERIAL.BLAZOR ERROR - {message}", args);


        /// <summary>
        /// Logs a error message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBError(string message, params object[] args) => Logger.LogError($"MATERIAL.BLAZOR ERROR - {message}", args);


        /// <summary>
        /// Logs a error message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBError(Exception exception, string message, params object[] args) => Logger.LogError(exception, $"MATERIAL.BLAZOR ERROR - {message}", args);


        /// <summary>
        /// Logs a information message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBInformation(EventId eventId, string message, params object[] args) => Logger.LogInformation(eventId, $"MATERIAL.BLAZOR INFORMATION - {message}", args);


        /// <summary>
        /// Logs a information message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBInformation(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogInformation(eventId, exception, $"MATERIAL.BLAZOR INFORMATION - {message}", args);


        /// <summary>
        /// Logs a information message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBInformation(string message, params object[] args) => Logger.LogInformation($"MATERIAL.BLAZOR INFORMATION - {message}", args);


        /// <summary>
        /// Logs a information message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBInformation(Exception exception, string message, params object[] args) => Logger.LogInformation(exception, $"MATERIAL.BLAZOR INFORMATION - {message}", args);


        /// <summary>
        /// Logs a trace message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBTrace(EventId eventId, string message, params object[] args) => Logger.LogCritical(eventId, $"MATERIAL.BLAZOR TRACE - {message}", args);


        /// <summary>
        /// Logs a trace message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBTrace(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogTrace(eventId, exception, $"MATERIAL.BLAZOR TRACE - {message}", args);


        /// <summary>
        /// Logs a trace message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBTrace(string message, params object[] args) => Logger.LogTrace($"MATERIAL.BLAZOR TRACE - {message}", args);


        /// <summary>
        /// Logs a trace message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBTrace(Exception exception, string message, params object[] args) => Logger.LogTrace(exception, $"MATERIAL.BLAZOR TRACE - {message}", args);


        /// <summary>
        /// Logs a warning message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBWarning(EventId eventId, string message, params object[] args) => Logger.LogWarning(eventId, $"MATERIAL.BLAZOR WARNING - {message}", args);


        /// <summary>
        /// Logs a warning message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBWarning(EventId eventId, Exception exception, string message, params object[] args) => Logger.LogWarning(eventId, exception, $"MATERIAL.BLAZOR WARNING - {message}", args);


        /// <summary>
        /// Logs a warning message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBWarning(string message, params object[] args) => Logger.LogWarning($"MATERIAL.BLAZOR WARNING - {message}", args);


        /// <summary>
        /// Logs a warning message identifying it as having come from Material.Blazor
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        protected void LogMBWarning(Exception exception, string message, params object[] args) => Logger.LogWarning(exception, $"MATERIAL.BLAZOR WARNING - {message}", args);
    }
}
