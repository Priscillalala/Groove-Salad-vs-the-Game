using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GSvs.Core.AssetManipulation
{
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class AssetManipulatorAttribute : Attribute
    {
        public static AsyncOperationHandle ExecuteAsync(MethodInfo method)
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