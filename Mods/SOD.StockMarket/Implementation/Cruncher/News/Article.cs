﻿using SOD.Common.Helpers;
using System.Text.Json.Serialization;

namespace SOD.StockMarket.Implementation.Cruncher.News
{
    internal class Article
    {
        [JsonIgnore]
        private Time.TimeData? _dateTime;
        [JsonIgnore]
        public Time.TimeData DateTime
        {
            get { return _dateTime ??= new Time.TimeData(Year, Month, Day, Hour, Minute); }
            set { _dateTime = value; }
        }

        // Serialization for timedata
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        // Actual properties
        public string Title { get; set; }
        /// <summary>
        /// Minutes before article is released
        /// </summary>
        public int MinutesLeft { get; set; }

        public Article() { }

        internal Article(Time.TimeData datetime, string title, int minutes)
        {
            DateTime = datetime;

            // Actual data
            Title = title;

            if (minutes > 0)
            {
                MinutesLeft = minutes;
                DateTime = DateTime.AddMinutes(minutes);
            }

            // Timedata serialization
            Year = datetime.Year;
            Month = datetime.Month;
            Day = datetime.Day;
            Hour = datetime.Hour;
            Minute = datetime.Minute;
        }
    }
}
