using SmartAddresser.Editor.Core.Models.Shared.AssetGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Item = SmartAddresser.Editor.Core.Tools.Addresser.Shared.TypeSelectDropdown.Item;

namespace GSvs.Editor
{
    public class ComponentTypeSelectDropdown : AdvancedDropdown
    {
        private const string RootItemKey = "Type";

        public ComponentTypeSelectDropdown(AdvancedDropdownState state) : base(state)
        {
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
        }

        public event Action<Item> OnItemSelected;

        protected override AdvancedDropdownItem BuildRoot()
        {
            var excludeTypes = new[]
            {
                typeof(Component),
                typeof(UnityEditor.Editor),
                typeof(EditorWindow)
            };

            var types = TypeCache.GetTypesDerivedFrom<Component>().Where(x => x.IsPublic && !x.IsAbstract);

            var items = new Dictionary<string, Item>();
            var rootItem = new Item(RootItemKey, RootItemKey, RootItemKey, RootItemKey);
            items.Add(RootItemKey, rootItem);

            foreach (var assemblyQualifiedName in types.Select(x => x.AssemblyQualifiedName).OrderBy(x => x))
            {
                var itemFullName = assemblyQualifiedName.Split(',')[0];
                while (true)
                {
                    var lastDotIndex = itemFullName.LastIndexOf('.');
                    if (!items.ContainsKey(itemFullName))
                    {
                        var typeName =
                            lastDotIndex == -1 ? itemFullName : itemFullName.Substring(lastDotIndex + 1);
                        var item = new Item(typeName, typeName, itemFullName, assemblyQualifiedName);
                        items.Add(itemFullName, item);
                    }

                    if (itemFullName.IndexOf('.') == -1) break;

                    itemFullName = itemFullName.Substring(0, lastDotIndex);
                }
            }

            foreach (var item in items)
            {
                if (item.Key == RootItemKey)
                    continue;

                var fullName = item.Key;
                if (fullName.LastIndexOf('.') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = fullName.Substring(0, fullName.LastIndexOf('.'));
                    items[parentName].AddChild(item.Value);
                }
            }

            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            OnItemSelected?.Invoke((Item)item);
        }

        /*public class Item : AdvancedDropdownItem
        {
            public Item(string displayName, Type type) : base(
                displayName)
            {
                if (type != null)
                {
                    TypeReference = TypeReference.Create(type);
                }
            }

            public TypeReference TypeReference;
        }*/
    }
}