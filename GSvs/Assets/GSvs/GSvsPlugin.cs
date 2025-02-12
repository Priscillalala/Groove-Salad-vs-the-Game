using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GSvs.Core.Configuration;
using GSvs.RoR2;
using HarmonyLib;
using R2API;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: SearchableAttribute.OptIn]

namespace GSvs
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    public class GSvsPlugin : BaseUnityPlugin
    {
        public const string
            GUID = "groovesalad." + NAME,
            NAME = "Groove_Salad_vs_the_Game",
            VERSION = "1.0.0";

        public static new PluginInfo Info { get; private set; }
        public static new ManualLogSource Logger { get; private set; }
        public static Dictionary<string, ConfigFile> ConfigFiles { get; private set; }
        public static Harmony Harmony { get; private set; }
        public static string RuntimeDirectory { get; private set; }
        public static string RuntimeAddressablesLocation { get; private set; }
        public static string RuntimeLanguageOverridesLocation { get; private set; }

        void Awake()
        {
            Info = base.Info;
            Logger = base.Logger;
            ConfigFiles = new Dictionary<string, ConfigFile>();
            Harmony = new Harmony(GUID);
            RuntimeDirectory = Path.GetDirectoryName(Info.Location);
            RuntimeAddressablesLocation = Path.Combine(RuntimeDirectory, "aa");
            RuntimeLanguageOverridesLocation = Path.Combine(RuntimeDirectory, "LanguageOverrides");
            Logger.LogMessage("We're so back");

            string catalogPath = Path.Combine(RuntimeAddressablesLocation, $"catalog_{NAME}.json");
            var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
            Logger.LogMessage("Loaded catalog:");

            InjectConfigAttribute.InjectAll();

#if false
            ElusiveAntlers.enabled = true;
            SaleStar.enabled = true;
            Logger.LogMessage($"ElusiveAntlers enabled: {ElusiveAntlers.enabled}");
            Logger.LogMessage($"SaleStar enabled: {SaleStar.enabled}");
            Type[] types = new[] { 
                Type.GetType("SaleStar"),
                Type.GetType("GSvs.RoR2.Items.ElusiveAntlers"), 
                typeof(ContentManipulatorInstance<SaleStar>), 
                typeof(StaticContentManipulator<ElusiveAntlers>) }; 
            foreach (var type in types)
            {
                Logger.LogMessage($"ContentManipulator: {type.FullName} derives from {type.BaseType?.FullName ?? "null"}");
                FieldInfo enabled = type.GetField("enabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (enabled != null)
                {
                    Logger.LogMessage($"    ENABLED: {enabled.GetValue(null)} (from type {enabled.DeclaringType.FullName})");
                }
            }
#endif
            ContentManager.collectContentPackProviders += add => add(new GSvsRoR2Content());
        }

        public static ConfigFile GetConfigFile(string configPath, bool saveOnInit = false)
        {
            configPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, configPath));
            Logger.LogMessage($"Get Config File: {configPath}");
            if (!ConfigFiles.TryGetValue(configPath, out ConfigFile configFile))
            {
                configFile = new ConfigFile(configPath, saveOnInit, Info.Metadata);
                ConfigFiles.Add(configPath, configFile);
            }
            return configFile;
        }
    }
}