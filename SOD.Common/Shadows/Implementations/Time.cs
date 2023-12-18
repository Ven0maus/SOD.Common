using System;

namespace SOD.Common.Shadows.Implementations
{
    public sealed class Time
    {
        /// <summary>
        /// When this property is true, you can collect time information from the game.
        /// </summary>
        public bool IsInitialized { get { return SessionData.Instance.play && (SessionData.Instance.gameTime > 0 || SessionData.Instance.gameTime < 0); } }

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
        /// Returns the current in game date and time.
        /// </summary>
        public TimeData CurrentDateTime
        {
            get
            {
                return GetTimeInfo() ?? 
                    throw new Exception("Time is not initialized at this state of the game, check for the property \"Lib.Time.IsInitialized\".");
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
        private TimeData? GetTimeInfo()
        {
            if (!IsInitialized) return null;

            var hour = SessionData.Instance.gameTime;
            var session = SessionData.Instance;
            var date = 0;
            var leapCycleOut = GameplayControls.Instance.yearZeroLeapYearCycle;
            var day = GameplayControls.Instance.dayZero;
            var month = 0;
            var year = GameplayControls.Instance.publicYearZero + GameplayControls.Instance.startingYear;
            while (hour >= 24)
            {
                hour -= 24;
                day++;
                date++;
                if (day >= 7)
                {
                    day -= 7;
                }
                int num = session.daysInMonths[month];
                if (leapCycleOut == 3 && month == 1)
                {
                    num++;
                }
                if (date >= num)
                {
                    date -= num;
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

            // Set information
            return new TimeData(year, month, day, hourInt, minute);
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

        public readonly struct TimeData : IEquatable<TimeData>
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

            public static bool operator ==(TimeData left, TimeData right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TimeData left, TimeData right)
            {
                return !(left == right);
            }
        }

        public sealed class TimeChangedArgs : EventArgs
        {
            public TimeData Previous { get; }
            public TimeData Current { get; }

            internal TimeChangedArgs(TimeData previous, TimeData current)
            {
                Previous = previous;
                Current = current;
            }
        }
    }
}
