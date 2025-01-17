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

        private readonly List<string> invalidAssemblyQualifiedNames = new List<string>();
        private readonly HashSet<string> matches = new HashSet<string>();

        public override void SetupForMatching()
        {
            invalidAssemblyQualifiedNames.Clear(); 
            matches.Clear();

            foreach (var typeRef in componentType)
            {
                if (typeRef == null)
                    continue;

                if (!typeRef.IsValid())
                    continue;

                var assemblyQualifiedName = typeRef.AssemblyQualifiedName;
                var type = Type.GetType(assemblyQualifiedName);
                if (type == null)
                {
                    invalidAssemblyQualifiedNames.Add(assemblyQualifiedName);
                }
                else
                {
                    foreach (var result in SearchService.Request($"p: prefab=any t={type.FullName}", SearchFlags.Synchronous))
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
        }

        public override bool Validate(out AssetFilterValidationError error)
        {
            if (invalidAssemblyQualifiedNames.Count >= 1)
            {
                error = new AssetFilterValidationError(
                    this,
                    invalidAssemblyQualifiedNames
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

                result.Insert(0, "Component: ");
            }

            return result.ToString();
        }
    }
}