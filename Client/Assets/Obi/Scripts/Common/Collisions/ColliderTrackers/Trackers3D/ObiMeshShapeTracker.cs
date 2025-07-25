﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Obi{

	public class ObiMeshShapeTracker : ObiShapeTracker
	{
        ObiTriangleMeshHandle handle;

        public Mesh targetMesh
        {
            get {
                var mc = collider as MeshCollider;
                return mc?.sharedMesh;
            }
        }

		public ObiMeshShapeTracker(ObiCollider source, MeshCollider collider){

            this.source = source;
			this.collider = collider;
		}

        /**
		 * Forces the tracker to update mesh data during the next call to UpdateIfNeeded().
		 */
        public void UpdateMeshData()
        {
            ObiColliderWorld.GetInstance().DestroyTriangleMesh(handle);
        }
	
		public override void UpdateIfNeeded ()
        {

            MeshCollider meshCollider = collider as MeshCollider;

            // retrieve collision world and index:
            var world = ObiColliderWorld.GetInstance();
            int index = source.Handle.index;

            // decrease reference count of current handle if the mesh data it points to is different
            // than the mesh used by the collider:
            if (handle != null && handle.owner != meshCollider.sharedMesh)
            {
                if (handle.Dereference())
                    world.DestroyTriangleMesh(handle);
            }

            // get or create the mesh:
            if (handle == null || !handle.isValid)
            {
                handle = world.GetOrCreateTriangleMesh(meshCollider.sharedMesh);
                handle.Reference();
            }

            // update collider:
            var shape = world.colliderShapes[index];
            shape.type = ColliderShape.ShapeType.TriangleMesh;
            shape.filter = source.Filter;
            shape.SetSign(source.Inverted);
            shape.isTrigger = meshCollider.isTrigger;
            shape.rigidbodyIndex = source.Rigidbody != null ? source.Rigidbody.Handle.index : -1;
            shape.materialIndex = source.CollisionMaterial != null ? source.CollisionMaterial.handle.index : -1;
            shape.forceZoneIndex = source.ForceZone != null ? source.ForceZone.Handle.index : -1;
            shape.contactOffset = source.Thickness;
            shape.dataIndex = handle.index;
            world.colliderShapes[index] = shape;

            // update bounds:
            var aabb = world.colliderAabbs[index];
            aabb.FromBounds(meshCollider.bounds, shape.contactOffset);
            world.colliderAabbs[index] = aabb;

            // update transform:
            var trfm = world.colliderTransforms[index];
            trfm.FromTransform3D(meshCollider.transform, source.Rigidbody as ObiRigidbody);
            world.colliderTransforms[index] = trfm;
        }

		public override void Destroy()
        {
			base.Destroy();

            if (handle != null && handle.Dereference())
                ObiColliderWorld.GetInstance().DestroyTriangleMesh(handle);
		}
	}
}

