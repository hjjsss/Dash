/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dash.Editor
{
    public class DashValidationWindow : EditorWindow
    {
        private Vector2 _scrollPositionScanned;
        private bool _isDirty = false;

        public static DashValidationWindow Instance { get; private set; }
        
        [MenuItem ("Tools/Dash/Validator")]
        public static DashValidationWindow InitValidationWindow()
        {
            Instance = GetWindow<DashValidationWindow>();
            Instance.titleContent = new GUIContent("Dash Validator");

            return Instance;
        }

        private void OnGUI()
        {
            var rect = new Rect(0, 0, position.width, position.height);

            var titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(1, .5f, 0);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 16;
            
            var infoStyle = new GUIStyle();
            infoStyle.normal.textColor = Color.gray;
            infoStyle.alignment = TextAnchor.MiddleCenter;

            var scrollViewStyle = new GUIStyle();
            scrollViewStyle.normal.background = DashEditorCore.GetColorTexture(new Color(.1f, .1f, .1f));
            
            GUILayout.Space(4);
            GUILayout.Label("Validation Scanner", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label("Current version: "+DashCore.VERSION, infoStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(2);

            _scrollPositionScanned = GUILayout.BeginScrollView(_scrollPositionScanned, scrollViewStyle, GUILayout.ExpandWidth(true), GUILayout.Height(rect.height-90));
            GUILayout.BeginVertical();

            if (DashValidationScanner.Log != null)
            {
                foreach ((string,Color) log in DashValidationScanner.Log)
                {
                    GUILayout.BeginHorizontal();
                    GUI.color = log.Item2;
                    GUILayout.Label(log.Item1);
                    GUILayout.EndHorizontal();
                }
            }

            GUI.color = Color.white;

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            
            GUILayout.BeginVertical();
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();

            bool scan = GUILayout.Button("Validate Project", GUILayout.Height(40));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (_isDirty)
            {
                _isDirty = false;
                Repaint();
            }

            if (scan)
            {
                DashValidationScanner.Scan();
            }
        }
    }
}