using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class NpcQuadWalkBlackboard : NpcBlackboard
    {
        public virtual bool IsJumping => false;
        public virtual bool IsGliding => false;
        public virtual bool IsFalling => false;
        public virtual bool IsGuarding => false;
        public virtual float SpacingInDistance => 0f;
        public virtual float SpacingOutDistance => 0f;
        public virtual float MinSpacingDistance => 0f;
        public virtual float MaxSpacingDistance => 0f;
        public virtual float MinApproachDistance => 0f;
    }
}