using System;
using RoR2;
using RoR2.Projectile;
using Twitch;
using UnityEngine;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000008 RID: 8
    public class TwitchThrowBomb : BaseSkillState
    {
        // Token: 0x06000029 RID: 41 RVA: 0x00003864 File Offset: 0x00001A64
        public override void OnEnter()
        {
            base.OnEnter();
            duration = TwitchThrowBomb.baseDuration / attackSpeedStat;
            fireDuration = 0.35f * duration;
            base.characterBody.SetAimTimer(2f);
            animator = base.GetModelAnimator();
            base.PlayAnimation("FullBody, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);
            Util.PlayAttackSpeedSound(Sounds.TwitchCaskStart, base.gameObject, attackSpeedStat);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000038ED File Offset: 0x00001AED
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x0600002B RID: 43 RVA: 0x000038F8 File Offset: 0x00001AF8
        private void ThrowBomb()
        {
            bool flag = !hasFired;
            if (flag)
            {
                hasFired = true;
                Util.PlaySound(Sounds.TwitchThrowCask, base.gameObject);
                base.characterBody.AddSpreadBloom(1f);
                Ray aimRay = base.GetAimRay();
                bool isAuthority = base.isAuthority;
                if (isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(Twitch.Twitch.caskProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, TwitchThrowBomb.damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        // Token: 0x0600002C RID: 44 RVA: 0x000039AC File Offset: 0x00001BAC
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.characterMotor;
            if (flag)
            {
                bool flag2 = base.characterMotor.velocity.y < 0f && !hasFired;
                if (flag2)
                {
                    base.characterMotor.velocity.y = 0f;
                }
            }
            bool flag3 = base.fixedAge >= fireDuration;
            if (flag3)
            {
                ThrowBomb();
            }
            bool flag4 = base.fixedAge >= duration && base.isAuthority;
            if (flag4)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00003A58 File Offset: 0x00001C58
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003A6B File Offset: 0x00001C6B
        public TwitchThrowBomb()
        {
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003A74 File Offset: 0x00001C74
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchThrowBomb()
        {
        }

        // Token: 0x04000045 RID: 69
        public static float damageCoefficient = 3f;

        // Token: 0x04000046 RID: 70
        public static float baseDuration = 1.1f;

        // Token: 0x04000047 RID: 71
        private float duration;

        // Token: 0x04000048 RID: 72
        private float fireDuration;

        // Token: 0x04000049 RID: 73
        private bool hasFired;

        // Token: 0x0400004A RID: 74
        private Animator animator;
    }
}
