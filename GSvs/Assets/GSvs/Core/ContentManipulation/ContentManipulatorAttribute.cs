using GSvs.Core.Configuration;
using HG.Reflection;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace GSvs.Core.ContentManipulation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentManipulatorAttribute : SearchableAttribute
    {
        private static bool init;
        private static Action initImplementation;

        public static event Action InitImplementation
        {
            add
            {
                if (init)
                {
                    value();
                    return;
                }
                initImplementation += value;
            }
            remove => initImplementation -= value;
        }

        [InitDuringStartup]
        private static void Init()
        {
            if (init)
            {
                throw new InvalidOperationException();
            }
            foreach (var contentManipulatorAttribute in GetInstances<ContentManipulatorAttribute>())
            {
                Type contentManipulatorType = (Type)contentManipulatorAttribute.target;
                RuntimeHelpers.RunClassConstructor(contentManipulatorType.TypeHandle);
                ConfigAttribute.Process(contentManipulatorType);
            }
            init = true;
            initImplementation?.Invoke();
            initImplementation = null;
        }
    }
}