using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2.ContentManagement;
using System.Collections;
using RoR2;

namespace GSvs.RoR2
{
    public class GSvsRoR2Content : IContentPackProvider
    {
        private readonly ContentPack contentPack = new ContentPack();

        public string identifier => "GSvs.RoR2";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;

            yield break;
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