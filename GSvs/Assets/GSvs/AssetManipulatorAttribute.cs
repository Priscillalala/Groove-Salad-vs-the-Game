using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GSvs
{
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class AssetManipulatorAttribute : Attribute
    {
        public static void Apply(MethodInfo method)
        {
            List<AsyncOperationHandle> loadAssetOperations = new List<AsyncOperationHandle>();
            foreach (var parameter in method.GetParameters())
            {
                LoadAssetAttribute loadAssetAttribute = parameter.GetCustomAttribute<LoadAssetAttribute>();
                Type parameterType = parameter.ParameterType;
                var loadAssetLocationsOp = Addressables.LoadResourceLocationsAsync(loadAssetAttribute.key, parameterType);
                loadAssetOperations.Add(Addressables.ResourceManager.CreateChainOperation(loadAssetLocationsOp, LoadAsset, true));

                AsyncOperationHandle<object> LoadAsset(AsyncOperationHandle<IList<IResourceLocation>> assetLocations)
                {
                    IResourceLocation assetLocation = assetLocations.Result.FirstOrDefault();
                    if (assetLocation == null)
                    {
                        return Addressables.ResourceManager.CreateCompletedOperationWithException<object>(null, new KeyNotFoundException(loadAssetAttribute.key));
                    }
                    return Addressables.ResourceManager.CreateChainOperation(
                        Addressables.ResourceManager.ProvideResource(assetLocation, parameterType, true),
                        asset => Addressables.ResourceManager.CreateCompletedOperationWithException(asset.Result, asset.OperationException),
                        false
                        );
                }
            }
            Addressables.ResourceManager.CreateGenericGroupOperation(loadAssetOperations, true).Completed += InvokeAssetManipulator;

            void InvokeAssetManipulator(AsyncOperationHandle<IList<AsyncOperationHandle>> assets)
            {
                method.Invoke(null, assets.Result.Select(x => x.Result).ToArray());
            }
        }
    }
}