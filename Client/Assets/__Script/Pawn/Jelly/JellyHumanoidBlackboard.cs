using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class JellyHumanoidBlackboard : JellyBlackboard
    {
        public virtual bool IsGuarding => false;
        public virtual float SpacingInDistance => 0f;
        public virtual float SpacingOutDistance => 0f;
        public virtual float MinSpacingDistance => 0f;
        public virtual float MaxSpacingDistance => 0f;
        public virtual float MinApproachDistance => 0f;
    }
}