using System;
using EntityStates.Commando;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TwitchStates
{
    // Token: 0x0200000D RID: 13
    public class TwitchScurry : BaseState
    {
        // Token: 0x0600004A RID: 74 RVA: 0x00004548 File Offset: 0x00002748
        public override void OnEnter()
        {
            base.OnEnter();
            modelTransform = base.GetModelTransform();
            twitchController = base.GetComponent<TwitchController>();
            bool flag = modelTransform;
            if (flag)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
            }
            animator = base.GetModelAnimator();
            ChildLocator component = animator.GetComponent<ChildLocator>();
            bool flag2 = base.isAuthority && base.inputBank && base.characterDirection;
            if (flag2)
            {
                forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            }
            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : forwardDirection;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
            float value = Vector3.Dot(forwardDirection, rhs);
            float value2 = Vector3.Dot(forwardDirection, rhs2);
            animator.SetFloat("forwardSpeed", value, 0.1f, Time.fixedDeltaTime);
            animator.SetFloat("rightSpeed", value2, 0.1f, Time.fixedDeltaTime);
            bool flag3 = base.characterBody && NetworkServer.active;
            if (flag3)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
            }
            bool flag4 = twitchController;
            if (flag4)
            {
                twitchController.EnterStealth();
            }
            CastSmoke();
            RecalculateSpeed();
            bool flag5 = base.characterMotor && base.characterDirection;
            if (flag5)
            {
                CharacterMotor characterMotor = base.characterMotor;
                characterMotor.velocity.y = characterMotor.velocity.y * 0.2f;
                base.characterMotor.velocity = forwardDirection * rollSpeed;
            }
            base.PlayAnimation("FullBody, Override", "Scurry");
            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            previousPosition = base.transform.position - b;
        }

        // Token: 0x0600004B RID: 75 RVA: 0x0000479F File Offset: 0x0000299F
        private void RecalculateSpeed()
        {
            rollSpeed = (2f + 0.5f * moveSpeedStat) * Mathf.Lerp(TwitchScurry.initialSpeedCoefficient, TwitchScurry.finalSpeedCoefficient, base.fixedAge / duration);
        }

        // Token: 0x0600004C RID: 76 RVA: 0x000047D8 File Offset: 0x000029D8
        private void CastSmoke()
        {
            bool flag = !hasCastSmoke;
            if (flag)
            {
                Util.PlaySound(Sounds.TwitchEnterStealth, base.gameObject);
                hasCastSmoke = true;
            }
            else
            {
                Util.PlaySound(Sounds.TwitchExitStealth, base.gameObject);
            }
            EffectManager.SpawnEffect(CastSmokescreenNoDelay.smokescreenEffectPrefab, new EffectData
            {
                origin = base.transform.position
            }, false);
        }

        // Token: 0x0600004D RID: 77 RVA: 0x00004848 File Offset: 0x00002A48
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateSpeed();
            bool flag = base.cameraTargetParams;
            if (flag)
            {
                base.cameraTargetParams.fovOverride = Mathf.Lerp(DodgeState.dodgeFOV, 60f, base.fixedAge / duration);
            }
            Vector3 normalized = (base.transform.position - previousPosition).normalized;
            bool flag2 = base.characterMotor && base.characterDirection && normalized != Vector3.zero;
            if (flag2)
            {
                Vector3 vector = normalized * rollSpeed;
                float y = vector.y;
                vector.y = 0f;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                vector.y += Mathf.Max(y, 0f);
                base.characterMotor.velocity = vector;
            }
            previousPosition = base.transform.position;
            bool flag3 = base.fixedAge >= duration && base.isAuthority;
            if (flag3)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x0600004E RID: 78 RVA: 0x00004998 File Offset: 0x00002B98
        public override void OnExit()
        {
            bool flag = base.cameraTargetParams;
            if (flag)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }
            bool flag2 = base.characterBody && NetworkServer.active;
            if (flag2)
            {
                bool flag3 = base.characterBody.HasBuff(RoR2Content.Buffs.Cloak);
                if (flag3)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                }
                bool flag4 = base.characterBody.HasBuff(RoR2Content.Buffs.CloakSpeed);
                if (flag4)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                }
                base.characterBody.AddTimedBuff(Twitch.Twitch.ambushBuff, 4f);
                base.characterBody.RecalculateStats();
                bool flag5 = modelTransform;
                if (flag5)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 4f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 2.5f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matPoisoned");
                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                }
            }
            bool flag6 = twitchController;
            if (flag6)
            {
                twitchController.GetAmbushBuff(5f);
            }
            bool flag7 = !outer.destroying;
            if (flag7)
            {
                CastSmoke();
            }
            bool flag8 = TwitchAmbush.destealthMaterial;
            if (flag8)
            {
                TemporaryOverlay temporaryOverlay2 = animator.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 1f;
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = TwitchAmbush.destealthMaterial;
                temporaryOverlay2.inspectorCharacterModel = animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.animateShaderAlpha = true;
            }
            bool flag9 = twitchController;
            if (flag9)
            {
                twitchController.ExitStealth();
            }
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();
        }

        // Token: 0x0600004F RID: 79 RVA: 0x00004BC1 File Offset: 0x00002DC1
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        // Token: 0x06000050 RID: 80 RVA: 0x00004BD9 File Offset: 0x00002DD9
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }

        // Token: 0x06000051 RID: 81 RVA: 0x00004BF0 File Offset: 0x00002DF0
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x06000052 RID: 82 RVA: 0x00004C03 File Offset: 0x00002E03
        public TwitchScurry()
        {
        }

        // Token: 0x06000053 RID: 83 RVA: 0x00004C17 File Offset: 0x00002E17
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchScurry()
        {
        }

        // Token: 0x04000069 RID: 105
        public float duration = 0.4f;

        // Token: 0x0400006A RID: 106
        public static GameObject dodgeEffect;

        // Token: 0x0400006B RID: 107
        public static float initialSpeedCoefficient = 5f;

        // Token: 0x0400006C RID: 108
        public static float finalSpeedCoefficient = 4f;

        // Token: 0x0400006D RID: 109
        private float rollSpeed;

        // Token: 0x0400006E RID: 110
        private bool hasCastSmoke;

        // Token: 0x0400006F RID: 111
        private Vector3 forwardDirection;

        // Token: 0x04000070 RID: 112
        private Animator animator;

        // Token: 0x04000071 RID: 113
        private Vector3 previousPosition;

        // Token: 0x04000072 RID: 114
        private Transform modelTransform;

        // Token: 0x04000073 RID: 115
        private CharacterModel characterModel;

        // Token: 0x04000074 RID: 116
        private TwitchController twitchController;
    }
}
