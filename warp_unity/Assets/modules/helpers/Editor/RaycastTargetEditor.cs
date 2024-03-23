using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

// from: https://answers.unity.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html
[CanEditMultipleObjects, CustomEditor(typeof(RaycastTarget), false)]
public class RaycastTargetEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();
        EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
        // skipping AppearanceControlsGUI
        base.RaycastControlsGUI();
        base.serializedObject.ApplyModifiedProperties();
    }
}
