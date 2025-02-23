﻿using UnityEngine;

namespace ECM2.Walkthrough.Ex51
{
    /// <summary>
    /// This example shows how to handle Character events when extending a Character through inheritance.
    /// </summary>
    
    public class PlayerCharacter : Character
    {
        protected override void OnCollided(ref CollisionResult collisionResult)
        {
            // Call base method implementation
            
            base.OnCollided(ref collisionResult);
            
            // Add your code here...
            
            Debug.Log($"Collided with {collisionResult.collider.name}");
        }

        protected override void OnFoundGround(ref FindGroundResult foundGround)
        {
            // Call base method implementation
            
            base.OnFoundGround(ref foundGround);
            
            // Add your code here...
            
            Debug.Log($"Found {foundGround.collider.name} ground");
        }

        protected override void OnLanded(Vector3 landingVelocity)
        {
            // Call base method implementation
            
            base.OnLanded(landingVelocity);
            
            // Add your code here...
            
            Debug.Log($"Landed with {landingVelocity:F4} landing velocity.");
        }

        protected override void OnCrouched()
        {
            // Call base method implementation
            
            base.OnCrouched();
            
            // Add your code here...
            
            Debug.Log("Crouched");
        }

        protected override void OnUnCrouched()
        {
            // Call base method implementation
            
            base.OnUnCrouched();
            
            // Add your code here...
            
            Debug.Log("UnCrouched");
        }

        protected override void OnJumped()
        {
            // Call base method implementation
            
            base.OnJumped();
            
            // Add your code here...
            
            Debug.Log("Jumped!");
            
            // Enable apex notification event
            
            notifyJumpApex = true;
        }

        protected override void OnReachedJumpApex()
        {
            // Call base method implementation
            
            base.OnReachedJumpApex();
            
            // Add your code here...
            
            Debug.Log($"Apex reached {GetVelocity():F4}");
        }
    }
}