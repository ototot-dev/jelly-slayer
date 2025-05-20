using UniRx;
using UnityEngine;
using ZLinq;

namespace Game
{
    public class SlayerPartsController : MonoBehaviour
    {
        [Header("Body")]
        public Renderer bodyMeshRenderer;

        [Header("Weapon")]
        public Renderer swordMeshRenderer;

        [Header("MechArm")]
        public Renderer[] mechArmRenderers;
        public Transform leftMechHandAttachPoint;
        public Transform leftMechElbowAttachPoint;
        public Transform leftMechHandRagdoll;
        public Transform leftMechElbowRagdoll;
        public Transform leftMechHandBone;
        public Transform leftMechElbowBone;

        void Awake()
        {
            __brain = GetComponent<SlayerBrain>();
        }

        SlayerBrain __brain;

        void Start()
        {
            __brain.onLateUpdate += () =>
            {
                if (leftMechHandRagdoll == null)
                    leftMechHandRagdoll = __brain.AnimCtrler.ragdollAnimator.Handler.Dummy_Container.gameObject.DescendantsAndSelf().First(d => d.name == "Left wrist").transform;
                if (leftMechElbowRagdoll == null)
                    leftMechElbowRagdoll = __brain.AnimCtrler.ragdollAnimator.Handler.Dummy_Container.gameObject.DescendantsAndSelf().First(d => d.name == "Left elbow").transform;

                if (__brain.AnimCtrler.ragdollAnimator.Handler != null && __brain.AnimCtrler.ragdollAnimator.Handler.AnimatingMode == FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling)
                {
                    var leftMechHandBlendMatrix = leftMechHandAttachPoint.localToWorldMatrix.Lerp(leftMechHandRagdoll.localToWorldMatrix * Matrix4x4.TRS(leftMechHandAttachPoint.localPosition, leftMechHandAttachPoint.localRotation, Vector3.one), __brain.AnimCtrler.ragdollAnimator.RagdollBlend);
                    leftMechHandBone.transform.SetPositionAndRotation(leftMechHandBlendMatrix.GetPosition(), Quaternion.LookRotation(leftMechHandBlendMatrix.GetColumn(2), leftMechHandBlendMatrix.GetColumn(1)));

                    var leftMechElbowBlendMatrix = leftMechElbowAttachPoint.localToWorldMatrix.Lerp(leftMechElbowRagdoll.localToWorldMatrix * Matrix4x4.TRS(leftMechElbowAttachPoint.localPosition, leftMechElbowAttachPoint.localRotation, Vector3.one), __brain.AnimCtrler.ragdollAnimator.RagdollBlend);
                    leftMechElbowBone.transform.SetPositionAndRotation(leftMechElbowBlendMatrix.GetPosition(), Quaternion.LookRotation(leftMechElbowBlendMatrix.GetColumn(2), leftMechElbowBlendMatrix.GetColumn(1)));
                }
                else
                {
                    leftMechHandBone.transform.SetPositionAndRotation(leftMechHandAttachPoint.position, leftMechHandAttachPoint.rotation);
                    leftMechElbowBone.transform.SetPositionAndRotation(leftMechElbowAttachPoint.position, leftMechElbowAttachPoint.rotation);
                }
            };
        }

        public void SetSwordHidden(bool newHidden)
        {
            swordMeshRenderer.enabled = !newHidden;
        }

        public void SetLeftMechArmHidden(bool newHidden)
        {
            foreach (var r in mechArmRenderers)
                r.enabled = !newHidden;
        }
    }
}