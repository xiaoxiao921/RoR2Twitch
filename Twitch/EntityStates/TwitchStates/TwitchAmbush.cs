using System;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TwitchStates
{
    // Token: 0x0200000C RID: 12
    public class TwitchAmbush : BaseState
    {
        // Token: 0x06000043 RID: 67 RVA: 0x0000410C File Offset: 0x0000230C
        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            modelTransform = base.GetModelTransform();
            twitchController = base.GetComponent<TwitchController>();
            CastSmoke();
            bool flag = base.characterBody && NetworkServer.active;
            if (flag)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
            }
            bool flag2 = twitchController;
            if (flag2)
            {
                twitchController.EnterStealth();
            }
            bool flag3 = base.skillLocator;
            if (flag3)
            {
                base.skillLocator.utility.SetSkillOverride(base.skillLocator.utility, Twitch.Twitch.ambushActiveDef, GenericSkill.SkillOverridePriority.Replacement);
            }
        }

        // Token: 0x06000044 RID: 68 RVA: 0x000041D0 File Offset: 0x000023D0
        public override void OnExit()
        {
            bool flag = base.characterBody && NetworkServer.active;
            if (flag)
            {
                bool flag2 = base.characterBody.HasBuff(RoR2Content.Buffs.Cloak);
                if (flag2)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                }
                bool flag3 = base.characterBody.HasBuff(RoR2Content.Buffs.CloakSpeed);
                if (flag3)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                }
                base.characterBody.AddTimedBuff(Twitch.Twitch.ambushBuff, 5f);
                base.characterBody.RecalculateStats();
                bool flag4 = modelTransform;
                if (flag4)
                {
                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = 5f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 7.5f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matPoisoned");
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                }
            }
            bool flag5 = twitchController;
            if (flag5)
            {
                twitchController.GetAmbushBuff(5f);
            }
            bool flag6 = !outer.destroying;
            if (flag6)
            {
                CastSmoke();
            }
            bool flag7 = TwitchAmbush.destealthMaterial;
            if (flag7)
            {
                var temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(animator.gameObject);
                temporaryOverlay2.duration = 1f;
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = TwitchAmbush.destealthMaterial;
                temporaryOverlay2.inspectorCharacterModel = animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.animateShaderAlpha = true;
            }
            bool flag8 = twitchController;
            if (flag8)
            {
                twitchController.ExitStealth();
            }
            bool flag9 = base.skillLocator;
            if (flag9)
            {
                base.skillLocator.utility.UnsetSkillOverride(base.skillLocator.utility, Twitch.Twitch.ambushActiveDef, GenericSkill.SkillOverridePriority.Replacement);
            }
            base.OnExit();
        }

        // Token: 0x06000045 RID: 69 RVA: 0x000043FC File Offset: 0x000025FC
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            bool flag = stopwatch >= TwitchAmbush.duration && base.isAuthority;
            if (flag)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000046 RID: 70 RVA: 0x0000444C File Offset: 0x0000264C
        private void CastSmoke()
        {
            bool flag = !hasCastSmoke;
            if (flag)
            {
                Util.PlaySound(Sounds.TwitchEnterStealth, base.gameObject);
                hasCastSmoke = true;
                bool flag2 = animator;
                if (flag2)
                {
                    animator.SetBool("isSneaking", true);
                }
            }
            else
            {
                Util.PlaySound(Sounds.TwitchExitStealth, base.gameObject);
                bool flag3 = animator;
                if (flag3)
                {
                    animator.SetBool("isSneaking", false);
                }
            }
            EffectManager.SpawnEffect(CastSmokescreenNoDelay.smokescreenEffectPrefab, new EffectData
            {
                origin = base.transform.position
            }, false);
        }

        // Token: 0x06000047 RID: 71 RVA: 0x000044FC File Offset: 0x000026FC
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            bool flag = stopwatch <= TwitchAmbush.minimumStateDuration;
            InterruptPriority result;
            if (flag)
            {
                result = InterruptPriority.PrioritySkill;
            }
            else
            {
                result = InterruptPriority.Any;
            }
            return result;
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00004528 File Offset: 0x00002728
        public TwitchAmbush()
        {
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00004531 File Offset: 0x00002731
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchAmbush()
        {
        }

        // Token: 0x0400005E RID: 94
        public static float duration = 6f;

        // Token: 0x0400005F RID: 95
        public static float minimumStateDuration = 0.25f;

        // Token: 0x04000060 RID: 96
        public static string startCloakSoundString;

        // Token: 0x04000061 RID: 97
        public static string stopCloakSoundString;

        // Token: 0x04000062 RID: 98
        public static GameObject smokescreenEffectPrefab;

        // Token: 0x04000063 RID: 99
        public static Material destealthMaterial;

        // Token: 0x04000064 RID: 100
        private float stopwatch;

        // Token: 0x04000065 RID: 101
        private bool hasCastSmoke;

        // Token: 0x04000066 RID: 102
        private Animator animator;

        // Token: 0x04000067 RID: 103
        private Transform modelTransform;

        // Token: 0x04000068 RID: 104
        private TwitchController twitchController;
    }
}
