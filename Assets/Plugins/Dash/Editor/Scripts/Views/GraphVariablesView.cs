/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Dash.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Object = System.Object;

namespace Dash
{
    public class GraphVariablesView : ViewBase
    {
        private Vector2 scrollPosition;

        public GraphVariablesView()
        {

        }

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (Graph == null || !Graph.showVariables)
                return;

            Rect rect = new Rect(20, 30, 340, 200);
            
            DrawBoxGUI(rect, "Graph Variables", TextAnchor.UpperCenter);

            GUILayout.BeginArea(new Rect(rect.x+5, rect.y+30, rect.width-10, rect.height-79));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);
            
            EditorGUI.BeginChangeCheck();

            GameObject boundObject = Graph.Controller == null ? null : Graph.Controller.gameObject;
            
            int index = 0;
            foreach (var variable in Graph.variables)
            {
                //var r = new Rect(0, 25 + 24 * index, rect.width, 30);
                GUIVariableUtils.VariableField(Graph.variables, variable.Name, boundObject);
                EditorGUILayout.Space(4);
                index++;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Graph);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (GUI.Button(new Rect(rect.x + 4, rect.y + rect.height - 48, rect.width - 8, 20), "Add Variable"))
            {
                Debug.Log("here");
                TypesMenu.Show(OnAddVariable);
            }
            
            if (GUI.Button(new Rect(rect.x + 4, rect.y + rect.height - 24, rect.width/2-6, 20), "Copy Variables"))
            {
                DashEditorCore.CopyVariables(Graph.variables);
            }
            
            if (GUI.Button(new Rect(rect.x + rect.width/2 + 2, rect.y + rect.height - 24, rect.width/2-6, 20), "Paste Variables"))
            {
                DashEditorCore.PasteVariables(Graph.variables, Graph.Controller != null ? Graph.Controller.gameObject : null);
            }

            UseEvent(new Rect(rect.x, rect.y, rect.width, rect.height));
        }

        void OnAddVariable(Type p_type)
        {
            string name = "new"+p_type.ToString().Substring(p_type.ToString().LastIndexOf(".")+1);

            int index = 0;
            while (Graph.variables.HasVariable(name + index)) index++;
            
            Graph.variables.AddVariableByType((Type)p_type, name+index, null);
        }
    }
}