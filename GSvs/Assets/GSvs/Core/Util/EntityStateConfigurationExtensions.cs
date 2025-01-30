using HG.GeneralSerializer;
using RoR2;

namespace GSvs.Core.Util
{
    public static class EntityStateConfigurationExtensions
    {
        public static bool SetFieldValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            SerializedFieldCollection serializedFieldsCollection = entityStateConfiguration.serializedFieldsCollection;
            if (serializedFieldsCollection.serializedFields == null)
            {
                return false;
            }
            for (int i = 0; i < serializedFieldsCollection.serializedFields.Length; i++)
            {
                ref SerializedField serializedField = ref serializedFieldsCollection.serializedFields[i];
                if (serializedField.fieldName == fieldName)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
                    {
                        serializedField.fieldValue.objectValue = value as UnityEngine.Object;
                        return true;
                    }
                    else if (StringSerializer.CanSerializeType(typeof(T)))
                    {
                        serializedField.fieldValue.stringValue = StringSerializer.Serialize(typeof(T), value);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}