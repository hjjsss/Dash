﻿/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dash.Attributes;
using OdinSerializer;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dash
{
    [AddComponentMenu("Dash/Dash Controller")]
    public class DashController : MonoBehaviour, IEditorControllerAccess, IExposedPropertyTable
    {

        public DashCore Core => DashCore.Instance;

        [HideInInspector]
        [SerializeField]
        protected DashGraph _assetGraph;

        [HideInInspector]
        [SerializeField]
        private byte[] _boundGraphData;

        [HideInInspector]
        [SerializeField]
        private int _selfReferenceIndex = -1;

        [HideInInspector]
        [SerializeField]
        private List<Object> _boundGraphReferences;

        DashGraph IEditorControllerAccess.graphAsset
        {
            get { return _assetGraph; }
            set
            {
                _assetGraph = value;
                if (_assetGraph != null)
                {
                    _boundGraphData = null;
                    _boundGraphReferences = null;
                }
            }
        }

        [NonSerialized]
        private DashGraph _graphInstance;

        public DashGraph Graph => GetGraphInstance();

        // public string Id = "DC";

        public bool IsGraphBound => _boundGraphData?.Length > 0;

        private DashGraph GetGraphInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !DashEditorCore.Previewer.IsPreviewing && _assetGraph != null)
            {
                return _assetGraph;
            }
#endif
            if (_graphInstance == null)
            {
                if (_boundGraphData?.Length > 0)
                {
                    InstanceBoundGraph();
                }
                else
                {
                    InstanceAssetGraph();
                }
            }

            return _graphInstance;
        }

        void InstanceBoundGraph()
        {
            _graphInstance = ScriptableObject.CreateInstance<DashGraph>();

            // Empty graphs don't self reference
            if (_selfReferenceIndex != -1)
            {
                _boundGraphReferences[_selfReferenceIndex] = _graphInstance;
            }

            _graphInstance.DeserializeFromBytes(_boundGraphData, DataFormat.Binary, ref _boundGraphReferences);
            _graphInstance.name = "Bound";
        }

        void InstanceAssetGraph()
        {
            if (_assetGraph == null)
                return;

            _graphInstance = _assetGraph.Clone();
        }
        
        public bool autoStart = false;

        [Dependency("autoStart", true)] 
        public string autoStartInput = "StartInput";
        
        public bool autoOnEnable = false;

        [Dependency("autoEnabled", true)]
        public string autoOnEnableInput = "OnEnableInput";
        
        private event Action UpdateCallback;

        void Awake()
        {
            if (Graph != null) 
                Graph.Initialize(this);

            Core.Bind(this);
        }

        void Start()
        {
            if (Graph == null)
                return;

            if (autoStart)
            {
#if UNITY_EDITOR
                DashEditorDebug.Debug(new ControllerDebugItem(ControllerDebugItem.ControllerDebugItemType.START, this));
#endif
                NodeFlowData data = NodeFlowDataFactory.Create(transform);
                Graph.ExecuteGraphInput(autoStartInput, data);
            }
        }

        private void OnEnable()
        {
            if (Graph == null)
                return;
            
            if (autoOnEnable)
            {
#if UNITY_EDITOR
                DashEditorDebug.Debug(new ControllerDebugItem(ControllerDebugItem.ControllerDebugItemType.ONENABLE, this));
#endif
                NodeFlowData data = NodeFlowDataFactory.Create(transform);
                Graph.ExecuteGraphInput(autoOnEnableInput, data);
            }
        }

        public void RegisterUpdateCallback(Action p_callback)
        {
            UpdateCallback += p_callback;
        }

        public void UnregisterUpdateCallback(Action p_callback)
        {
            UpdateCallback -= p_callback;
        }

        void Update()
        {
            UpdateCallback?.Invoke();
        }

        public void SendEvent(string p_name)
        {
            if (Graph == null)
                return;

            Graph.SendEvent(p_name, transform);
        }

        public void SendEvent(string p_name, NodeFlowData p_flowData)
        {
            if (Graph == null || transform == null)
                return;

            p_flowData = p_flowData == null ? NodeFlowDataFactory.Create(transform) : p_flowData.Clone();

            if (!p_flowData.HasAttribute(NodeFlowDataReservedAttributes.TARGET))
            {
                p_flowData.SetAttribute(NodeFlowDataReservedAttributes.TARGET, transform);
            }
            
            p_flowData.SetAttribute(NodeFlowDataReservedAttributes.EVENT, p_name);

            Graph.SendEvent(p_name, p_flowData);
        }

        public void AddListener(string p_name, Action<NodeFlowData> p_callback) =>
            Graph?.AddListener(p_name, p_callback);

        public void RemoveListener(string p_name, Action<NodeFlowData> p_callback) =>
            Graph?.RemoveListener(p_name, p_callback);

        public void SetListener(string p_name, Action<NodeFlowData> p_callback) =>
            Graph?.SetListener(p_name, p_callback);

        private void OnDestroy()
        {
            Core.Unbind(this);
            
            if (Graph != null) {
                Graph.Stop();
            }
        }

#if UNITY_EDITOR
        [HideInInspector]
        public bool previewing = false;
        public string graphPath = "";

        public void ReserializeBound()
        {
            if (_graphInstance != null)
            {
                _boundGraphData = _graphInstance.SerializeToBytes(DataFormat.Binary, ref _boundGraphReferences);
                _selfReferenceIndex = _boundGraphReferences.FindIndex(r => r == _graphInstance);
            }
        }
#endif

        // Handle Unity property exposing - may be removed later to avoid external references

        #region EXPOSE_TABLE

        [HideInInspector] 
        [SerializeField]
        protected List<PropertyName> _propertyNames = new List<PropertyName>();

        [HideInInspector]
        [SerializeField]
        protected List<Object> _references = new List<Object>();

        public void CleanupReferences(List<string> p_existingGUIDs)
        {
            for (int i = 0; i < _propertyNames.Count; i++)
            {
                if (p_existingGUIDs.Contains(_propertyNames[i].ToString()))
                    continue;

                _propertyNames.RemoveAt(i);
                _references.RemoveAt(i);
                i--;
            }
        }

        public void ClearReferenceValue(PropertyName id)
        {
            int index = _propertyNames.IndexOf(id);
            if (index != -1)
            {
                _references.RemoveAt(index);
                _propertyNames.RemoveAt(index);
            }
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            int index = _propertyNames.IndexOf(id);
            if (index != -1)
            {
                idValid = true;
                return _references[index];
            }

            idValid = false;
            return null;
        }

        public void SetReferenceValue(PropertyName id, Object value)
        {
            int index = _propertyNames.IndexOf(id);
            if (index != -1)
            {
                _references[index] = value;
            }
            else
            {
                _propertyNames.Add(id);
                _references.Add(value);
            }
        }

        public void BindGraph(DashGraph p_graph)
        {
            _assetGraph = null;
            _graphInstance = null;
            _boundGraphData = null;
            _boundGraphReferences = null;

            if (p_graph != null)
            {
                DashGraph graph = p_graph.Clone();
                _boundGraphData = graph.SerializeToBytes(DataFormat.Binary, ref _boundGraphReferences);
                _selfReferenceIndex = _boundGraphReferences.FindIndex(r => r == graph);
            }
        }

        public DashGraph GetGraphAtPath(string p_path)
        {
            if (string.IsNullOrWhiteSpace(p_path))
                return Graph;

            List<string> split = p_path.Split('/').ToList();
            DashGraph currentGraph = Graph;
            foreach (string id in split)
            {
                SubGraphNode node = currentGraph.GetNodeById(id) as SubGraphNode;
                if (node == null)
                {
                    Debug.LogWarning("Cannot retrieve subgraph at invalid graph path " + p_path);
                    return null;
                }

                currentGraph = node.GetSubGraphInstance();
            }

            return currentGraph;
        }

        #endregion
    }
}