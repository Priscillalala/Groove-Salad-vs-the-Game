using SmartAddresser.Editor.Core.Models.Shared.AssetGroups;
using SmartAddresser.Editor.Core.Models.Shared.AssetGroups.AssetFilterImpl;
using SmartAddresser.Editor.Core.Models.Shared.AssetGroups.ValidationError;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace GSvs.Editor
{
    [Serializable]
    [AssetFilter("Component Filter", "Component Filter")]
    public class ComponentBasedAssetFilter : AssetFilterBase
    {
        public TypeReferenceListableProperty componentType = new TypeReferenceListableProperty();
        public bool matchWithDerivedComponentTypes = true;
        public bool searchChildren = false;

        private readonly List<string> _invalidAssemblyQualifiedNames = new List<string>();
        private readonly List<Type> _validComponentTypes = new List<Type>();
        private readonly Dictionary<Type, bool> _resultCache = new Dictionary<Type, bool>();
        private readonly object _resultCacheLocker = new object();
        private readonly List<Component> _componentsList = new List<Component>();

        private readonly HashSet<string> matches = new HashSet<string>();

        public override void SetupForMatching()
        {
            _validComponentTypes.Clear();
            _invalidAssemblyQualifiedNames.Clear();
            foreach (var typeRef in componentType)
            {
                if (typeRef == null)
                    continue;

                if (!typeRef.IsValid())
                    continue;

                var assemblyQualifiedName = typeRef.AssemblyQualifiedName;
                var type = Type.GetType(assemblyQualifiedName);
                if (type == null)
                    _invalidAssemblyQualifiedNames.Add(assemblyQualifiedName);
                else
                    _validComponentTypes.Add(type);
            }

            _resultCache.Clear();

            matches.Clear();
            foreach (var validComponentType in _validComponentTypes)
            {
                foreach (var result in SearchService.Request($"p: prefab=any t={validComponentType.FullName}", SearchFlags.Synchronous))
                {
                    string assetPath = SearchUtils.GetAssetPath(result);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        Debug.Log($"Matched {assetPath}");
                        matches.Add(assetPath);
                    }
                }
            }
        }

        public override bool Validate(out AssetFilterValidationError error)
        {
            if (_invalidAssemblyQualifiedNames.Count >= 1)
            {
                error = new AssetFilterValidationError(
                    this,
                    _invalidAssemblyQualifiedNames
                        .Select(qualifiedName => $"Invalid type reference: {qualifiedName}")
                        .ToArray());
                return false;
            }

            error = null;
            return true;
        }

        public override bool IsMatch(string assetPath, Type assetType, bool isFolder)
        {
            return matches.Contains(assetPath);
            if (assetType != typeof(GameObject))
            {
                return false;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab || !PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                return false;
            }

            if (searchChildren)
            {
                prefab.GetComponentsInChildren(_componentsList);
            }
            else
            {
                prefab.GetComponents(_componentsList);
            }

            bool result = false;
            foreach (Component component in _componentsList)
            {
                Type componentType = component.GetType();

                if (_resultCache.TryGetValue(componentType, out result))
                {
                    if (result)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                foreach (Type validComponentType in _validComponentTypes)
                {
                    if (componentType == validComponentType)
                    {
                        result = true;
                        break;
                    }

                    if (matchWithDerivedComponentTypes && componentType.IsSubclassOf(validComponentType))
                    {
                        result = true;
                        break;
                    }
                }

                lock (_resultCacheLocker)
                {
                    _resultCache.Add(componentType, result);
                }
            }

            _componentsList.Clear();

            return result;
        }

        public override string GetDescription()
        {
            var result = new StringBuilder();
            var elementCount = 0;
            foreach (var type in componentType)
            {
                if (type == null || string.IsNullOrEmpty(type.Name))
                    continue;

                if (elementCount >= 1)
                    result.Append(" || ");

                result.Append(type.Name);
                elementCount++;
            }

            if (result.Length >= 1)
            {
                if (elementCount >= 2)
                {
                    result.Insert(0, "( ");
                    result.Append(" )");
                }

                result.Insert(0, "Has Component: ");
            }

            if (matchWithDerivedComponentTypes)
                result.Append(" or derived components");

            return result.ToString();
        }
    }
}