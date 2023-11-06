using System;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000007 RID: 7
    public class TwitchFireBazooka : BaseSkillState
    {
        // Token: 0x06000023 RID: 35 RVA: 0x000037D5 File Offset: 0x000019D5
        public override void OnEnter()
        {
            base.OnEnter();
            duration = TwitchFireBazooka.baseDuration / attackSpeedStat;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000037F1 File Offset: 0x000019F1
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000037FC File Offset: 0x000019FC
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.fixedAge >= duration && base.isAuthority;
            if (flag)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000026 RID: 38 RVA: 0x0000383C File Offset: 0x00001A3C
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x0000384F File Offset: 0x00001A4F
        public TwitchFireBazooka()
        {
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003858 File Offset: 0x00001A58
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchFireBazooka()
        {
        }

        // Token: 0x04000043 RID: 67
        public static float baseDuration = 0.4f;

        // Token: 0x04000044 RID: 68
        private float duration;
    }
}
