using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GSvs.Core.AssetManipulation
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class BaseLoadAssetAttribute : Attribute
    {
        public abstract AsyncOperationHandle LoadAssetAsync(Type assetType);
    }
}