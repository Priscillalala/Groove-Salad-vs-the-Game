using BepInEx.Configuration;
using System;
using System.Globalization;

namespace GSvs.Core.Configuration
{
    public readonly struct Percent : IComparable<float>, IEquatable<float>, IFormattable
    {
        public static readonly NumberFormatInfo defaultFormatInfo = new NumberFormatInfo
        {
            PercentPositivePattern = 1,
            PercentNegativePattern = 1,
            PercentDecimalDigits = 0
        };

        static Percent()
        {
            TomlTypeConverter.AddConverter(typeof(Percent), new TypeConverter
            {
                ConvertToString = (obj, type) => obj.ToString(),
                ConvertToObject = (str, type) => Parse(str)
            });
        }

        const string PERCENT_FORMAT = "p";

        public float Value { get; }

        public Percent(float value)
        {
            Value = value;
        }

        public int CompareTo(float other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(float other)
        {
            return Value.Equals(other);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(PERCENT_FORMAT, formatProvider);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(PERCENT_FORMAT, formatProvider);
        }

        public override string ToString()
        {
            return Value.ToString(PERCENT_FORMAT, defaultFormatInfo);
        }

        public override bool Equals(object obj)
        {
            if (obj is Percent percent)
            {
                return Value == percent.Value;
            }
            if (obj is float other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static Percent Parse(string str)
        {
            string percentSymbol = defaultFormatInfo.PercentSymbol;
            int index = str.IndexOf(percentSymbol);
            if (index < 0)
            {
                return new Percent(float.Parse(str, defaultFormatInfo));
            }
            str = str.Remove(index, percentSymbol.Length);
            return new Percent(float.Parse(str, defaultFormatInfo) / 100f);
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