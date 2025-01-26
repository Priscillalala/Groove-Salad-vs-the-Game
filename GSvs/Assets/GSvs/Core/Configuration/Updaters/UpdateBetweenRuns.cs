using BepInEx.Configuration;
using RoR2;
using System;

namespace GSvs.Core.Configuration.Updaters
{
    public class UpdateBetweenRuns : ConfigValueUpdater
    {
        public bool InRun => Run.instance != null;

        public override void Bind<T>(ConfigEntry<T> entry)
        {
            base.Bind(entry);
            entry.SettingChanged += OnSettingChanged;
            Run.onRunDestroyGlobal += OnRunDestroyGlobal;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (!InRun)
            {
                UpdateValue();
            }
        }

        private void OnRunDestroyGlobal(Run run)
        {
            UpdateValue();
        }
    }
}