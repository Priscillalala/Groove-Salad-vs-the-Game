using BepInEx;
using BepInEx.Logging;
using GSvs.Core.AssetManipulation;
using GSvs.Core.ContentManipulation;
using GSvs.RoR2;
using HarmonyLib;
using HG.Reflection;
using RoR2.ContentManagement;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace GSvs
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class GSvsPlugin : BaseUnityPlugin
    {
        public const string
            GUID = "groovesalad." + NAME,
            NAME = "Groove_Salad_vs_the_Game",
            VERSION = "1.0.0";

        public static new ManualLogSource Logger { get; private set; }
        public static Harmony Harmony { get; private set; }
        public static string RuntimeDirectory { get; private set; }
        public static string RuntimeAddressablesLocation { get; private set; }

        void Awake()
        {
            Logger = base.Logger;
            Harmony = new Harmony(GUID);
            RuntimeDirectory = Path.GetDirectoryName(Info.Location);
            RuntimeAddressablesLocation = Path.Combine(RuntimeDirectory, "aa");
            Logger.LogMessage("We're so back");

            string catalogPath = Path.Combine(RuntimeAddressablesLocation, $"catalog_{NAME}.json");
            var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
            Logger.LogMessage("Loaded catalog:");
            foreach (var key in locator.Keys)
            {
                Logger.LogMessage(key);
            }

            foreach (var attribute in SearchableAttribute.GetInstances<ContentManipulatorAttribute>())
            {
                Type targetType = (Type)attribute.target;
                if (targetType.IsAbstract && !targetType.IsSealed)
                {
                    continue;
                }

                PatchClassProcessor patcher = Harmony.CreateClassProcessor(targetType);
                if (targetType.IsAbstract)
                {
                    patcher.Patch();
                }
                else
                {
                    ContentManipulatorInstance contentManipulatorInstance = (ContentManipulatorInstance)Activator.CreateInstance(targetType);
                    contentManipulatorInstance.patcher = patcher;
                    contentManipulatorInstance.SetEnabled(true);
                }

                var methods = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    if (Attribute.IsDefined(method, typeof(AssetManipulatorAttribute)))
                    {
                        AssetManipulatorAttribute.ExecuteAsync(method);
                    }
                }
            }

            ContentManager.collectContentPackProviders += add => add(new GSvsRoR2Content());
        }
    }
}