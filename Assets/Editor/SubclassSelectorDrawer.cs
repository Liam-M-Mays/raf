// Assets/Editor/SubclassSelectorDrawer.cs
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure it's a managed reference field
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.HelpBox(position, "Use [SerializeReference] with SubclassSelector.", MessageType.Error);
            return;
        }

        // Draw label and get base rect
        position = EditorGUI.PrefixLabel(position, label);

        // Determine the base type (abstract MoveState)
        Type baseType = fieldInfo.FieldType;

        // Get all concrete subclasses
        Type[] types = UnityEditor.TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => t.IsClass && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();

        string[] typeNames = types.Select(t => t.Name).Prepend("(None)").ToArray();

        // Get current instantiated type
        Type currentType = GetCurrentType(property);
        int index = currentType != null ? Array.IndexOf(types, currentType) + 1 : 0;

        // Draw dropdown selector
        int newIndex = EditorGUI.Popup(position, index, typeNames);

        if (newIndex != index)
        {
            if (newIndex == 0)
                property.managedReferenceValue = null;
            else
                property.managedReferenceValue = Activator.CreateInstance(types[newIndex - 1]);

            property.serializedObject.ApplyModifiedProperties();
        }

        // Draw the selected object’s fields below dropdown
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property, includeChildren: true);
            EditorGUI.indentLevel--;
        }
    }

    // Draw height properly
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    private Type GetCurrentType(SerializedProperty property)
    {
        string fullType = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullType)) return null;

        var split = fullType.Split(' ');
        string assemblyName = split[0];
        string typeName = split[1];

        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName)
            ?.GetType(typeName);
    }
}
