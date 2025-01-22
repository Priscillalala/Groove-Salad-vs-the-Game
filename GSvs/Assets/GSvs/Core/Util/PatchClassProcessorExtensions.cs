using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GSvs.Core.Util
{
    public static class PatchClassProcessorExtensions
    {
        private static readonly FieldInfo _containerAttributes = typeof(PatchClassProcessor).GetField("containerAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _patchMethods = typeof(PatchClassProcessor).GetField("patchMethods", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _GetBulkMethods = typeof(PatchClassProcessor).GetMethod("GetBulkMethods", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _info;
        private static readonly MethodInfo _GetOriginalMethod;

        static PatchClassProcessorExtensions()
        {
            Assembly assembly = typeof(Harmony).Assembly;
            _info = assembly.GetType("HarmonyLib.AttributePatch").GetField("info", BindingFlags.Instance | BindingFlags.NonPublic);
            _GetOriginalMethod = assembly.GetType("HarmonyLib.PatchTools").GetMethod("GetOriginalMethod", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static void Unpatch(this PatchClassProcessor patchClassProcessor)
        {
            if (_containerAttributes.GetValue(patchClassProcessor) == null)
            {
                return;
            }
            IList patchMethods = (IList)_patchMethods.GetValue(patchClassProcessor);
            List<MethodBase> bulkMethods = (List<MethodBase>)_GetBulkMethods.Invoke(patchClassProcessor, null);
            if (bulkMethods.Count > 0)
            {
                foreach (var original in bulkMethods)
                {
                    PatchProcessor patchProcessor = new PatchProcessor(null, original);
                    foreach (var patchMethod in patchMethods)
                    {
                        HarmonyMethod info = (HarmonyMethod)_info.GetValue(patchMethod);
                        patchProcessor.Unpatch(info.method);
                    }
                }
            }
            else
            {
                foreach (var patchMethod in patchMethods)
                {
                    HarmonyMethod info = (HarmonyMethod)_info.GetValue(patchMethod);
                    MethodBase original = (MethodBase)_GetOriginalMethod.Invoke(null, new[] { info });
                    PatchProcessor patchProcessor = new PatchProcessor(null, original);
                    patchProcessor.Unpatch(info.method);
                }
            }
        }
    }
}