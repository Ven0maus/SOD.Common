using System;

namespace SOD.Common.Shadows.Implementations
{
    public sealed class Time
    {
        private bool _initialized = false;
        /// <summary>
        /// When this property is true, you can collect time information from the game.
        /// </summary>
        public bool IsInitialized { get { return _initialized; } }

        /// <summary>
        /// Raised when an in game minute passes.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnMinuteChanged;
        /// <summary>
        /// Raised when an in game hour passes.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnHourChanged;
        /// <summary>
        /// Raised when an in game day passes.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnDayChanged;
        /// <summary>
        /// Raised when an in game month passes.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnMonthChanged;
        /// <summary>
        /// Raised when an in game year passes.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnYearChanged;
        /// <summary>
        /// Raised when the time is initialized and can be used.
        /// </summary>
        public event EventHandler<TimeChangedArgs> OnTimeInitialized;

        private float? _currentGameTime;
        private TimeData? _currentTimeData, _currentDateData;

        /// <summary>
        /// Returns the current in game date and time.
        /// </summary>
        public TimeData CurrentDateTime
        {
            get
            {
                if (!IsInitialized)
                    throw new Exception("Time is not initialized at this state of the game, check for the property \"Lib.Time.IsInitialized\".");

                if (_currentGameTime == null || !_currentGameTime.Value.Equals(SessionData.Instance.gameTime))
                {
                    _currentGameTime = SessionData.Instance.gameTime;
                    GetGameTimeInfo(out _currentTimeData, out _currentDateData);
                }
                return _currentTimeData.Value;
            }
        }

        /// <summary>
        /// Returns the current in game date. (no time included)
        /// </summary>
        public TimeData CurrentDate
        {
            get
            {
                if (!IsInitialized)
                    throw new Exception("Time is not initialized at this state of the game, check for the property \"Lib.Time.IsInitialized\".");

                if (_currentGameTime == null || !_currentGameTime.Value.Equals(SessionData.Instance.gameTime))
                {
                    _currentGameTime = SessionData.Instance.gameTime;
                    GetGameTimeInfo(out _currentTimeData, out _currentDateData);
                }
                return _currentDateData.Value;
            }
        }

        /// <summary>
        /// Returns the current weekday enum
        /// </summary>
        public SessionData.WeekDay CurrentWeekDay
        {
            get
            {
                return SessionData.Instance.WeekdayFromInt(CurrentDateTime.Day);
            }
        }

        /// <summary>
        /// Returns the current month enum
        /// </summary>
        public SessionData.Month CurrentMonth
        {
            get
            {
                return SessionData.Instance.MonthFromInt(CurrentDateTime.Month);
            }
        }

        /// <summary>
        /// Updates the current game time information, if the game is in a playing state.
        /// </summary>
        private static void GetGameTimeInfo(out TimeData? dateTime, out TimeData? date)
        {
            var hour = SessionData.Instance.gameTime;
            var session = SessionData.Instance;
            var dateNr = 0;
            var leapCycleOut = GameplayControls.Instance.yearZeroLeapYearCycle;
            var day = GameplayControls.Instance.dayZero;
            var month = 0;
            var year = GameplayControls.Instance.publicYearZero + GameplayControls.Instance.startingYear;
            while (hour >= 24)
            {
                hour -= 24;
                day++;
                dateNr++;
                if (day >= 7)
                {
                    day -= 7;
                }
                int num = session.daysInMonths[month];
                if (leapCycleOut == 3 && month == 1)
                {
                    num++;
                }
                if (dateNr >= num)
                {
                    dateNr -= num;
                    month++;
                    if (month >= 12)
                    {
                        month -= 12;
                        year++;
                        for (leapCycleOut++; leapCycleOut >= 4; leapCycleOut -= 4) { }
                    }
                }
            }

            // Convert hour/minutes properly the way the game does it
            var formatted = session.FloatMinutes24H(hour);
            var hourInt = UnityEngine.Mathf.FloorToInt(formatted);
            var minute = UnityEngine.Mathf.RoundToInt((formatted - (float)hourInt) * 100f);

            // Because it starts at 0, and we don't have 0 based months/days
            month++;
            day++;

            // Set datetime structs
            date = new TimeData(year, month, day, 0, 0);
            dateTime = new TimeData(year, month, day, hourInt, minute);
        }

        internal void OnTimeChanged(TimeData previous, TimeData current)
        {
            if (previous.Minute != current.Minute)
                OnMinuteChanged?.Invoke(this, new TimeChangedArgs(previous, current));
            if (previous.Hour != current.Hour)
                OnHourChanged?.Invoke(this, new TimeChangedArgs(previous, current));
            if (previous.Day != current.Day)
                OnDayChanged?.Invoke(this, new TimeChangedArgs(previous, current));
            if (previous.Month != current.Month)
                OnMonthChanged?.Invoke(this, new TimeChangedArgs(previous, current));
            if (previous.Year != current.Year)
                OnYearChanged?.Invoke(this, new TimeChangedArgs(previous, current));
        }

        internal void InitializeTime()
        {
            if (_initialized) return;
            _initialized = true;
            OnTimeInitialized?.Invoke(this, new TimeChangedArgs(CurrentDateTime, CurrentDateTime));
        }

        public readonly struct TimeData : IEquatable<TimeData>, IComparable<TimeData>, IComparable
        {
            public int Year { get; }
            public int Month { get; }
            public int Day { get; }
            public int Hour { get; }
            public int Minute { get; }

            public SessionData.WeekDay CurrentWeekDay
            {
                get
                {
                    return SessionData.Instance.WeekdayFromInt(Day);
                }
            }

            public SessionData.Month CurrentMonth
            {
                get
                {
                    return SessionData.Instance.MonthFromInt(Month);
                }
            }

            public TimeData(int year, int month, int day, int hour, int minute)
            {
                Year = year;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
            }

            public override string ToString()
            {
                return "MM/dd/yyyy HH:mm"
                    .Replace("dd", Month.ToString("00"))
                    .Replace("MM", Day.ToString("00"))
                    .Replace("yyyy", Year.ToString())
                    .Replace("HH", Hour.ToString("00"))
                    .Replace("mm", Minute.ToString("00"));
            }

            public bool Equals(TimeData other)
            {
                return other.Year == Year && 
                    other.Month == Month && 
                    other.Day == Day && 
                    other.Hour == Hour && 
                    other.Minute == Minute;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Year, Month, Day, Hour, Minute);
            }

            public override bool Equals(object obj)
            {
                return obj is TimeData data && Equals(data);
            }

            public TimeData AddDays(int days)
            {
                DateTime currentDateTime = new(Year, Month, Day, Hour, Minute, 0);
                DateTime newDateTime = currentDateTime.AddDays(days);
                return new TimeData(newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute);
            }

            public int CompareTo(TimeData other)
            {
                // Compare by year
                int result = Year.CompareTo(other.Year);
                if (result != 0)
                    return result;

                // If years are equal, compare by month
                result = Month.CompareTo(other.Month);
                if (result != 0)
                    return result;

                // If months are equal, compare by day
                result = Day.CompareTo(other.Day);
                if (result != 0)
                    return result;

                // If days are equal, compare by hour
                result = Hour.CompareTo(other.Hour);
                if (result != 0)
                    return result;

                // If hours are equal, compare by minute
                return Minute.CompareTo(other.Minute);
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                {
                    // A null reference is considered greater than any non-null reference
                    return 1;
                }

                if (obj is TimeData otherTimeData)
                {
                    // Use the existing generic CompareTo method
                    return CompareTo(otherTimeData);
                }

                throw new ArgumentException("Object is not a TimeData");
            }

            public static bool operator ==(TimeData left, TimeData right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TimeData left, TimeData right)
            {
                return !(left == right);
            }

            public static TimeSpan operator -(TimeData left, TimeData right)
            {
                var dateLeft = new DateTime(left.Year, left.Month, left.Day, left.Hour, left.Minute, 0);
                var dateRight = new DateTime(right.Year, right.Month, right.Day, right.Hour, right.Minute, 0);
                var diff = dateLeft - dateRight;
                return diff;
            }
        }
    }

    /// <summary>
    /// EventArgs for Lib.Time
    /// </summary>
    public sealed class TimeChangedArgs : EventArgs
    {
        public Time.TimeData Previous { get; }
        public Time.TimeData Current { get; }
        public bool IsMinuteChanged { get; }
        public bool IsHourChanged { get; }
        public bool IsDayChanged { get; }
        public bool IsMonthChanged { get; }
        public bool IsYearChanged { get; }

        internal TimeChangedArgs(Time.TimeData previous, Time.TimeData current)
        {
            Previous = previous;
            Current = current;
            IsMinuteChanged = previous.Minute != current.Minute;
            IsHourChanged = previous.Hour != current.Hour;
            IsDayChanged = previous.Day != current.Day;
            IsMonthChanged = previous.Month != current.Month;
            IsYearChanged = previous.Year != current.Year;
        }
    }
}
