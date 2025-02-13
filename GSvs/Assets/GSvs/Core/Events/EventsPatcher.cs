namespace GSvs.Core.Events
{
    public abstract class EventsPatcher<This> where This : EventsPatcher<This>
    {
        public static bool Patched { get; private set; }

        protected static void PatchAll()
        {
            if (Patched)
            {
                return;
            }
            GSvsPlugin.Harmony.PatchAll(typeof(This));
            Patched = true;
        }
    }
}