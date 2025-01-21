using BepInEx;
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

        public static string RuntimeAddressablesLocation { get; private set; }

        void Awake()
        {
            string directory = Path.GetDirectoryName(Info.Location);
            RuntimeAddressablesLocation = Path.Combine(directory, "aa");
            Logger.LogMessage("We're so back");

            string catalogPath = Path.Combine(RuntimeAddressablesLocation, $"catalog_{NAME}.json");
            var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
            Logger.LogMessage("Loaded catalog:");
            foreach (var key in locator.Keys)
            {
                Logger.LogMessage(key);
            }
            Harmony harmony = new Harmony(GUID);

            foreach (var attribute in SearchableAttribute.GetInstances<ContentModificationAttribute>())
            {
                Type targetType = (Type)attribute.target;
                IContentModification contentModification = (IContentModification)Activator.CreateInstance(targetType);
                contentModification.Initialize();
                harmony.CreateClassProcessor(targetType).Patch();

                var methods = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    if (Attribute.IsDefined(method, typeof(AssetManipulatorAttribute)))
                    {
                        AssetManipulatorAttribute.Apply(method);
                    }
                }
            }

            ContentManager.collectContentPackProviders += add => add(new GSvsRoR2Content());
        }
    }
}