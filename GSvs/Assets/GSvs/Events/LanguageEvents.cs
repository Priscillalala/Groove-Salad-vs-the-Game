using GSvs.Core.Events;
using HarmonyLib;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace GSvs.Events
{
    [HarmonyPatch(typeof(Language))]
    public abstract class LanguageEvents : EventsPatcher<LanguageEvents>
    {
        private static Action<List<string>> collectLanguageOverrideRootFolders;

        public static event Action<List<string>> CollectLanguageOverrideRootFolders
        {
            add
            {
                Patch();
                collectLanguageOverrideRootFolders += value;
            }
            remove => collectLanguageOverrideRootFolders -= value;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(Language.GetLanguageRootFolders))]
        private static void GetLanguageRootFolders(List<string> __result)
        {
            collectLanguageOverrideRootFolders?.Invoke(__result);
        }
    }
}