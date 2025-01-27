using BepInEx.Logging;
using GSvs.Core.Configuration;
using GSvs.Core.Util;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using SimpleJSON;
using System;
using System.Reflection;

namespace GSvs
{
    public static class GSvsLanguage
    {
        [InitDuringStartup]
        private static void Init()
        {
            GSvsPlugin.Harmony.PatchAll(typeof(GSvsLanguage));
        }

        public static bool TryConfigureLanguage(JSONNode jsonNode)
        {
            JSONNode configNode = jsonNode["config"];
            JSONNode stringsNode = jsonNode["strings"];
            if (stringsNode == null || configNode == null || !configNode.IsArray)
            {
                return false;
            }
            JSONArray configArray = configNode.AsArray;
            object[] args = new object[configArray.Count];
            for (int i = 0; i < configArray.Count; i++)
            {
                JSONNode configArrayNode = configArray[i];
                if (!configArrayNode.IsString || !SerializedMemberUtil.TryParseMember(configArrayNode.Value, out Type type, out string memberName))
                {
                    GSvsPlugin.Logger.LogWarning($"{configArrayNode.Value} failed to parse member");
                    continue;
                }
                const BindingFlags MEMBER_BINDING_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
                FieldInfo field = type.GetField(memberName, MEMBER_BINDING_FLAGS);
                if (field != null)
                {
                    args[i] = field.GetValue(null);
                }
            }
            foreach (JSONNode stringNode in stringsNode.Children)
            {
                stringNode.Value = string.Format(stringNode.Value, args);
            }
            return true;
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Language), nameof(Language.LoadTokensFromData))]
        private static void LanguageDataExtensions(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locJsonIndex = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt(typeof(JSON), nameof(JSON.Parse)),
                x => x.MatchStloc(out locJsonIndex)
                );
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(locJsonIndex),
                x => x.MatchLdnull(),
                x => x.MatchCallOrCallvirt<JSONNode>("op_Inequality"),
                x => x.MatchBrfalse(out _)
                );
            c.Emit(OpCodes.Ldloc, locJsonIndex);
            c.EmitDelegate<Action<JSONNode>>(jsonNode =>
            {
                JSONNode modNode = jsonNode["mod"];
                if (modNode != null && modNode.IsString && modNode.Value == GSvsPlugin.GUID)
                {
                    TryConfigureLanguage(jsonNode);
                }
            });
        }
    }
}