using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Material.Blazor.Internal
{
    internal static class GroupingExtension
    {
        public static IEnumerable<T[]> InGroupsOf<T>(this IEnumerable<T> enumerable, int groupSize)
        {
            var group = new T[groupSize];
            int index = 0;
            foreach (var element in enumerable)
            {
                group[index] = element;
                ++index;
                if (index == groupSize)
                {
                    yield return group;
                    index = 0;
                    group = new T[groupSize];
                }
            }
            if (index > 0)
            {
                // the last group has less than groupSize elements, therefore we need to return a trimmed array.
                yield return group.Take(index).ToArray();
            }
        }
    }


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
        /// Set to indicate that if the value is default(DateTime) then no date is initially shown
        /// and the panel will start with the current year and month
        /// </summary>
        [Parameter] public bool SupressDefaultDate { get; set; }


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

        private bool PreviousMonthDisabled => false
            || (StartOfDisplayMonth.Year == DateTime.MinValue.Year && StartOfDisplayMonth.Month == DateTime.MinValue.Month)
            || (StartOfDisplayMonth <= MinDate);

        private bool NextMonthDisabled => false
            || (StartOfDisplayMonth.Year == DateTime.MaxValue.Year && StartOfDisplayMonth.Month == DateTime.MaxValue.Month) // special case:
            || (StartOfDisplayMonth.AddMonths(1) >= MaxDate);

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

        private List<int[]> YearsInGroupsOfFour { get; set; } = new List<int[]>();

        private string ValueText => Utilities.DateToString(Value, DateFormat);

        private int MonthsOffset { get; set; } = 0;

        private DateTime StartOfDisplayMonth { get; set; }

        private string MonthText => StartOfDisplayMonth.ToString("MMMM yyyy");

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

            SetParameters();
        }


        internal void SetParameters(DateTime? newValue = null)
        {
            if (newValue != null)
            {
                Value = (DateTime)newValue;
            }

            DateTime startDate;
            int startDateYear;
            int startDateMonth;
            var today = DateTime.Today;

            if (SupressDefaultDate &&
                (Value == default) &&
                (today >= MinDate) &&
                (today <= MaxDate))
            {
                startDateYear = today.Year;
                startDateMonth = today.Month;
            }
            else
            {
                startDateYear = Value.Year;
                startDateMonth = Value.Month;
            }

            try
            {
                startDate = StartOfDisplayMonth = new DateTime(startDateYear, startDateMonth, 1).AddMonths(MonthsOffset);
            }
            catch (ArgumentOutOfRangeException)
            {
                startDate = StartOfDisplayMonth = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, 1);
            }
            while (startDate.Date > DateTime.MinValue.Date && startDate.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
            {
                startDate = startDate.AddDays(-1);
            }
            DateTime endDate;
            try
            {
                endDate = startDate.AddDays(6 * 7); // 6 lines of 7 days each
            }
            catch (ArgumentOutOfRangeException)
            {
                endDate = DateTime.MaxValue.Date;
            }

            Dates = new List<DateTime>();

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                Dates.Add(date);
            }

            var startYear = Math.Max(DateTime.MinValue.Year, ((MinDate.Year - 1) / 4) * 4 + 1);
            var endYear = Math.Min(DateTime.MaxValue.Year, ((MaxDate.Year + 3) / 4) * 4);

            Years = new List<int>();

            for (var year = startYear; year <= endYear; year++)
            {
                Years.Add(year);
            }
            YearsInGroupsOfFour = Years.InGroupsOf(4).ToList();

            ShowYearPad = false;

            StateHasChanged();
        }


        private async Task OnDayItemClickAsync(DateTime dateTime)
        {
            // Invoke JS first. if ComponentValue is set first we are at risk of this element being re-rendered before this line is run, making ListItemReference stale and causing a JS exception.
            await JsRuntime.InvokeVoidAsync("MaterialBlazor.MBDatePicker.listItemClick", ListItemReference, Utilities.DateToString(dateTime, DateFormat));
            ComponentValue = dateTime;
            MonthsOffset = 0;
            SetParameters();
        }


        private void OnYearItemClick(int year)
        {
            MonthsOffset += (year - StartOfDisplayMonth.Year) * 12;
            ShowYearPad = false;
            SetParameters();
        }


        private void OnPreviousMonthClick()
        {
            MonthsOffset--;
            SetParameters();
        }


        private void OnShowCurrentDateClick()
        {
            MonthsOffset = 0;
            SetParameters();
        }


        private void OnNextMonthClick()
        {
            MonthsOffset++;
            SetParameters();
        }


        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender) // TODO: this prevents us from marking OnAfterRenderAsync in InputComponent as sealed. Consider alternatives!
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender && ScrollToYear)
            {
                ScrollToYear = false;

                await JsRuntime.InvokeVoidAsync("MaterialBlazor.MBDatePicker.scrollToYear", currentYearId);
            }
        }
    }
}
