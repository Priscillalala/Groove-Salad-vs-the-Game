using RoR2;

namespace GSvs.Core.Util
{
    public static class LanguageUtil
    {
        public static void ReloadAllLanguages()
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
    }
}