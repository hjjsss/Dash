using Dash.Editor;
using UnityEditor;
using UnityEngine;

namespace Dash
{
    [CustomEditor(typeof(DashVariablesController))]
    public class DashVariablesControllerInspector : UnityEditor.Editor
    {
        protected DashVariables variables => ((DashVariablesController) target).Variables;

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(Resources.Load<Texture>("Textures/dash"), GUILayout.ExpandWidth(true), GUILayout.Height(40));
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 100;

            if (PrefabUtility.GetPrefabInstanceStatus(target) != PrefabInstanceStatus.NotAPrefab)
            {
                EditorGUILayout.LabelField("Prefab overrides are not supported.");
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                GUIVariableUtils.DrawVariablesInspector(variables, ((DashVariablesController) target).gameObject);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
        }
    }
}