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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LineHeight;

            var entries = property.FindPropertyRelative("_entries");
            float h = LineHeight + Gap;               // foldout header
            h += LineHeight + Gap;                     // "Size: N" label
            h += entries.arraySize * (LineHeight + Gap); // rows
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // ---- foldout ----
            Rect foldoutRect = new(position.x, position.y, position.width, LineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            position.y += LineHeight + Gap;

            if (property.isExpanded)
            {
                var entries = property.FindPropertyRelative("_entries");

                // ---- read-only size label (instead of an editable array-size field) ----
                Rect sizeRect = new(position.x, position.y, position.width, LineHeight);
                EditorGUI.LabelField(sizeRect, "Size", entries.arraySize.ToString());
                position.y += LineHeight + Gap;

                // ---- entries ----
                EditorGUI.indentLevel++;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var entry    = entries.GetArrayElementAtIndex(i);
                    var keyProp  = entry.FindPropertyRelative("key");
                    var valProp  = entry.FindPropertyRelative("value");

                    float keyW = position.width * 0.38f;
                    float valW = position.width * 0.58f;

                    Rect keyRect = new(position.x,     position.y, keyW, LineHeight);
                    Rect valRect = new(position.x + keyW + 4f, position.y, valW, LineHeight);

                    // key: read-only label
                    EditorGUI.LabelField(keyRect, keyProp.stringValue);
                    // value: editable — type is handled automatically (float or Vector4)
                    EditorGUI.PropertyField(valRect, valProp, GUIContent.none);

                    position.y += LineHeight + Gap;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(FloatDictionary))]
    public sealed class FloatDictionaryDrawer : DictionaryDrawer { }

    [CustomPropertyDrawer(typeof(VectorDictionary))]
    public sealed class VectorDictionaryDrawer : DictionaryDrawer { }
}
