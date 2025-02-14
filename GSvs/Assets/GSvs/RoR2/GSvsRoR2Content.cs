using RoR2;
using RoR2.ContentManagement;
using System.Collections;

namespace GSvs.RoR2
{
    public class GSvsRoR2Content : IContentPackProvider
    {
        public static class Items
        {
            [TargetAssetName("GSvsBoostSpeed")]
            public static ItemDef BoostSpeed;
        }

        public static class Buffs
        {
            public static BuffDef DelicateWatchBonus;
        }

        public const string ADDRESSABLES_LABEL = "ContentPack:GSvs.RoR2";

        private readonly ContentPack contentPack = new ContentPack();

        public string identifier => "GSvs.RoR2";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator(ADDRESSABLES_LABEL);
            yield return loadHelper.AddContentPackLoadOperationWithYields(contentPack);
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs);
                ContentLoadHelper.PopulateTypeFields(typeof(Buffs), contentPack.buffDefs);
            }, 0.05f);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            yield break;
        }
    }
}