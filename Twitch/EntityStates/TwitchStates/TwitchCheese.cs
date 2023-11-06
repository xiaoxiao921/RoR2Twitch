using System;
using RoR2;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TwitchStates
{
    // Token: 0x0200000B RID: 11
    public class TwitchCheese : BaseSkillState
    {
        // Token: 0x0600003D RID: 61 RVA: 0x00003FB4 File Offset: 0x000021B4
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration;
            fireDuration = duration;
            animator = base.GetModelAnimator();
            base.PlayAnimation("FullBody, Override", "Cheese");
            Util.PlaySound(Sounds.TwitchCheese, base.gameObject);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x0000400F File Offset: 0x0000220F
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x0600003F RID: 63 RVA: 0x0000401C File Offset: 0x0000221C
        private void EatCheese()
        {
            bool flag = !hasFired;
            if (flag)
            {
                hasFired = true;
                Util.PlaySound(Sounds.TwitchHeal, base.gameObject);
                bool active = NetworkServer.active;
                if (active)
                {
                    base.characterBody.healthComponent.Heal(base.characterBody.healthComponent.fullHealth, default(ProcChainMask), true);
                }
            }
        }

        // Token: 0x06000040 RID: 64 RVA: 0x00004088 File Offset: 0x00002288
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.fixedAge >= fireDuration;
            if (flag)
            {
                EatCheese();
            }
            bool flag2 = base.fixedAge >= duration && base.isAuthority;
            if (flag2)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x000040E4 File Offset: 0x000022E4
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x06000042 RID: 66 RVA: 0x000040F7 File Offset: 0x000022F7
        public TwitchCheese()
        {
        }

        // Token: 0x04000059 RID: 89
        public float baseDuration = 4f;

        // Token: 0x0400005A RID: 90
        private float duration;

        // Token: 0x0400005B RID: 91
        private float fireDuration;

        // Token: 0x0400005C RID: 92
        private bool hasFired;

        // Token: 0x0400005D RID: 93
        private Animator animator;
    }
}
