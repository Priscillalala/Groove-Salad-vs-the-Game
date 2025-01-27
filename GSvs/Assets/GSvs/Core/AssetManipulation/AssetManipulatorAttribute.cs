using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GSvs.Core.AssetManipulation
{
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AssetManipulatorAttribute : Attribute
    {
        public static AsyncOperationHandle ProcessAsync(Type type)
        {
            if (!IsDefined(type, typeof(AssetManipulatorAttribute)))
            {
                return default;
            }
            List<AsyncOperationHandle> operations = new List<AsyncOperationHandle>();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (IsDefined(method, typeof(AssetManipulatorAttribute)))
                {
                    operations.Add(ProcessAsync(method));
                }
            }
            if (operations.Count > 0)
            {
                return Addressables.ResourceManager.CreateGenericGroupOperation(operations, true);
            }
            else
            {
                return default;
            }
        }

        public static AsyncOperationHandle ProcessAsync(MethodInfo method)
        {
            List<AsyncOperationHandle> loadAssetOperations = new List<AsyncOperationHandle>();
            foreach (var parameter in method.GetParameters())
            {
                BaseLoadAssetAttribute loadAssetAttribute = parameter.GetCustomAttribute<BaseLoadAssetAttribute>();
                loadAssetOperations.Add(loadAssetAttribute.LoadAssetAsync(parameter.ParameterType));
            }
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(loadAssetOperations, true);
            groupOp.Completed += InvokeAssetManipulator;
            return groupOp;

            void InvokeAssetManipulator(AsyncOperationHandle<IList<AsyncOperationHandle>> assets)
            {
                method.Invoke(null, assets.Result.Select(x => x.Result).ToArray());
            }
        }
    }
}