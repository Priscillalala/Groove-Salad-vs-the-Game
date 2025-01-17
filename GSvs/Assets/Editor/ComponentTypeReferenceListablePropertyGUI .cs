using SmartAddresser.Editor.Core.Models.Shared.AssetGroups;
using SmartAddresser.Editor.Core.Tools.Addresser.Shared;
using SmartAddresser.Editor.Foundation.ListableProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GSvs.Editor
{
    public class ComponentTypeReferenceListablePropertyGUI : ListablePropertyGUI<TypeReference>
    {
        private const string TempControlName = "ComponentTypeReferenceListablePropertyGUI.TempControl";

        private static readonly PropertyInfo Name = typeof(TypeReference).GetProperty(nameof(TypeReference.Name), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo FullName = typeof(TypeReference).GetProperty(nameof(TypeReference.FullName), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo AssemblyQualifiedName = typeof(TypeReference).GetProperty(nameof(TypeReference.AssemblyQualifiedName), BindingFlags.Instance | BindingFlags.Public);

        public ComponentTypeReferenceListablePropertyGUI(string displayName, ListableProperty<TypeReference> list)
            : base(displayName, list, (rect, label, value, onValueChanged) =>
            {
                var buttonText = value.Name;
                if (string.IsNullOrEmpty(buttonText))
                    buttonText = "-";

                var propertyRect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
                GUI.SetNextControlName(TempControlName);
                if (EditorGUI.DropdownButton(propertyRect, new GUIContent(buttonText), FocusType.Passive))
                {
                    GUI.FocusControl(TempControlName);
                    var dropdown = new ComponentTypeSelectDropdown(new AdvancedDropdownState());

                    void OnItemSelected(TypeSelectDropdown.Item item)
                    {
                        Name.SetValue(value, item.TypeName);
                        FullName.SetValue(value, item.FullName);
                        AssemblyQualifiedName.SetValue(value, item.AssemblyQualifiedName);
                        onValueChanged(value);
                        dropdown.OnItemSelected -= OnItemSelected;
                    }

                    dropdown.OnItemSelected += OnItemSelected;
                    dropdown.Show(propertyRect);
                }
            }, () => new TypeReference())
        {
        }
    }
}