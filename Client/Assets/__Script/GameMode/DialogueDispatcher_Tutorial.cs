using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FinalFactory;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    // Tutorial
    public partial class DialogueDispatcher : DialogueViewBase
    {
        public void JumpTo(string text)
        {
            Debug.Log($"From {text}");
        }

        public void Sleep()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.GetComponent<SlayerAnimController>().PlaySingleClipLooping(slayerBrain.BB.dialogue.sleepAnimClip, 0.1f);
        }

        public void GetUp()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.GetComponent<SlayerAnimController>().PlaySingleClip(slayerBrain.BB.dialogue.getUpAnimClip, 0.5f, 0.5f);
        }

        public void ShowMechArm()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PartsCtrler.SetLeftMechArmHidden(false);
        }

        public void HideMechArm()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PartsCtrler.SetLeftMechArmHidden(true);
        }

        public void ShowSword()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PartsCtrler.SetSwordHidden(false);
        }

        public void HideSword()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PartsCtrler.SetSwordHidden(true);
        }

        public void PlaySound(string soundName) 
        {
            Debug.Log("YarnCommand PlaySound: " + soundName);
        }
    }
}