using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// This is like InputBase from Microsoft.AspNetCore.Components.Forms, except that it treats
    /// [CascadingParameter] EditContext as optional.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InputComponent<T> : ComponentFoundation
    {
        #region members

        private bool _previousParsingAttemptFailed;
        private ValidationMessageStore _parsingValidationMessages;
        private Type _nullableUnderlyingType;
        private bool _hasSetInitialParameters;


        /// <summary>
        /// Gets the associated EditContext.
        /// </summary>
        protected EditContext EditContext { get; private set; }


        /// <summary>
        /// Gets the <see cref="FieldIdentifier"/> for the bound value.
        /// </summary>
        protected FieldIdentifier FieldIdentifier { get; private set; }


        /// <summary>
        /// Performs validation only if true. Used by <see cref="MBDebouncedTextField"/> to disable
        /// form validation for the embedded <see cref="MBTextField"/>, because a debounced field
        /// should not be in a form.
        /// </summary>
        private bool IgnoreFormField => this is MBDebouncedTextField or MultiSelectComponent<T, MBSelectElement<T>>;

        /// <summary>
        /// Allows <see cref="ShouldRender()"/> to return "true" habitually.
        /// </summary>
        private protected bool ForceShouldRenderToTrue { get; set; } = false;


        /// <summary>
        /// Allows <see cref="ShouldRender()"/> to return "true" for the next render only.
        /// </summary>
        private bool AllowNextRender = false;

        /// <summary>
        /// Gets a string that indicates the status of the field being edited. This will include
        /// some combination of "modified", "valid", or "invalid", depending on the status of the field.
        /// </summary>
        protected string FieldClass => !IgnoreFormField ? (EditContext?.FieldCssClass(FieldIdentifier) ?? string.Empty) : string.Empty;

        #endregion

        #region parameters

        [CascadingParameter] private EditContext CascadedEditContext { get; set; }


        /// <summary>
        /// Gets or sets the value of the input. This should be used with two-way binding.
        /// </summary>
        /// <example>
        /// @bind-Value="@model.PropertyName"
        /// </example>
        [Parameter] public T Value { get; set; }
        private T _cachedValue;


        /// <summary>
        /// Derived components can use this to get a callback from SetParameters(Async) when the consumer changes
        /// the value. This allows a component to take action with Material Theme js to update the DOM to reflect
        /// the data change visually. An example is a select where the relevant list item needs to be
        /// automatically clicked to get Material Theme to update the value shown in the
        /// <c>&lt;input&gt;</c> HTML tag.
        /// </summary>
        protected event Action SetComponentValue;


        /// <summary>
        /// Gets or sets a callback that updates the bound value.
        /// </summary>
        [Parameter] public EventCallback<T> ValueChanged { get; set; }


        /// <summary>
        /// Gets or sets an expression that identifies the bound value.
        /// </summary>
        [Parameter] public Expression<Func<T>> ValueExpression { get; set; }

        #endregion

        #region AllowNextShouldRender

        private protected void AllowNextShouldRender()
        {
            AllowNextRender = true;
        }

        #endregion

        #region ComponentValue

        /// <summary>
        /// Gets or sets the value of the component. To be used by Material.Blazor components for binding to
        /// native components, or to set the value in response to an event arising from the native component.
        /// </summary>

        private T _componentValue;
        private protected T ComponentValue
        {
            get => _componentValue;
            set
            {
                LoggingService.LogTrace($"ComponentValue setter entered: _componentValue is '{_cachedValue?.ToString() ?? "null"}' and new value is'{value?.ToString() ?? "null"}'");

                if (!EqualityComparer<T>.Default.Equals(value, _componentValue))
                {
                    LoggingService.LogTrace($"ComponentValue setter changed _componentValue");

                    _componentValue = value;
                    _ = ValueChanged.InvokeAsync(value);

                    if (EditContext != null && !IgnoreFormField)
                    {
                        if (string.IsNullOrWhiteSpace(FieldIdentifier.FieldName))
                        {
                            throw new Exception("Material.Blazor: ValueExpression must be defined for a field contained in an EditForm");
                        }
                        else
                        {
                            EditContext?.NotifyFieldChanged(FieldIdentifier);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets the current value of the input, represented as a string.
        /// </summary>
        protected string ComponentValueAsString
        {
            get => FormatValueToString(ComponentValue);
            set
            {
                _parsingValidationMessages?.Clear();

                bool parsingFailed;

                if (_nullableUnderlyingType != null && string.IsNullOrEmpty(value))
                {
                    // Assume if it's a nullable type, null/empty inputs should correspond to default(T)
                    // Then all subclasses get nullable support almost automatically (they just have to
                    // not reject Nullable<T> based on the type itself).
                    parsingFailed = false;
                    ComponentValue = default;
                }
                else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
                {
                    parsingFailed = false;
                    ComponentValue = parsedValue;
                }
                else
                {
                    parsingFailed = true;

                    if (EditContext != null && !IgnoreFormField)
                    {
                        if (_parsingValidationMessages == null)
                        {
                            _parsingValidationMessages = new ValidationMessageStore(EditContext);
                        }

                        _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage);

                        // Since we're not writing to ComponentValue, we'll need to notify about modification from here
                        EditContext.NotifyFieldChanged(FieldIdentifier);
                    }
                }

                // We can skip the validation notification if we were previously valid and still are
                if (parsingFailed || _previousParsingAttemptFailed)
                {
                    EditContext?.NotifyValidationStateChanged();
                    _previousParsingAttemptFailed = parsingFailed;
                }
            }
        }

        #endregion

        #region FormatValueToString

        /// <summary>
        /// Formats the value as a string. Derived classes can override this to determine the formating used for <see cref="ComponentValueAsString"/>.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value.</returns>
        protected virtual string FormatValueToString(T value)
            => value?.ToString();

        #endregion

        #region GetExpressionCustomAttributes

        /// <summary>
        /// Returns the custom attributes assocated with a field. Used by <see cref="MBTextArea"/> and <see cref="MBTextField"/> to
        /// look for a required attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accessor"></param>
        /// <returns></returns>
        private static IEnumerable<Attribute> GetExpressionCustomAttributes<TItem>(Expression<Func<TItem>> accessor)
        {
            var accessorBody = accessor.Body;

            // Unwrap casts to object
            if (accessorBody is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Type == typeof(object))
            {
                accessorBody = unaryExpression.Operand;
            }

            if (accessorBody is not MemberExpression memberExpression)
            {
                throw new ArgumentException($"The provided expression contains a {accessorBody.GetType().Name} which is not supported. {nameof(FieldIdentifier)} only supports simple member accessors (fields, properties) of an object.");
            }

            return memberExpression.Member.GetCustomAttributes();
        }

        #endregion

        #region HasRequiredAttribute

        /// <summary>
        /// Returns true if one of the custom attributes is the <see cref="RequiredAttribute"/>. Used by <see cref="MBTextArea"/> and <see cref="MBTextField"/> to
        /// look for a required attribute.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="accessor"></param>
        /// <returns></returns>
        private protected static bool HasRequiredAttribute<TItem>(Expression<Func<TItem>> accessor)
        {
            if (accessor == null)
            {
                return false;
            }

            var customAttributes = GetExpressionCustomAttributes<TItem>(accessor);

            return customAttributes.Any(a => a.GetType() == typeof(RequiredAttribute));
        }

        #endregion

        #region OnInitializedAsync

        /// <para>
        /// Components must call base.OnInitializedAsync() otherwise rendering in dialogs will be unpredictable.
        /// </para>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (EditContext != null && IgnoreFormField)
            {
                LoggingService.LogWarning($"{GetType()} is in a form but has EditContext features disabled because it is considered a valid Material.Blazor form field type");
            }
        }

        #endregion

        #region OnParametersSetAsync

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            LoggingService.LogTrace($"OnParametersSetAsync setter entered: _cachedValue is '{_cachedValue?.ToString() ?? "null"}' and Value is'{Value?.ToString() ?? "null"}'");

            if (!EqualityComparer<T>.Default.Equals(_cachedValue, Value))
            {
                _cachedValue = Value;

                LoggingService.LogTrace($"OnParametersSetAsync changed _cachedValue value");

                if (!EqualityComparer<T>.Default.Equals(_componentValue, Value))
                {
                    LoggingService.LogTrace("OnParametersSetAsync update _componentValue value from '" + _componentValue?.ToString() ?? "null" + "'");

                    _componentValue = Value;
                    if (HasInstantiated)
                    {
                        SetComponentValue?.Invoke();
                    }
                }
            }
        }

        #endregion

        #region SetParametersAsync

        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor.
        //
        // This implementation of SetParametersAsync is largely untouched from our original fork of Steve Sanderson's
        // RazorComponents.MaterialDesign repo. We've added the storage of a cached Value for use in
        // OnSetParameters/OnSetParametersAsync.
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (!_hasSetInitialParameters)
            {
                // This is the first run
                // Could put this logic in OnInit, but its nice to avoid forcing people who override OnInit to call base.OnInit()

                if (ValueExpression != null)
                {
                    FieldIdentifier = FieldIdentifier.Create(ValueExpression);
                }

                EditContext = CascadedEditContext;
                _nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(T));
                _hasSetInitialParameters = true;

                LoggingService.LogTrace($"SetParametersAsync setting ComponentValue value to '{Value?.ToString() ?? "null"}'");

                _cachedValue = Value;
                _componentValue = Value;
            }
            else if (CascadedEditContext != EditContext)
            {
                // Not the first run, this is a re-render caused by the parent re-render

                // We don't support changing EditContext because it's messy to be clearing up state and event
                // handlers for the previous one, and there's no strong use case. If a strong use case
                // emerges, we can consider changing this.
                throw new InvalidOperationException($"{GetType()} does not support changing the {nameof(EditContext)} dynamically.");
            }

            // For derived components, retain the usual lifecycle with OnInit/OnParametersSet/etc.
            await base.SetParametersAsync(ParameterView.Empty);
        }

        #endregion

        #region ShouldRender

        /// <summary>
        /// Material.Blazor components descending from MdcInputComponentBase _*must not*_ override ShouldRender().
        /// </summary>
        protected sealed override bool ShouldRender()
        {
            if (ForceShouldRenderToTrue || AllowNextRender)
            {
                AllowNextRender = false;
                return true;
            }

            return false;
        }

        #endregion

        #region TryParseValueFromString

        /// <summary>
        /// Parses a string to create an instance of <typeparamref name="T"/>. Derived classes can override this to change how
        /// <see cref="ComponentValueAsString"/> interprets incoming values.
        /// </summary>
        /// <param name="value">The string value to be parsed.</param>
        /// <param name="result">An instance of <typeparamref name="T"/>.</param>
        /// <param name="validationErrorMessage">If the value could not be parsed, provides a validation error message.</param>
        /// <returns>True if the value could be parsed; otherwise false.</returns>
        protected virtual bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
            => throw new NotImplementedException($"This component does not parse string inputs. Bind to the '{nameof(ComponentValue)}' property, not '{nameof(ComponentValueAsString)}'.");

        #endregion

    }
}
