using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using GSvs.RoR2;
using GSvs.RoR2.Items;
using HarmonyLib;
using HG.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
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

        void Awake()
        {
            Info = base.Info;
            Logger = base.Logger;
            ConfigFiles = new Dictionary<string, ConfigFile>();
            Harmony = new Harmony(GUID);
            Harmony.PatchAll(typeof(GSvsPlugin));
            RuntimeDirectory = Path.GetDirectoryName(Info.Location);
            RuntimeAddressablesLocation = Path.Combine(RuntimeDirectory, "aa");
            Logger.LogMessage("We're so back");

            string catalogPath = Path.Combine(RuntimeAddressablesLocation, $"catalog_{NAME}.json");
            var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
            Logger.LogMessage("Loaded catalog:");
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
                    /*ContentManipulatorInstance contentManipulatorInstance = (ContentManipulatorInstance)Activator.CreateInstance(targetType);
                    contentManipulatorInstance.patcher = patcher;
                    contentManipulatorInstance.SetEnabled(true);*/
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

        public static ConfigFile GetConfigFile(string configPath, bool saveOnInit = false)
        {
            configPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, configPath));
            if (!ConfigFiles.TryGetValue(configPath, out ConfigFile configFile))
            {
                configFile = new ConfigFile(configPath, saveOnInit, Info.Metadata);
                ConfigFiles.Add(configPath, configFile);
            }
            return configFile;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Language), nameof(Language.GetLanguageRootFolders))]
        private static void RegisterLanguage(ref List<string> __result)
        {
            __result.Add(Path.Combine(RuntimeDirectory, "Language"));
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Language), nameof(Language.LoadTokensFromData))]
        private static void HandleConditionalLanguage(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locJsonIndex = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt(nameof(JSON), nameof(JSON.Parse)),
                x => x.MatchStloc(out locJsonIndex)
                );
            ILLabel breakLabel = null;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(locJsonIndex),
                x => x.MatchLdnull(),
                x => x.MatchCallOrCallvirt<JSONNode>("op_Inequality"),
                x => x.MatchBrfalse(out breakLabel)
                );

            c.Emit(OpCodes.Ldloc, locJsonIndex);
            c.EmitDelegate<Func<JSONNode, bool>>(jsonNode =>
            {
                JSONNode modNode = jsonNode["mod"];
                if (modNode == null || !modNode.IsString || modNode.Value != GUID)
                {
                    return true;
                }
                JSONNode conditionNode = jsonNode["condition"];
                if (conditionNode == null || !conditionNode.IsString)
                {
                    return true;
                }
                string condition = conditionNode.Value;
                if (string.IsNullOrEmpty(condition))
                {
                    return true;
                }
                int typeSeperator = condition.LastIndexOf('.');
                if (typeSeperator < 0)
                {
                    return true;
                }
                string typeString = condition[..typeSeperator];
                Type type = Type.GetType(typeString, false);
                if (type == null)
                {
                    return true;
                }
                string memberString = condition[(typeSeperator + 1)..];
                const BindingFlags MEMBER_BINDING_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
                FieldInfo field = type.GetField(memberString, MEMBER_BINDING_FLAGS);
                if (field != null)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        return (bool)field.GetValue(null);
                    }
                    else if (typeof(IConfigValue).IsAssignableFrom(field.FieldType))
                    {
                        IConfigValue configValue = (IConfigValue)field.GetValue(null);
                        if (configValue.ValueType == typeof(bool))
                        {
                            return (bool)configValue.BoxedValue;
                        }
                    }
                }
                PropertyInfo property = type.GetProperty(memberString, MEMBER_BINDING_FLAGS);
                if (property != null && property.CanRead)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        return (bool)property.GetValue(null);
                    }
                    else if (typeof(IConfigValue).IsAssignableFrom(property.PropertyType))
                    {
                        IConfigValue configValue = (IConfigValue)property.GetValue(null);
                        if (configValue.ValueType == typeof(bool))
                        {
                            return (bool)configValue.BoxedValue;
                        }
                    }
                }
                return true;
            });
        }
    }
}