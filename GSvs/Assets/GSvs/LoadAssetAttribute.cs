using System;

namespace GSvs
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LoadAssetAttribute : Attribute
    {
        public string key;

        public LoadAssetAttribute(string key)
        {
            this.key = key;
        }
    }
}