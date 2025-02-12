using GSvs.Core;
using GSvs.Core.Configuration;
using GSvs.Core.ContentManipulation;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace GSvs.RoR2.Items
{
    [HarmonyPatch]
    [LanguageOverrides]
    public abstract class Gasoline : ContentManipulator<Gasoline>
    {
        [InjectConfig(desc = "Remove the initial damage blast from Gasoline")]
        public static readonly bool Installed = true;

        [InitDuringStartup]
        static void Init()
        {
            if (Installed)
            {
                DefaultInit();
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(GlobalEventManager), nameof(GlobalEventManager.ProcIgniteOnKill))]
        static void RemoveBlast(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                _ => _.MatchCallOrCallvirt<BlastAttack>(nameof(BlastAttack.Fire)),
                _ => _.MatchPop()
                );
            ILLabel breakLabel = c.MarkLabel();
            c.GotoPrev(MoveType.Before,
                _ => _.MatchNewobj<BlastAttack>()
                );
            c.Emit(OpCodes.Br, breakLabel);
        }
    }
}
