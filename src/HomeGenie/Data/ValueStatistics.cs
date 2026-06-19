/*
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as
   published by the Free Software Foundation, either version 3 of the
   License, or (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.

   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using HomeGenie.Service;

namespace HomeGenie.Data
{
    /// <summary>
    /// Value statistics.
    /// </summary>
    public class ValueStatistics
    {
        /// <summary>
        /// Stat value.
        /// </summary>
        public class StatValue
        {
            private static readonly DateTime UnixEpoch =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            /// <summary>
            /// Gets the value.
            /// </summary>
            public readonly double Value;

            /// <summary>
            /// Gets the timestamp.
            /// </summary>
            public readonly DateTime Timestamp;

            /// <summary>
            /// Gets the unix timestamp.
            /// </summary>
            public double UnixTimestamp
            {
                get { return (Timestamp - UnixEpoch).TotalMilliseconds; }
            }

            public StatValue(double value, DateTime timestamp)
            {
                Value = value;
                Timestamp = timestamp;
            }
        }

        private TsList<StatValue> historyValues;

        // historyLimit is expressed in minutes
        private int historyLimit = 60 * 24;
        private int historyLimitSize = 86400;

        private StatValue lastEvent;
        private StatValue lastOn;
        private StatValue lastOff;

        public ValueStatistics()
        {
            var initValue = new StatValue(0, DateTime.UtcNow);

            lastEvent = initValue;
            lastOn = initValue;
            lastOff = initValue;

            historyValues = new TsList<StatValue>();
        }

        /// <summary>
        /// Gets or sets the history limit.
        /// </summary>
        public int HistoryLimit
        {
            get { return historyLimit; }
            set { historyLimit = value; }
        }

        /// <summary>
        /// Gets or sets the history limit size.
        /// </summary>
        public int HistoryLimitSize
        {
            get { return historyLimitSize; }
            set { historyLimitSize = value; }
        }

        /// <summary>
        /// Gets the history.
        /// </summary>
        public TsList<StatValue> History
        {
            get { return historyValues; }
            set { historyValues = value; }
        }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public StatValue Current
        {
            get
            {
                return historyValues.Count > 0
                    ? historyValues[0]
                    : new StatValue(0, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Gets the last value.
        /// </summary>
        public StatValue Last
        {
            get { return lastEvent; }
        }

        /// <summary>
        /// Gets the last on value (value != 0).
        /// </summary>
        public StatValue LastOn
        {
            get { return lastOn; }
        }

        /// <summary>
        /// Gets the last off value (value == 0).
        /// </summary>
        public StatValue LastOff
        {
            get { return lastOff; }
        }

        internal void AddValue(string fieldName, double value, DateTime timestamp)
        {
            // "value" is the occurring event in this very moment,
            // so "Current" is holding previous value right now
            var current = Current;

            if (current != null && current.Value != value)
            {
                lastEvent = new StatValue(current.Value, current.Timestamp);

                if (value == 0 && lastEvent.Value > 0)
                {
                    lastOn = lastEvent;
                    lastOff = new StatValue(value, timestamp);
                }
                else if (value > 0 && lastEvent.Value == 0)
                {
                    lastOff = lastEvent;
                    lastOn = new StatValue(value, timestamp);
                }
            }

            // keep size within historyLimit (minutes)
            try
            {
                var count = historyValues.Count;

                if (count > historyLimitSize)
                {
                    historyValues.RemoveRange(
                        historyLimitSize,
                        count - historyLimitSize
                    );
                }

                if (historyValues.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    var oldest = historyValues[historyValues.Count - 1];

                    if ((now - oldest.Timestamp).TotalMinutes > historyLimit)
                    {
                        historyValues.RemoveAll(
                            sv => (now - sv.Timestamp).TotalMinutes > historyLimit
                        );
                    }
                }

                // leave this wrapped in a try..catch
            }
            catch
            {
            }

            // insert current value into history and so update "Current" to "value"
            historyValues.Insert(0, new StatValue(value, timestamp));
        }

        /// <summary>
        /// Get resampled statistic values by averaging values for a given time range increment (eg 60 minutes)
        /// </summary>
        internal List<StatValue> GetResampledValues(int sampleWidth) // in minutes
        {
            // historyValues.FindAll(sv => (DateTime.UtcNow - sv.Timestamp).TotalMinutes < sampleWidth);
            // TODO: to be implemented
            return null;
        }
    }
}
