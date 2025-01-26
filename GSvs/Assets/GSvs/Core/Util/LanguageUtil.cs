using RoR2;
using System.Collections;

namespace GSvs.Core.Util
{
    public static class LanguageUtil
    {
        public static void ReloadLanguages()
        {
            foreach (Language language in Language.GetAllLanguages())
            {
                if (language.stringsLoaded)
                {
                    language.UnloadStrings();
                    language.LoadStrings();
                }
            }
        }

        public static IEnumerator RebuildLanguagesCoroutine()
        {
            yield return Language.BuildLanguagesFromFolders();
            foreach (Language language in Language.GetAllLanguages())
            {
                if (language.stringsLoaded)
                {
                    language.UnloadStrings();
                    yield return new Language.LanguageLoaderCoroutine(language).LoadStringsWithYield();
                }
            }
        }
    }
}