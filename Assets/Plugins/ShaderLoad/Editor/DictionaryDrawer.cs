using UnityEditor;
using UnityEngine;
using ShaderLoad.Util;

namespace ShaderLoad.Editor
{
    /// <summary>
    /// Shared drawer logic for FloatDictionary / VectorDictionary.
    /// Renders keys as read-only labels, values as editable fields.
    /// No add / remove / reorder controls.
    /// </summary>
    public abstract class DictionaryDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Gap = 2f;

        protected abstract void DrawValue(Rect rect, SerializedProperty valueProp);

        private float GetRowHeight() => LineHeight + Gap;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LineHeight;

            var entries = property.FindPropertyRelative("_entries");
            float h = LineHeight + Gap;                     // foldout header
            h += entries.arraySize * GetRowHeight();         // rows
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // ---- foldout ----
            Rect foldoutRect = new(position.x, position.y, position.width, LineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            position.y += LineHeight + Gap;

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            var entries = property.FindPropertyRelative("_entries");

            // ---- entries ----
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var keyProp = entry.FindPropertyRelative("key");
                var valProp = entry.FindPropertyRelative("value");

                float keyW = position.width * 0.38f;
                float valW = position.width * 0.58f;

                Rect keyRect = new(position.x, position.y, keyW, LineHeight);
                Rect valRect = new(position.x + keyW + 4f, position.y, valW, LineHeight);

                EditorGUI.LabelField(keyRect, keyProp.stringValue);
                DrawValue(valRect, valProp);

                position.y += GetRowHeight();
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(FloatDictionary))]
    public sealed class FloatDictionaryDrawer : DictionaryDrawer
    {
        protected override void DrawValue(Rect rect, SerializedProperty valueProp)
        {
            EditorGUI.PropertyField(rect, valueProp, GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(VectorDictionary))]
    public sealed class VectorDictionaryDrawer : DictionaryDrawer
    {
        protected override void DrawValue(Rect rect, SerializedProperty valueProp)
        {
            var x = valueProp.FindPropertyRelative("x");
            var y = valueProp.FindPropertyRelative("y");
            var z = valueProp.FindPropertyRelative("z");
            var w = valueProp.FindPropertyRelative("w");

            float spacing = 2f;
            float w4 = (rect.width - spacing * 3f) / 4f;

            Rect r = new(rect.x, rect.y, w4, rect.height);
            EditorGUI.PropertyField(r, x, GUIContent.none);
            r.x += w4 + spacing;
            EditorGUI.PropertyField(r, y, GUIContent.none);
            r.x += w4 + spacing;
            EditorGUI.PropertyField(r, z, GUIContent.none);
            r.x += w4 + spacing;
            EditorGUI.PropertyField(r, w, GUIContent.none);
        }
    }
}
