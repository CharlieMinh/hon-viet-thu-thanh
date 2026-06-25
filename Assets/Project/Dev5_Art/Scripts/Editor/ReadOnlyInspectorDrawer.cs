using UnityEditor;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Custom Property Drawer cho thuộc tính [ReadOnlyInspector]
    /// giúp hiển thị các biến trên Inspector nhưng không cho phép chỉnh sửa.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}
