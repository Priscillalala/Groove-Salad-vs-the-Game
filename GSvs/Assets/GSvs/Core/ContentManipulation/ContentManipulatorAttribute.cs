using RoR2;
using System;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace GSvs.Core.ContentManipulation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentManipulatorAttribute : SearchableAttribute
    {
        [InitDuringStartup]
        private static void Init()
        {
            foreach (var contentManipulatorAttribute in GetInstances<ContentManipulatorAttribute>())
            {
                Type contentManipulatorType = (Type)contentManipulatorAttribute.target;
                Activator.CreateInstance(contentManipulatorType);
            }
        }
    }
}