﻿/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using Dash.Attributes;
using UnityEngine;

namespace Dash
{
    [Help("Animate RectTransform position on current target.")]
    [Category(NodeCategoryType.ANIMATION)]
    [OutputCount(1)]
    [InputCount(1)]
    [Size(220,85)]
    [Serializable]
    public class AnimateAnchoredPositionNode : AnimationNodeBase<AnimateAnchoredPositionNodeModel>
    {
        protected override DashTween AnimateOnTarget(Transform p_target, NodeFlowData p_flowData)
        {
            RectTransform rectTransform = p_target.GetComponent<RectTransform>();
            
            if (CheckException(rectTransform, "No RectTransform component found on target"))
                return null;

            Vector2 fromPosition = GetParameterValue<Vector2>(Model.fromPosition, p_flowData);

            Vector2 startPosition = Model.useFrom 
                ? Model.isFromRelative 
                    ? rectTransform.anchoredPosition + fromPosition 
                    : fromPosition 
                : rectTransform.anchoredPosition;

            if (Model.storeToAttribute)
            {
                string attribute = GetParameterValue(Model.storeAttributeName, p_flowData);
                p_flowData.SetAttribute<Vector2>(attribute, rectTransform.anchoredPosition);
            }

            Vector2 finalPosition = GetParameterValue<Vector2>(Model.toPosition, p_flowData);
            EaseType easeType = GetParameterValue(Model.easeType, p_flowData);
            
            float time = GetParameterValue(Model.time, p_flowData);
            float delay = GetParameterValue(Model.delay, p_flowData);
            if (time == 0)
            {
                UpdateTween(rectTransform, 1, p_flowData, startPosition, finalPosition, easeType);
                return null;
            }
            
            return DashTween.To(rectTransform, 0, 1, time).SetDelay(delay)
                .OnUpdate(f => UpdateTween(rectTransform, f, p_flowData, startPosition, finalPosition, easeType));
        }

        protected void UpdateTween(RectTransform p_target, float p_delta, NodeFlowData p_flowData, Vector2 p_startPosition, Vector2 p_finalPosition, EaseType p_easeType)
        {
            if (p_target == null)
                return;

            if (Model.isToRelative)
            {
                p_target.anchoredPosition =
                    p_startPosition + new Vector2(DashTween.EaseValue(0, p_finalPosition.x, p_delta, p_easeType),
                        DashTween.EaseValue(0, p_finalPosition.y, p_delta, p_easeType));
            }
            else
            {
                p_target.anchoredPosition = new Vector2(DashTween.EaseValue(p_startPosition.x, p_finalPosition.x, p_delta, p_easeType),
                    DashTween.EaseValue(p_startPosition.y, p_finalPosition.y, p_delta, p_easeType));
            }
        }
        
#if UNITY_EDITOR
        protected override void GetCustomContextMenu(ref RuntimeGenericMenu p_menu)
        {
            p_menu.AddSeparator("");

            Transform target = ResolveEditorTarget();
            if (target != null)
            {
                p_menu.AddItem(new GUIContent("Bind TO from target"), false, BindTargetTo, target);
                p_menu.AddItem(new GUIContent("Bind FROM from target"), false, BindTargetFrom, target);
            }
        }

        protected void BindTargetTo(object p_target)
        {
            Model.toPosition.isExpression = false;
            Model.isToRelative = false;
            Model.toPosition.SetValue(((RectTransform)p_target).anchoredPosition);
        }
        
        protected void BindTargetFrom(object p_target)
        {
            Model.useFrom = true;
            Model.fromPosition.isExpression = false;
            Model.isFromRelative = false;
            Model.fromPosition.SetValue(((RectTransform)p_target).anchoredPosition);
        }
        
        public override void DrawInspectorControls(Rect p_rect)
        {
            Transform target = ResolveEditorTarget();
            
            if (target == null)
                return;
            
            GUIStyle style = new GUIStyle();
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            style.normal.background = Texture2D.whiteTexture;
            GUI.Box(new Rect(p_rect.x, p_rect.y + p_rect.height + 4, 390, 36), "", style);
            GUI.backgroundColor = Color.white;
            
            GUILayout.BeginArea(new Rect(p_rect.x, p_rect.y + p_rect.height + 8, p_rect.width, 28));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("BIND TARGET FROM", GUILayout.Width(140), GUILayout.Height(28)))
            {
                BindTargetFrom(target);
            }
            
            if (GUILayout.Button("BIND TARGET TO", GUILayout.Width(140), GUILayout.Height(28)))
            {
                BindTargetTo(target);
            }

            GUI.backgroundColor = new Color(1, .75f, .5f);
            if (GUILayout.Button("PREVIEW", GUILayout.Width(80), GUILayout.Height(28)))
            {
                TransformStorageData data = new TransformStorageData(target, TransformStorageOption.POSITION);
                AnimateOnTarget(target, NodeFlowDataFactory.Create()).OnComplete(() =>
                {
                    DashTweenCore.Uninitialize();
                    data.Restore(target);
                }).Start();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
#endif
    }
}
