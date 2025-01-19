using UnityEngine;
using BepInEx;
using System.Security.Permissions;
using System.Security;
using Path = System.IO.Path;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2.ContentManagement;
using GSvs.RoR2;
using HG.Reflection;
using System;

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

            foreach (var attribute in SearchableAttribute.GetInstances<ContentModificationAttribute>())
            {
                ((IContentModification)Activator.CreateInstance((Type)attribute.target)).Initialize();
            }

            ContentManager.collectContentPackProviders += add => add(new GSvsRoR2Content());
        }
    }
}