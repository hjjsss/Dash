/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Dash
{
    public interface IInternalGraphAccess
    {
        DashGraph ParentGraph { set; }
        
        // void AddActiveTween(DashTween p_tween);
        //
        // void RemoveActiveTween(DashTween p_tween);
        //
        // void StopActiveTweens(System.Object p_target);

        void OutputExecuted(OutputNode p_node, NodeFlowData p_data);
        
        void SetVersion(int p_version);
    }
}