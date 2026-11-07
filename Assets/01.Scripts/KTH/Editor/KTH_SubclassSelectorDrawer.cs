using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// [SerializeReference, KTH_SubclassSelector] 필드 옆에 구현 클래스를 고르는 드롭다운을 그린다
// 리스트에 붙이면 각 칸마다 드롭다운이 생긴다
[CustomPropertyDrawer(typeof(KTH_SubclassSelectorAttribute))]
public class KTH_SubclassSelectorDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        Type current = property.managedReferenceValue?.GetType();
        string typeName = current == null ? "(패턴 선택)" : ObjectNames.NicifyVariableName(current.Name);

        // 폴드아웃보다 먼저 그려야 버튼 클릭을 폴드아웃이 가져가지 않는다
        float labelWidth = EditorGUIUtility.labelWidth;
        Rect buttonRect = new Rect(position.x + labelWidth + 2f, position.y,
            position.width - labelWidth - 2f, EditorGUIUtility.singleLineHeight);
        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(typeName), FocusType.Keyboard))
            ShowMenu(property, current);

        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    private void ShowMenu(SerializedProperty property, Type current)
    {
        // 메뉴 콜백은 나중에 불리므로 프로퍼티 대신 경로를 들고 있다가 다시 찾는다
        SerializedObject so = property.serializedObject;
        string path = property.propertyPath;

        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("(없음)"), current == null, () => Assign(so, path, null));
        menu.AddSeparator("");

        foreach (Type t in GetCandidateTypes(BaseType()))
        {
            Type type = t;
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), type == current,
                () => Assign(so, path, Activator.CreateInstance(type)));
        }

        menu.ShowAsContext();
    }

    private static void Assign(SerializedObject so, string path, object value)
    {
        so.Update();
        SerializedProperty p = so.FindProperty(path);
        if (p == null) return;
        p.managedReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    // List<T>나 T[]에 붙었으면 원소 타입 T를 기준으로 고른다
    private Type BaseType()
    {
        Type t = fieldInfo.FieldType;
        if (t.IsArray) return t.GetElementType();
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) return t.GetGenericArguments()[0];
        return t;
    }

    private static IEnumerable<Type> GetCandidateTypes(Type baseType)
    {
        return TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract && !t.IsGenericType
                        && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                        && t.IsDefined(typeof(SerializableAttribute), false)
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name);
    }
}
