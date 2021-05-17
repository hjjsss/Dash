/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dash.Editor
{
    public class DashAOTWindow : EditorWindow
    {
        private Vector2 _scrollPositionScanned;
        private Vector2 _scrollPositionExplicit;
        private bool _isDirty = false;

        private bool _generateLinkXml = true;
        private bool _includeOdin = false;

        public static DashAOTWindow Instance { get; private set; }
        
        [MenuItem ("Tools/Dash/AOT")]
        public static DashAOTWindow InitDebugWindow()
        {
            Instance = GetWindow<DashAOTWindow>();
            Instance.titleContent = new GUIContent("Dash AOT Editor");

            return Instance;
        }

        private void OnGUI()
        {
            var rect = new Rect(0, 0, position.width, position.height);

            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            
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
            GUILayout.Label("Scanned types", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label("Last scan found "+(DashEditorCore.Config.scannedAOTTypes == null ? 0 : DashEditorCore.Config.scannedAOTTypes.Count)+" types", infoStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(2);

            _scrollPositionScanned = GUILayout.BeginScrollView(_scrollPositionScanned, scrollViewStyle, GUILayout.ExpandWidth(true), GUILayout.Height(rect.height/2-85));
            GUILayout.BeginVertical();
            
            if (DashEditorCore.Config.scannedAOTTypes != null)
            {
                foreach (Type type in DashEditorCore.Config.scannedAOTTypes)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".") +
                                    type.GetReadableTypeName());
                    if (GUILayout.Button("Remove", GUILayout.Width(120)))
                    {
                        DashAOTScanner.RemoveScannedAOTType(type);
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();


            GUILayout.Space(4);
            GUILayout.Label("Explicit Types", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label("You have "+(DashEditorCore.Config.explicitAOTTypes == null ? 0 : DashEditorCore.Config.explicitAOTTypes.Count)+" explicitly defined types.", infoStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(2);
            
            _scrollPositionExplicit = GUILayout.BeginScrollView(_scrollPositionExplicit, scrollViewStyle, GUILayout.ExpandWidth(true), GUILayout.Height(rect.height/2-85));
            GUILayout.BeginVertical();
            
            if (DashEditorCore.Config.explicitAOTTypes != null)
            {
                foreach (Type type in DashEditorCore.Config.explicitAOTTypes)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".") +
                                    type.GetReadableTypeName());
                    if (GUILayout.Button("Remove", GUILayout.Width(120)))
                    {
                        DashAOTScanner.RemoveExplicitAOTType(type);
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            GUILayout.Space(4);
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal();
            _generateLinkXml = GUILayout.Toggle(_generateLinkXml, new GUIContent("Generate Link Xml"), GUILayout.Width(140));
            _includeOdin = GUILayout.Toggle(_includeOdin, new GUIContent("Include Odin Assembly"));

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();

            bool generate = GUILayout.Button("Generate AOT DLL", GUILayout.Height(40));

            bool scan = GUILayout.Button("Scan Types", GUILayout.Height(40));
            
            if (GUILayout.Button("Add Explicit Type", GUILayout.Height(40)))
            {
                AddTypeContextMenu.ShowAsPopup();
            }

            GUILayout.EndHorizontal();

            var dll = PluginImporter.GetAtPath(DashEditorCore.Config.AOTAssemblyPath + "/" +
                                                  DashEditorCore.Config.AOTAssemblyName+".dll");

            if (dll != null)
            {
                GUILayout.Label("Assembly generated in " + DashEditorCore.Config.AOTAssemblyPath + "/" +
                                DashEditorCore.Config.AOTAssemblyName + ".dll" + " last generated on " +
                                DashEditorCore.Config.AOTAssemblyGeneratedTime);
            }
            else
            {
                GUILayout.Label("No generated Dash AOT Assembly found.");
            }

            GUILayout.EndVertical();

            if (_isDirty)
            {
                _isDirty = false;
                Repaint();
            }

            if (scan)
            {
                DashAOTScanner.Scan();
            } else if (generate)
            {
                DashAOTScanner.GenerateDLL(_generateLinkXml, _includeOdin);
            }
        }
    }
}