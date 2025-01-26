using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration.Updaters
{
    public class UpdateAlways : ConfigValueUpdater
    {
        public override void Bind<T>(ConfigEntry<T> entry)
        {
            base.Bind(entry);
            entry.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            UpdateValue();
        }
    }
}