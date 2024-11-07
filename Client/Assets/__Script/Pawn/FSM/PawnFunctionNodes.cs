using System;
using Game;
using UniRx;
using UnityEngine;

namespace FlowCanvas.Nodes
{

	
	// /// <summary>
	// /// 
	// /// </summary>
	// public class CheckPawnPossessed : PureFunctionNode<bool, GameObject>{
	// 	public override bool Invoke(GameObject pawn)
	// 	{
	// 		return pawn.GetComponent<Game.PawnBrainController>()?.ownerCtrler != null;
	// 	}
	// }

	// /// <summary>
	// /// 
	// /// </summary>
	// public class GetForwardToVector2D : PureFunctionNode<Vector3, Transform, Transform>{
	// 	public override Vector3 Invoke(Transform from, Transform to)
	// 	{
	// 		return (to.position - from.position).Vector2D().normalized;
	// 	}
	// }
}