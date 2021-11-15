﻿/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dash.Editor
{
    public class GraphView : ViewBase
    {
        private bool _initialized = false;

        private float Zoom => DashEditorCore.EditorConfig.zoom;

        private Rect zoomedRect;

        private DraggingType dragging = DraggingType.NONE;
        private Rect selectedRegion = Rect.zero;
        
        private bool _rightDrag = false;
        private Vector2 _rightDragStart;

        private Texture backgroundTexture;
        private Texture whiteRectTexture;

        private GraphMenuView _graphMenuView;

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (!_initialized)
            {
                backgroundTexture = Resources.Load<Texture>("Textures/graph_background");
                whiteRectTexture = Resources.Load<Texture>("Textures/white_rect_64");
                _graphMenuView = new GraphMenuView();
                GUIScaleUtils.CheckInit();
                _initialized = true;
            }
            
            zoomedRect = new Rect(0, 0, p_rect.width, p_rect.height);

            GUI.color = DashEditorCore.Previewer.IsPreviewing ? new Color(0f, 1f, .2f, 1) :  new Color(0f, .1f, .2f, 1);
            GUI.Box(p_rect, "");

            if (Graph != null)
            {
                // Draw background texture
                GUI.color = new Color(0, 0, 0, .4f);
                GUI.DrawTextureWithTexCoords(zoomedRect, backgroundTexture,
                    new Rect(-Graph.viewOffset.x / backgroundTexture.width,
                        Graph.viewOffset.y / backgroundTexture.height,
                        Zoom * p_rect.width / backgroundTexture.width,
                        Zoom * p_rect.height / backgroundTexture.height), true);
                GUI.color = Color.white;
                // Draw graph
                GUIScaleUtils.BeginScale(ref zoomedRect, new Vector2(p_rect.width/2, p_rect.height/2), Zoom, false, false);
                Graph.DrawGUI(zoomedRect);
                //Graph.DrawComments(zoomedRect);
                GUIScaleUtils.EndScale();
                
                Graph.DrawComments(p_rect, false);
                
                DrawHelp(p_rect);

                DrawControllerInfo(p_rect);

                DrawPreviewInfo(p_rect);
                
                DrawSelectingRegion(p_rect);
            }

            DrawTitle(p_rect);
        }
        
        void DrawHelp(Rect p_rect)
        {
            if (Graph == null || Graph.Nodes.Count > 0)
                return;

            string helpString = "RIGHT CLICK to create nodes.\n" +
                         "Hold RIGHT mouse button to DRAG around.";
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 18;
            style.normal.textColor = Color.gray;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(p_rect.x, p_rect.y + 30, p_rect.width, p_rect.height), helpString, style);
        }

        void DrawSelectingRegion(Rect p_rect)
        {
            if (dragging == DraggingType.SELECTION)
            {
                GUI.color = new Color(1, 1, 1, 0.1f);
                GUI.DrawTextureWithTexCoords(selectedRegion, whiteRectTexture, new Rect(0,0,64,64), true);
                GUI.color = Color.white;
            }
        }
        
        void DrawControllerInfo(Rect p_rect)
        {
            if (Graph == null)
                return;
            
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            GUI.color = new Color(1, 1, 1, 0.25f);
            if (Graph.IsBound)
            {
                GUI.Label(new Rect(p_rect.x + 16, p_rect.height - 40, 200, 40), "Bound", style);
            }
            else
            {
                GUI.Label(new Rect(p_rect.x + 16, p_rect.height - 40, 200, 40), "Asset", style);
            }

            if (Graph.Controller != null)
            {
                style.normal.textColor = Color.yellow;
                style.fontSize = 18;
                GUI.Label(new Rect(p_rect.x + 16, p_rect.height - 58, 200, 40), Graph.Controller.name, style);
            }
            
            if (GraphUtils.IsSubGraph(DashEditorCore.EditorConfig.editingGraphPath) && Graph.Controller != null)
            {
                if (GUI.Button(new Rect(p_rect.x + 16, p_rect.height - 98, 100, 32), "GO TO PARENT"))
                {
                    DashEditorCore.EditController(DashEditorCore.EditorConfig.editingGraph.Controller, GraphUtils.GetParentPath(DashEditorCore.EditorConfig.editingGraphPath));
                }
            }
        }

        void DrawPreviewInfo(Rect p_rect)
        {
            if (!DashEditorCore.Previewer.IsPreviewing)
                return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.red;
            if (Graph.Nodes.Exists(n => n.hasErrorsInExecution))
            {
                GUI.Label(new Rect(p_rect.width - 200, p_rect.height - 64, 200, 40), "ERROR!", style);
            }
            style.normal.textColor = new Color(0,1,0,.8f);
            GUI.Label(new Rect(p_rect.width-200, p_rect.height-40, 200,40), "PREVIEWING...", style);
        }
        
        void DrawTitle(Rect p_rect)
        {
            // Draw title background
            Rect titleRect = new Rect(0, 0, p_rect.width, 24);
            GUI.color = new Color(0.1f, 0.1f, .1f, .8f);
            GUI.DrawTexture(titleRect, whiteRectTexture);
            GUI.color = Color.white;

            // Draw graph name
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            if (Graph != null)
            {
                style.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, 0, p_rect.width, 24), new GUIContent("Editing graph:"), style);
                style.normal.textColor = Color.white;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleLeft;
                if (Graph.IsBound)
                {
                    GUI.Label(new Rect(p_rect.width / 2 + 40, 0, p_rect.width, 24),
                        new GUIContent(Graph.Controller.name + (GraphUtils.IsSubGraph(DashEditorCore.EditorConfig.editingGraphPath) ? "/"+DashEditorCore.EditorConfig.editingGraphPath : "")), style);
                }
                else
                {
                    GUI.Label(new Rect(p_rect.width / 2 + 40, 0, p_rect.width, 24),
                        new GUIContent(Graph.name + (GraphUtils.IsSubGraph(DashEditorCore.EditorConfig.editingGraphPath) ? "/"+DashEditorCore.EditorConfig.editingGraphPath : "")), style);
                }
            }
            else
            {
                style.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, 0, p_rect.width, 24), new GUIContent("No graph loaded."), style);
            }

            if (Application.isPlaying && Graph != null && Graph.Controller != null)
            {
                style = new GUIStyle();
                style.fontSize = 18;
                style.normal.textColor = Color.yellow;
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, 32, p_rect.width, 24),
                    new GUIContent("Debugging bound: " + Graph.Controller.name), style);
                GUI.color = Color.white;
            }
            
            // Draw version info
            style = new GUIStyle();
            style.normal.textColor = Color.gray;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(0 + p_rect.width - 75, 0, 70, 24), "Dash Animation System v" + DashRuntimeCore.VERSION,
                style);

            _graphMenuView.Draw(Graph);
        }

        public override void ProcessEvent(Event p_event, Rect p_rect)
        {
            if (Graph == null || !p_rect.Contains(p_event.mousePosition))
                return;

            ProcessZoom(p_event, p_rect);

            if (!Application.isPlaying && !DashEditorCore.Previewer.IsPreviewing && Graph != null)
            {
                ProcessLeftClick(p_event, p_rect);

                ProcessRightClick(p_event, p_rect);
            }

            ProcessDragging(p_event, p_rect);

            if (Graph.connectingNode != null)
                EditorWindow.SetDirty(true);
        }
        
        void ProcessZoom(Event p_event, Rect p_rect)
        {
            if (!p_event.isScrollWheel)
                return;
            
            float zoom = DashEditorCore.EditorConfig.zoom;
            
            float previousZoom = zoom;
            zoom += p_event.delta.y / 12;
            if (zoom < 1) zoom = 1;
            if (zoom > 4) zoom = 4;
            if (previousZoom != zoom && Graph != null)
            {
                Graph.viewOffset.x += (zoom - previousZoom) * p_rect.width / 2 + (p_event.mousePosition.x - p_rect.x - p_rect.width/2) * (zoom - previousZoom);
                Graph.viewOffset.y += (zoom - previousZoom) * p_rect.height / 2 + (p_event.mousePosition.y - p_rect.y - p_rect.height/2) * (zoom - previousZoom);
            }

            DashEditorCore.EditorConfig.zoom = zoom;
            EditorWindow.SetDirty(true);
        }
        
        void ProcessLeftClick(Event p_event, Rect p_rect)
        {
            if (p_event.button != 0)
                return;

            if (p_event.type == EventType.MouseDown)
            {
                GUI.FocusControl("");
            }

            // Select
            if (p_event.type == EventType.MouseDown && !p_event.alt && Graph != null && !p_event.control)
            {
                EditorWindow.SetDirty(true);
                
                NodeBase hitNode = Graph.HitsNode(p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y));
                int hitNodeIndex = Graph.Nodes.IndexOf(hitNode);

                if (!SelectionManager.IsSelected(hitNodeIndex) && (!p_event.shift || hitNodeIndex == 0))
                {
                    SelectionManager.ClearSelection();
                }

                if (hitNodeIndex >= 0)
                {
                    AddSelectedNode(hitNodeIndex);

                    dragging = DraggingType.NODE_DRAG;
                }
                else
                {
                    GraphBox box = Graph.HitsBoxDrag(p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y));

                    if (box != null)
                    {
                        DashEditorCore.selectedBox = box;
                        DashEditorCore.selectedBox.StartDrag();
                        dragging = DraggingType.BOX_DRAG;
                    }
                    else
                    {
                        box = Graph.HitsBoxResize(p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y));

                        if (box != null)
                        {
                            DashEditorCore.selectedBox = box;
                            DashEditorCore.selectedBox.StartResize();
                            dragging = DraggingType.BOX_RESIZE;
                        }
                        else
                        {
                            dragging = DraggingType.SELECTION;
                            DashEditorCore.selectedBox = null;
                            Graph.connectingNode = null;
                            selectedRegion = new Rect(p_event.mousePosition.x, p_event.mousePosition.y, 0, 0);
                        }
                    }
                }
            }

            // Dragging
            if (p_event.type == EventType.MouseDrag)
            {
                switch (dragging)
                {
                    case DraggingType.NODE_DRAG:
                        Vector2 delta = p_event.alt ? Snapping.Snap(p_event.delta, new Vector2(10,10)): p_event.delta;
                        SelectionManager.DragSelectedNodes(delta, Graph);
                        break;
                    case DraggingType.BOX_DRAG:
                        DashEditorCore.selectedBox.Drag(new Vector2(p_event.delta.x * Zoom, p_event.delta.y * Zoom));
                        break;
                    case DraggingType.BOX_RESIZE:
                        DashEditorCore.selectedBox.Resize(new Vector2(p_event.delta.x * Zoom, p_event.delta.y * Zoom));
                        break;
                    case DraggingType.SELECTION:
                        selectedRegion.width += p_event.delta.x;
                        selectedRegion.height += p_event.delta.y;
                        Rect fixedRect = FixRect(selectedRegion);
                        SelectionManager.SelectingNodes(Graph.Nodes.FindAll(n => n.IsInsideRect(fixedRect)).Select(n => n.Index).ToList());
                        break;
                }

                EditorWindow.SetDirty(true);
            }

            if (p_event.type == EventType.MouseUp)
            {
                if (dragging == DraggingType.SELECTION)
                {
                    SelectionManager.SelectingToSelected();
                }

                if (dragging == DraggingType.NODE_DRAG || dragging == DraggingType.BOX_DRAG || dragging == DraggingType.BOX_RESIZE)
                {
                    DashEditorCore.SetDirty();
                }

                dragging = DraggingType.NONE;
                selectedRegion = Rect.zero;
                EditorWindow.SetDirty(true);
            }
        }

        void ProcessRightClick(Event p_event, Rect p_rect)
        {
            if (p_event.button != 1)
                return;

            if (p_event.type == EventType.MouseUp)
            {
                if (!_rightDrag)
                {
                    NodeBase hitNode = Graph.HitsNode(p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y));
                    if (hitNode != null)
                    {
                        NodeContextMenu.Show(hitNode);
                    }
                    else
                    {
                        NodeConnection hitConnection = Graph.HitsConnection(
                            p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y),
                            12);

                        if (hitConnection != null)
                        {
                            ConnectionContextMenu.Show(hitConnection);
                        }
                        else
                        {
                            GraphBox hitRegion =
                                Graph.HitsBoxDrag(p_event.mousePosition * Zoom - new Vector2(p_rect.x, p_rect.y));

                            if (hitRegion != null)
                            {
                                BoxContextMenu.Show(hitRegion);
                            }
                            else
                            {
                                CreateNodeContextMenu.ShowAsPopup();
                            }
                        }
                    }
                }
                else
                {
                    _rightDrag = false;
                }

                p_event.Use();
                
                EditorWindow.SetDirty(true);
            }
        }

        void ProcessDragging(Event p_event, Rect p_rect)
        {
            // Left drag
            if (p_event.button == 0 && p_event.alt && p_event.type == EventType.MouseDrag && dragging == DraggingType.NONE)
            {
                if (Graph != null)
                {
                    Graph.viewOffset += p_event.delta * Zoom;
                    
                    EditorWindow.SetDirty(true);
                }
            }
            
            // Right drag start
            if (p_event.button == 1 && p_event.type == EventType.MouseDown)
            {
                _rightDragStart = p_event.mousePosition;
            }

            // Right drag
            if (p_event.button == 1 && p_event.type == EventType.MouseDrag)
            {
                if (_rightDrag)
                {
                    Graph.viewOffset += p_event.delta * Zoom;
                } else if ((p_event.mousePosition - _rightDragStart).magnitude > 5)
                {
                    _rightDrag = true;
                    Graph.viewOffset += (p_event.mousePosition - _rightDragStart) * Zoom;
                }
                
                EditorWindow.SetDirty(true);
            }
        }

        void AddSelectedNode(int p_nodeIndex)
        {
            if (!SelectionManager.IsSelected(p_nodeIndex))
            {
                SelectionManager.AddNodeToSelection(p_nodeIndex);

                // If the controller is not null autoselect it in hierarchy TODO: maybe put this as setting
                if (Graph.Controller != null)
                {
                    Selection.activeGameObject = Graph.Controller.gameObject;
                }
            }
        }

        Rect FixRect(Rect p_rect)
        {
            Rect fixedRect = selectedRegion;
            if (fixedRect.width < 0)
            {
                fixedRect.x += fixedRect.width;
                fixedRect.width = -fixedRect.width;
            }
            if (fixedRect.height < 0)
            {
                fixedRect.y += fixedRect.height;
                fixedRect.height = -fixedRect.height;
            }

            return fixedRect;
        }
        
        // bool SelectedRegionContains(Rect p_rect, NodeBase p_node)
        // {
        //     if (p_rect.Contains(new Vector2((p_node.rect.x + Graph.viewOffset.x)/Zoom,
        //             (p_node.rect.y + Graph.viewOffset.y)/Zoom)) ||
        //         p_rect.Contains(new Vector2((p_node.rect.x + p_node.rect.width + Graph.viewOffset.x)/Zoom,
        //             (p_node.rect.y + p_node.rect.height + Graph.viewOffset.y)/Zoom)))
        //     {
        //         return true;
        //     }
        //     
        //     return false;
        // }
    }
}