using BepInEx.Configuration;
using System;
using System.Globalization;

namespace GSvs.Core.Configuration
{
    public readonly struct Percent : IEquatable<Percent>, IFormattable
    {
        private static readonly NumberFormatInfo invariantFormatProvider = new NumberFormatInfo
        {
            PercentPositivePattern = 1,
            PercentNegativePattern = 1,
        };

        static Percent()
        {
            TomlTypeConverter.AddConverter(typeof(Percent), new TypeConverter
            {
                ConvertToString = (obj, type) => obj.ToString(),
                ConvertToObject = (str, type) => Parse(str)
            });
        }

        public float Value { get; }

        public Percent(float value)
        {
            Value = value;
        }

        public bool Equals(Percent other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Percent other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return ToPercentString(formatProvider);
            }
            return Value.ToString(format, formatProvider);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToPercentString(formatProvider);
        }

        public override string ToString()
        {
            return ToPercentString(invariantFormatProvider);
        }

        private string ToPercentString(IFormatProvider formatProvider)
        {
            string percentString = Value.ToString("p99", formatProvider);
            int endIndex = percentString.LastIndexOf('0');
            int startIndex = endIndex;
            while (startIndex > 0 && percentString[startIndex - 1] == '0')
            {
                startIndex--;
            }
            while (startIndex > 0 && !char.IsDigit(percentString[startIndex - 1]))
            {
                startIndex--;
            }
            return percentString.Remove(startIndex, endIndex - startIndex + 1);
        }

        public static Percent Parse(string str)
        {
            string percentSymbol = invariantFormatProvider.PercentSymbol;
            int percentIndex = str.IndexOf(percentSymbol);
            if (percentIndex < 0)
            {
                return new Percent(float.Parse(str, invariantFormatProvider));
            }
            return new Percent(float.Parse(str.Remove(percentIndex, percentSymbol.Length), invariantFormatProvider) / 100f);
        }

        public static implicit operator Percent(float value)
        {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent)
        {
            return percent.Value;
        }
    }
}