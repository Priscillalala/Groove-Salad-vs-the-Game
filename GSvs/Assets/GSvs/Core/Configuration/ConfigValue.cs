using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace GSvs.Core.Configuration
{
    public struct ConfigValue<T> : IConfigValue
    {
        private readonly T defaultValue;
        private ConfigEntry<T> source;

        public event Action ValueChanged;

        public readonly T Value => source != null ? source.Value : defaultValue;
        readonly object IConfigValue.BoxedValue => Value;
        readonly Type IConfigValue.ValueType => typeof(T);

        public ConfigValue(T defaultValue)
        {
            this.defaultValue = defaultValue;
            source = null;
            ValueChanged = null;
        }

        void IConfigValue.BindValue(ConfigEntryBase source)
        {
            if (this.source != null)
            {
                throw new InvalidOperationException();
            }
            if (source is not ConfigEntry<T>)
            {
                throw new ArgumentException(nameof(source));
            }
            this.source = (ConfigEntry<T>)source;
            if (!EqualityComparer<T>.Default.Equals(this.source.Value, defaultValue))
            {
                ValueChanged?.Invoke();
            }
            this.source.SettingChanged += OnSourceSettingChanged;
        }

        private readonly void OnSourceSettingChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke();
        }

        public static implicit operator ConfigValue<T>(T defaultValue) => new ConfigValue<T>(defaultValue);

        public static implicit operator T(ConfigValue<T> configValue) => configValue.Value;
    }
}