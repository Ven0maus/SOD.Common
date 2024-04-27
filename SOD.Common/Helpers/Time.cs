using System;
using System.Linq;

namespace SOD.Common.Helpers
{
    public sealed class Time
    {
        internal Time() { }

        private bool _initialized = false;
        /// <summary>
        /// When this property is true, you can collect time information from the game.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Raised when the game is paused.
        /// </summary>
        public event EventHandler OnGamePaused;
        /// <summary>
        /// Raised when the game is resumed from a paused state.
        /// </summary>
        public event EventHandler OnGameResumed;
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

        public bool IsGamePaused { get; internal set; }

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
        public SessionData.WeekDay CurrentDayEnum => SessionData.Instance.WeekdayFromInt(CurrentDateTime.Day);

        /// <summary>
        /// Returns the current month enum
        /// </summary>
        public SessionData.Month CurrentMonthEnum => SessionData.Instance.MonthFromInt(CurrentDateTime.Month);

        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void ResumeGame() => SessionData.Instance.ResumeGame();

        /// <summary>
        /// Pauses the game.
        /// </summary>
        /// <param name="showPauseText">Whether to show "Paused" text.</param>
        /// <param name="delayOverride"></param>
        /// <param name="openDesktopMode">Whether to open the pause menu including the map and notes screen.</param>
        public void PauseGame(bool showPauseText = true, bool openDesktopMode = true, bool delayOverride = false)
            => SessionData.Instance.PauseGame(showPauseText, delayOverride, openDesktopMode);

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
            var minute = UnityEngine.Mathf.RoundToInt((formatted - hourInt) * 100f);

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

        internal void OnPauseModeChanged(bool paused)
        {
            if (paused)
                OnGamePaused?.Invoke(this, EventArgs.Empty);
            else
                OnGameResumed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes init time, if reset is true it will simply reset to false.
        /// <br>The next run will then pick it up automatically.</br>
        /// </summary>
        /// <param name="reset"></param>
        internal void InitializeTime(bool reset = false)
        {
            if (reset)
            {
                _initialized = false;
                _currentGameTime = null;
                _currentDateData = null;
                _currentTimeData = null;
                return;
            }

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

            /// <summary>
            /// Return's the game's WeekDay enum for the week day
            /// </summary>
            public SessionData.WeekDay DayEnum => SessionData.Instance.WeekdayFromInt(Day - 1);

            /// <summary>
            /// Return's the game's Month enum for the current month
            /// </summary>
            public SessionData.Month MonthEnum => SessionData.Instance.MonthFromInt(Month - 1);

            public TimeData(int year, int month, int day, int hour, int minute)
            {
                Year = year;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
            }

            /// <summary>
            /// Serializes the data in a string format "{Year}|{Month}|{Day}|{Hour}|{Minute}".
            /// </summary>
            /// <returns></returns>
            public string Serialize()
            {
                return $"{Year}|{Month}|{Day}|{Hour}|{Minute}";
            }

            /// <summary>
            /// Deserializes to a TimeData struct from following string format "{Year}|{Month}|{Day}|{Hour}|{Minute}".
            /// </summary>
            /// <param name="serializedString"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public static TimeData Deserialize(string serializedString)
            {
                var data = serializedString.Split('|').Select(int.Parse).ToArray();
                if (data.Length != 5) throw new Exception("Invalid timedata serialize string, use method .Serialize() on TimeData instance.");
                return new TimeData(data[0], data[1], data[2], data[3], data[4]);
            }

            /// <summary>
            /// Returns a string format "MM/dd/yyyy HH:mm".
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return ToString("MM/dd/yyyy HH:mm");
            }

            /// <summary>
            /// Returns a custom string format, you can use any of the following elements which will be correctly replaced:
            /// <br>dd -> day</br>
            /// <br>MM -> month</br>
            /// <br>yyyy -> year</br>
            /// <br>HH -> hour</br>
            /// <br>mm -> minute</br>
            /// </summary>
            /// <param name="format"></param>
            /// <returns></returns>
            public string ToString(string format)
            {
                return format
                    .Replace("dd", Day.ToString("00"))
                    .Replace("MM", Month.ToString("00"))
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

            /// <summary>
            /// Add's a certain amount of days to the TimeData
            /// </summary>
            /// <param name="days"></param>
            /// <returns></returns>
            public TimeData AddDays(int days)
            {
                return AddHours(days * 24);
            }

            /// <summary>
            /// Add's a certain amount of hours to the TimeData
            /// </summary>
            /// <param name="minutes"></param>
            /// <returns></returns>
            public TimeData AddHours(int hours)
            {
                return AddMinutes(hours * 60);
            }

            /// <summary>
            /// Add's a certain amount of minutes to the TimeData
            /// </summary>
            /// <param name="minutes"></param>
            /// <returns></returns>
            public TimeData AddMinutes(int minutes)
            {
                DateTime currentDateTime = new(Year, Month == 0 ? Month + 1 : Month, Day == 0 ? Day + 1 : Day, Hour, Minute, 0);
                DateTime newDateTime = currentDateTime.AddMinutes(minutes);
                return new TimeData(newDateTime.Year, Month == 0 ? newDateTime.Month - 1 : newDateTime.Month,
                    Day == 0 ? newDateTime.Day - 1 : newDateTime.Day, newDateTime.Hour, newDateTime.Minute);
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

            public static bool operator <(TimeData t1, TimeData t2)
            {
                if (t1.Year < t2.Year)
                    return true;
                else if (t1.Year == t2.Year)
                {
                    if (t1.Month < t2.Month)
                        return true;
                    else if (t1.Month == t2.Month)
                    {
                        if (t1.Day < t2.Day)
                            return true;
                        else if (t1.Day == t2.Day)
                        {
                            if (t1.Hour < t2.Hour)
                                return true;
                            else if (t1.Hour == t2.Hour)
                            {
                                if (t1.Minute < t2.Minute)
                                    return true;
                            }
                        }
                    }
                }
                return false;
            }

            public static bool operator >(TimeData t1, TimeData t2)
            {
                return t2 < t1;
            }

            public static bool operator <=(TimeData t1, TimeData t2)
            {
                return t1 < t2 || t1 == t2;
            }

            public static bool operator >=(TimeData t1, TimeData t2)
            {
                return t1 > t2 || t1 == t2;
            }

            public static TimeSpan operator -(TimeData left, TimeData right)
            {
                try
                {
                    var dateLeft = new DateTime(left.Year, left.Month == 0 ? left.Month + 1 : left.Month,
                        left.Day == 0 ? left.Day + 1 : left.Day, left.Hour, left.Minute, 0);
                    var dateRight = new DateTime(right.Year, right.Month == 0 ? right.Month + 1 : right.Month,
                        right.Day == 0 ? right.Day + 1 : right.Day, right.Hour, right.Minute, 0);
                    return dateLeft - dateRight;
                }
                catch (Exception)
                {
                    Plugin.Log.LogError($"[Date information]: Left ({left}) | Right ({right})");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// EventArgs for time events
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