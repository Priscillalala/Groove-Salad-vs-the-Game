using System;

namespace GSvs.Core.Util
{
    public static class SerializedMemberUtil
    {
        public static bool TryParseMember(string serializedMember, out Type type, out string memberName)
        {
            type = null;
            memberName = null;
            if (string.IsNullOrEmpty(serializedMember))
            {
                return false;
            }
            int typeSeperator = serializedMember.LastIndexOf('.');
            if (typeSeperator < 0)
            {
                return false;
            }
            string typeName = serializedMember[..typeSeperator];
            type = Type.GetType(typeName, false);
            if (type == null)
            {
                return false;
            }
            memberName = serializedMember[(typeSeperator + 1)..];
            if (string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }
            return true;
        }
    }
}