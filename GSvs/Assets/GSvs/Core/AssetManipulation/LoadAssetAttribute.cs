using GSvs.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GSvs.Core.AssetManipulation
{
    public class LoadAssetAttribute : BaseLoadAssetAttribute
    {
        public string key;

        public LoadAssetAttribute(string key)
        {
            this.key = key;
        }

        public override AsyncOperationHandle LoadAssetAsync(Type assetType)
        {
            var loadAssetLocationsOp = Addressables.LoadResourceLocationsAsync(key, assetType);
            return Addressables.ResourceManager.CreateChainOperation(loadAssetLocationsOp, LoadAssetFromLocationsAsync, true);

            AsyncOperationHandle<object> LoadAssetFromLocationsAsync(AsyncOperationHandle<IList<IResourceLocation>> assetLocations)
            {
                IResourceLocation assetLocation = assetLocations.Result.FirstOrDefault();
                if (assetLocation == null)
                {
                    return Addressables.ResourceManager.CreateCompletedOperationWithException<object>(null, new KeyNotFoundException(key));
                }
                var loadAssetOp = Addressables.ResourceManager.ProvideResource(assetLocation, assetType, true);
                return Addressables.ResourceManager.CreateChainOperation(loadAssetOp, ConvertAssetResult, false);
            }

            static AsyncOperationHandle<object> ConvertAssetResult(AsyncOperationHandle asset)
            {
                return Addressables.ResourceManager.CreateCompletedOperationWithException(asset.Result, asset.OperationException);
            }
        }
    }
}