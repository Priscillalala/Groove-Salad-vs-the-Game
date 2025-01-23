using BepInEx.Configuration;
using System;

namespace GSvs.Core.Configuration
{
    public interface IConfigValue
    {
        public object BoxedValue { get; }
        public Type ValueType { get; }
        
        public void BindValue(ConfigEntryBase configEntry);
    }
}