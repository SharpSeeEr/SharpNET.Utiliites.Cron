using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    /// <summary>
    /// Represents a single field inside of a full cron entry
    /// </summary>
    internal abstract class CronFieldBase : ICronField
    {
        protected readonly int _minValue;
        protected readonly int _maxValue;

        private readonly string _fieldLabel;
        private readonly List<int> _allowedValues = new List<int>();

        /// <summary>
        /// The raw string value of the field
        /// </summary>
        public string Value { get; protected set; }


        internal CronFieldBase(string fieldLabel, string value, int minValue, int maxValue)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(_fieldLabel + " Field Value cannot be null or empty");

            _fieldLabel = fieldLabel;
            Value = value;
            _minValue = minValue;
            _maxValue = maxValue;

            ParseCronFieldValue();
        }

        public bool IsValid(int value)
        {
            return _allowedValues.Contains(value);
        }

        /// <summary>
        /// Retrieves the next valid value that satisfies the field
        /// that is greater than or equal to the value passed in.
        /// </summary>
        /// <param name="start">Start Value</param>
        /// <returns>Next valid value, or -1 if none found</returns>
        public int GetNext(int start)
        {
            foreach (var item in _allowedValues)
            {
                if (item >= start) return item;
            }
            // No valid value greater than start found.
            return -1;
        }

        /// <summary>
        /// Retrieves the previous valid value that satisfies the field
        /// that is less than or equal to the value passed in.
        /// </summary>
        /// <param name="before">Before value</param>
        /// <returns>Previous valid value, or -1 if none found.</returns>
        public int GetPrev(int before)
        {
            for (int i = _allowedValues.Count - 1; i >= 0; i--)
            {
                if (_allowedValues[i] <= before) return _allowedValues[i];
            }
            return -1;
        }

        /// <summary>
        /// Gets the first valid value that satisfies the field
        /// </summary>
        public int GetFirst()
        {
            return _allowedValues[0];
        }

        /// <summary>
        /// Gets the last valid value that satisfies the field
        /// </summary>
        public int GetLast()
        {
            return _allowedValues[_allowedValues.Count - 1];
        }

        /// <summary>
        /// Parses the value for this field to determine all valid values
        /// </summary>
        /// <example>
        /// 5				Just # 5
        ///	1-10			1,2,3,4,5,6,7,8,9,10
        ///	2,3,9			2,3,9
        ///	2,3,5-7			2,3,5,6,7
        ///	1-10/3			1,4,7,10
        ///	2,3,4-10/2		2,3,4,6,8,10
        ///	*/3				minValue,minValue+3,...&lt;=maxValue
        /// </example>
        private void ParseCronFieldValue()
        {
            var parts = Value.Split(',');
            foreach (var part in parts)
            {
                ParseCronFieldValuePart(part);
            }
        }

        /// <summary>
        /// Parses a single part of a multipart value (parts separated by commas)
        /// </summary>
        /// <param name="part"></param>
        private void ParseCronFieldValuePart(string part)
        {
            if (string.IsNullOrEmpty(part))
                throw new CronFieldException(_fieldLabel + " Field cannot be empty");

            int interval = -1;
            // Check for an interval: 30/2
            if (part.Contains('/'))
            {
                string[] intervalParts = part.Split('/');
                if (string.IsNullOrEmpty(intervalParts[0]))
                    throw new CronFieldException(_fieldLabel + " Field cannot be empty: " + part);

                if (!int.TryParse(intervalParts[1], out interval))
                    throw new CronFieldException(_fieldLabel + " Field contains unexpected character: " + part);

                part = intervalParts[0];
            }

            // Check if this is wildcard
            if (part == "*")
            {
                AddRange(_minValue, _maxValue, interval);
            }
            else
            {
                // Not a wild card.
                // Check for a range
                if (part.Contains('-'))
                {
                    string[] rangeParts = part.Split('-');

                    if (!int.TryParse(rangeParts[0], out int startValue) ||
                        !int.TryParse(rangeParts[1], out int endValue))
                        throw new CronFieldException(_fieldLabel + " Field contains unexpected character: " + part);

                    if (startValue < _minValue)
                        throw new CronFieldException(_fieldLabel + " Field range starts below minimum: " + part);
                    if (endValue > _maxValue)
                        throw new CronFieldException(_fieldLabel + " Field range ends above maximum: " + part);
                    if (startValue > endValue)
                        throw new CronFieldException(_fieldLabel + " Field contains reversed range: " + part);

                    AddRange(startValue, endValue, interval);
                }
                else
                {
                    // Not a range.  MUst be a single number.
                    if (!int.TryParse(part, out int value))
                        throw new CronFieldException(_fieldLabel + " Field contains unexpected character: " + part);

                    if (value < _minValue)
                        throw new CronFieldException(_fieldLabel + " Field contains a value below minimum: " + part);
                    if (value > _maxValue)
                        throw new CronFieldException(_fieldLabel + " Field contains a value above maximum: " + part);

                    int stopValue = value; // +1;
                    if (interval != -1) stopValue = _maxValue;
                    AddRange(value, stopValue, interval);
                }
            }
        }

        /// <summary>
        /// Add a range of valid values to <see cref="_allowedValues"/>.
        /// </summary>
        /// <param name="start">First valid value, inclusive</param>
        /// <param name="stop">Last valid value, inclusive</param>
        /// <param name="interval">Interval between numbers</param>
        private void AddRange(int start, int stop, int interval)
        {
            if (interval == -1) interval = 1;
            for (int i = start; i <= stop; i += interval)
            {
                if (!_allowedValues.Contains(i)) _allowedValues.Add(i);
            }
        }
    }
}
