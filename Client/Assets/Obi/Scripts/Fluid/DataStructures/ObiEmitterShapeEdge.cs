﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;

namespace Obi
{

    [AddComponentMenu("Physics/Obi/Emitter shapes/Edge", 853)]
	[ExecuteInEditMode]
	public class ObiEmitterShapeEdge : ObiEmitterShape
	{

		public float lenght = 0.5f;
		public float radialVelocity = 0;

		public void OnValidate(){
			lenght = Mathf.Max(0,lenght);
		}

		public override void GenerateDistribution(){

			distribution.Clear(); 

			float separation = particleSize + 0.01f;
		
			int amount = (int)(lenght / separation);
		
			for (int i = 0; i <= amount; ++i)
			{
				Vector3 pos = new Vector3(i*separation - lenght*0.5f,0,0);
				Vector3 vel = Quaternion.AngleAxis(i*radialVelocity,Vector3.right) * Vector3.forward;
                float velScale = 1;//1 - Mathf.Abs(i - amount * 0.5f) / (amount * 0.75f);

                distribution.Add(new EmitPoint(pos,vel * velScale));
			}

		}

	#if UNITY_EDITOR
		public void OnDrawGizmosSelected(){

			Handles.matrix = transform.localToWorldMatrix;
			Handles.color  = Color.cyan;

			Handles.DrawLine(-Vector3.right*lenght*0.5f,Vector3.right*lenght*0.5f);

			foreach (EmitPoint point in distribution)
                Handles.ArrowHandleCap(0, point.position, Quaternion.LookRotation(point.direction), 0.05f * point.direction.magnitude, EventType.Repaint);
        }
#endif

    }
}

