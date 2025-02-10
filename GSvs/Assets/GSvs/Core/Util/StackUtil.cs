using System.Runtime.CompilerServices;

namespace GSvs.Core.Util
{
    public static class StackUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Scale(float baseValue, float stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Scale(int baseValue, int stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0;
        }
    }
}