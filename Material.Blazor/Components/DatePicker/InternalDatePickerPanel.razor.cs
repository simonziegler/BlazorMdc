using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    /// <summary>
    /// For Material.Blazor internal use only.
    /// </summary>
    public partial class InternalDatePickerPanel : InputComponent<DateTime>
    {
        /// <summary>
        /// Date selection criteria
        /// </summary>
        [Parameter] public MBDateSelectionCriteria? DateSelectionCriteria { get; set; }


#nullable enable annotations
        /// <summary>
        /// Control whether a date is selectable by evaluating the method.
        /// </summary>
        [Parameter] public Func<DateTime, bool>? DateIsSelectable { get; set; }
#nullable restore annotations


        /// <summary>
        /// Minimum date set by the consumer
        /// </summary>
        [Parameter] public DateTime MinDate { get; set; }


        /// <summary>
        /// Maximum date set by the consumer
        /// </summary>
        [Parameter] public DateTime MaxDate { get; set; }


        /// <summary>
        /// Reference to the <c>&lt;li&gt;</c> embedded in the panel.
        /// </summary>
        internal ElementReference ListItemReference { get; set; }


        /// <summary>
        /// Specification for date format
        /// </summary>
        [Parameter] public string DateFormat { get; set; }


        private bool ScrollToYear { get; set; } = false;

        private string[] DaysOfWeek { get; set; }

        private bool PreviousMonthDisabled => (StartOfDisplayMonth <= MinDate);

        private bool NextMonthDisabled => (StartOfDisplayMonth.AddMonths(1) >= MaxDate);

        private bool _showYearPad = false;
        internal bool ShowYearPad
        {
            get => _showYearPad;
            set
            {
                _showYearPad = value;
                ScrollToYear = value;
            }
        }

        private List<DateTime> Dates { get; set; } = new List<DateTime>();

        private List<int> Years { get; set; } = new List<int>();

        private DateTime InitialDate { get; set; }

        private DateTime CachedComponentValue { get; set; }

        private DateTime CachedMinDate { get; set; }

        private DateTime CachedMaxDate { get; set; }

        private string CachedComponentValueText => Utilities.DateToString(CachedComponentValue, DateFormat);

        private int MonthsOffset { get; set; } = 0;

        private DateTime StartOfDisplayMonth { get; set; }

        private string MonthText => StartOfDisplayMonth.ToString("MMMM yyyy");

        private bool IsFirstParametersSet { get; set; } = true;


        private readonly string currentYearId = Utilities.GenerateUniqueElementName();

        private readonly IMBIconFoundry foundry = MBIconHelper.MIFoundry(MBIconMITheme.Filled);


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            DaysOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames.Select(d => d[0..1]).ToArray();
            var rotate_by = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            if (rotate_by > 0)
            {
                DaysOfWeek = DaysOfWeek.Skip(rotate_by).Concat(DaysOfWeek.Take(rotate_by)).ToArray();
            }

            ForceShouldRenderToTrue = true;
        }


        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (IsFirstParametersSet)
            {
                IsFirstParametersSet = false;
                SetParameters(true);
            }
        }


        internal void SetParameters(bool forceSetup, DateTime? newValue = null)
        {
            if (forceSetup || ComponentValue != CachedComponentValue || MinDate != CachedMinDate || MaxDate != CachedMaxDate)
            {
                if (newValue != null)
                {
                    Value = (DateTime)newValue;
                }

                InitialDate = ComponentValue;
                CachedComponentValue = ComponentValue;
                CachedMinDate = MinDate;
                CachedMaxDate = MaxDate;
                var startDate = StartOfDisplayMonth = new DateTime(ComponentValue.Year, ComponentValue.Month, 1).AddMonths(MonthsOffset);
                while (startDate.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                {
                    startDate = startDate.AddDays(-1);
                }
                var endDate = startDate.AddDays(6 * 7); // 6 lines of 7 days each

                Dates = new List<DateTime>();

                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    Dates.Add(date);
                }

                var startYear = ((MinDate.Year - 1) / 4) * 4 + 1;
                var endYear = ((MaxDate.Year + 3) / 4) * 4 + 1;

                Years = new List<int>();

                for (var year = startYear; year < endYear; year++)
                {
                    Years.Add(year);
                }

                ShowYearPad = false;

                StateHasChanged();
            }
        }


        private async Task OnDayItemClickAsync(DateTime dateTime)
        {
            // Invoke JS first. if ComponentValue is set first we are at risk of this element being re-rendered before this line is run, making ListItemReference stale and causing a JS exception.
            await InvokeVoidAsync("MaterialBlazor.MBDatePicker.listItemClick", ListItemReference, Utilities.DateToString(dateTime, DateFormat));
            ComponentValue = dateTime;
            CachedComponentValue = Value;
            MonthsOffset = 0;
            SetParameters(true);
        }


        private void OnYearItemClick(int year)
        {
            MonthsOffset += (year - StartOfDisplayMonth.Year) * 12;
            ShowYearPad = false;
            SetParameters(true);
        }


        private void OnPreviousMonthClick()
        {
            MonthsOffset--;
            SetParameters(true);
        }


        private void OnShowCurrentDateClick()
        {
            MonthsOffset = 0;
            SetParameters(true);
        }


        private void OnNextMonthClick()
        {
            MonthsOffset++;
            SetParameters(true);
        }


        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender) // TODO: this prevents us from marking OnAfterRenderAsync in InputComponent as sealed. Consider alternatives!
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender && ScrollToYear)
            {
                ScrollToYear = false;

                await InvokeVoidAsync("MaterialBlazor.MBDatePicker.scrollToYear", currentYearId);
            }
        }
    }
}
