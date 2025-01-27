using HG;
using System;
using System.Text;

namespace GSvs.Core.Util
{
    public static class NicifyUtil
    {
        public static string NicifyIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return identifier;
            }
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.Append(char.ToUpperInvariant(identifier[0]));
            for (int i = 1; i < identifier.Length; i++)
            {
                char character = identifier[i];
                if (char.IsUpper(character))
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(character);
            }
            string niceIdentifier = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return niceIdentifier;
        }
    }
}