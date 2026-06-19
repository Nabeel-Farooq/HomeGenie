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
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

using LiteDB;
using Newtonsoft.Json;

namespace HomeGenie.Data
{
    /// <summary>
    /// Module parameter.
    /// </summary>
    [Serializable]
    public class ModuleParameter
    {
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            };

        [NonSerialized]
        private ValueStatistics statistics;

        [NonSerialized]
        private DateTime requestUpdateTimestamp = DateTime.UtcNow;

        private object data;

        public ModuleParameter()
        {
            Name = string.Empty;
            Value = string.Empty;
            Description = string.Empty;
            FieldType = string.Empty;
            UpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        [XmlIgnore, JsonIgnore, BsonIgnore]
        public ValueStatistics Statistics
        {
            get
            {
                return statistics ?? (statistics = new ValueStatistics());
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the data object.
        /// </summary>
        public object GetData()
        {
            return data;
        }

        /// <summary>
        /// If data is stored as a JSON serialized string, use this method to get the object instance specifying its type T.
        /// </summary>
        public T GetData<T>()
        {
            if (data is string)
            {
                try
                {
                    data = JsonConvert.DeserializeObject<T>(
                        Convert.ToString(data, CultureInfo.InvariantCulture),
                        JsonSettings
                    );
                }
                catch
                {
                    // ignored
                }
            }

            return (T)data;
        }

        /// <summary>
        /// Sets the data of this parameter.
        /// </summary>
        public void SetData(object dataObject)
        {
            UpdateTime = DateTime.UtcNow;
            data = dataObject;

            if (data == null)
                return;

            var stringValue = Value;

            double numericValue;
            if (!string.IsNullOrEmpty(stringValue) &&
                double.TryParse(
                    stringValue.Replace(",", "."),
                    NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out numericValue))
            {
                Statistics.AddValue(Name, numericValue, UpdateTime);
            }
        }

        /// <summary>
        /// Gets or sets the data as string.
        /// </summary>
        public string Value
        {
            get
            {
                bool isNumber =
                    data is sbyte ||
                    data is byte ||
                    data is short ||
                    data is ushort ||
                    data is int ||
                    data is uint ||
                    data is long ||
                    data is ulong ||
                    data is float ||
                    data is double ||
                    data is decimal;

                if (isNumber || data is string)
                {
                    return Convert.ToString(data, CultureInfo.InvariantCulture);
                }

                return JsonConvert.SerializeObject(data, JsonSettings);
            }
            set
            {
                SetData(value);
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [BsonIgnore]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        [BsonIgnore]
        public string FieldType { get; set; }

        [JsonIgnore, BsonIgnore]
        public int ParentId { get; set; }

        /// <summary>
        /// Gets the update time.
        /// </summary>
        public DateTime UpdateTime { get; set; }

        // TODO: deprecate this field
        [XmlIgnore, BsonIgnore]
        public bool NeedsUpdate { get; set; }

        /// <summary>
        /// Gets the decimal value.
        /// </summary>
        [XmlIgnore, JsonIgnore, BsonIgnore]
        public double DecimalValue
        {
            get
            {
                var stringValue = Value;

                double value;
                return !string.IsNullOrEmpty(stringValue) &&
                       double.TryParse(
                           stringValue.Replace(",", "."),
                           NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                           CultureInfo.InvariantCulture,
                           out value)
                    ? value
                    : 0d;
            }
        }

        /// <summary>
        /// Determines whether this instance has the given name.
        /// </summary>
        public bool Is(string name)
        {
            return string.Equals(
                Name,
                name,
                StringComparison.OrdinalIgnoreCase
            );
        }

        public void RequestUpdate()
        {
            requestUpdateTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Waits until this parameter is updated.
        /// </summary>
        public bool WaitUpdate(double timeoutSeconds)
        {
            var lastUpdateTicks = UpdateTime.Ticks;

            while (lastUpdateTicks == UpdateTime.Ticks &&
                   (DateTime.UtcNow - requestUpdateTimestamp).TotalSeconds < timeoutSeconds)
            {
                Thread.Sleep(250);
            }

            return lastUpdateTicks != UpdateTime.Ticks;
        }

        /// <summary>
        /// Gets the idle time in seconds.
        /// </summary>
        [XmlIgnore, JsonIgnore, BsonIgnore]
        public double IdleTime
        {
            get
            {
                return (DateTime.UtcNow - UpdateTime).TotalSeconds;
            }
        }
    }
}
