namespace GSvs.Core.Events
{
    public abstract class EventsPatcher<This> where This : EventsPatcher<This>
    {
        private static bool patched;

        protected static void Patch()
        {
            if (patched)
            {
                return;
            }
            GSvsPlugin.Harmony.PatchAll(typeof(This));
            patched = true;
        }
    }
}