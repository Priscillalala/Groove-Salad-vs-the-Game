using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.Util;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Reflection;
using Path = System.IO.Path;

namespace GSvs
{
    public static class GSvsLanguage
    {
        [InitDuringStartup]
        private static void Init()
        {
            GSvsPlugin.Harmony.PatchAll(typeof(GSvsLanguage));
        }

        public static bool CheckLanguageCondition(JSONNode jsonNode)
        {
            JSONNode conditionNode = jsonNode["condition"];
            if (conditionNode != null && conditionNode.IsString && TryParseMember(conditionNode.Value, out bool value, out var valueWrapper))
            {
                if (valueWrapper != null)
                {
                    valueWrapper.ValueChanged -= LanguageUtil.ReloadAllLanguages;
                    valueWrapper.ValueChanged += LanguageUtil.ReloadAllLanguages;
                }
                return value;                
            }
            return true;
        }

        public static bool TryFormatLanguage(JSONNode jsonNode)
        {
            JSONNode argsNode = jsonNode["args"];
            JSONNode stringsNode = jsonNode["strings"];
            if (stringsNode != null && argsNode != null && argsNode.IsArray)
            {
                JSONArray argsArray = argsNode.AsArray;
                object[] args = new object[argsArray.Count];
                for (int i = 0; i < argsArray.Count; i++)
                {
                    JSONNode argsArrayNode = argsArray[i];
                    object arg;
                    switch (argsArrayNode.Tag)
                    {
                        case JSONNodeType.String:
                            TryParseMember(argsArrayNode.string)
                    }
                }
            }
        }

        public static bool TryParseMember<T>(string qualifiedMemberName, out T value, out IValueWrapper<T> valueWrapper)
        {
            value = default;
            valueWrapper = null;
            if (string.IsNullOrEmpty(qualifiedMemberName))
            {
                return false;
            }
            int typeSeperator = qualifiedMemberName.LastIndexOf('.');
            if (typeSeperator < 0)
            {
                return false;
            }
            string typeName = qualifiedMemberName[..typeSeperator];
            Type type = Type.GetType(typeName, false);
            if (type == null)
            {
                return false;
            }
            string memberName = qualifiedMemberName[(typeSeperator + 1)..];
            const BindingFlags MEMBER_BINDING_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            FieldInfo field = type.GetField(memberName, MEMBER_BINDING_FLAGS);
            if (field != null)
            {
                if (typeof(T).IsAssignableFrom(field.FieldType))
                {
                    value = (T)field.GetValue(null);
                    return true;
                }
                else if (typeof(IValueWrapper<T>).IsAssignableFrom(field.FieldType))
                {
                    valueWrapper = (IValueWrapper<T>)field.GetValue(null);
                    if (valueWrapper != null)
                    {
                        value = valueWrapper.Value;
                        return true;
                    }
                }
            }
#if false
            PropertyInfo property = type.GetProperty(memberString, MEMBER_BINDING_FLAGS);
            if (property != null && property.CanRead)
            {
                if (typeof(T).IsAssignableFrom(property.PropertyType))
                {
                    value = (T)property.GetValue(null);
                    return true;
                }
                else if (typeof(IValueWrapper<T>).IsAssignableFrom(property.PropertyType))
                {
                    valueWrapper = (IValueWrapper<T>)property.GetValue(null);
                    if (valueWrapper != null)
                    {
                        value = valueWrapper.Value;
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Language), nameof(Language.GetLanguageRootFolders))]
        private static void RegisterLanguage(ref List<string> __result)
        {
            __result.Add(Path.Combine(GSvsPlugin.RuntimeDirectory, "Language"));
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Language), nameof(Language.LoadTokensFromData))]
        private static void LanguageDataExtensions(ILContext il)
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
                if (modNode == null || !modNode.IsString || modNode.Value != GSvsPlugin.GUID)
                {
                    return true;
                }
                if (!CheckLanguageCondition(modNode))
                {
                    return false;
                }

                return true;
            });
        }
    }
}