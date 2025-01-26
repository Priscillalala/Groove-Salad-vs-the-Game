using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public abstract class ConfigValueUpdater
    {
        public event Action OnValueUpdated;

        protected ConfigEntryBase trackedEntry;
        private object cachedValue;

        public virtual void Bind<T>(ConfigEntry<T> entry)
        {
            trackedEntry = entry;
            cachedValue = entry.Value;
            //RoO stuff
        }

        protected void UpdateValue()
        {
            if (!Equals(cachedValue, trackedEntry.BoxedValue))
            {
                cachedValue = trackedEntry.BoxedValue;
                OnValueUpdated?.Invoke();
            }
        }
    }
}