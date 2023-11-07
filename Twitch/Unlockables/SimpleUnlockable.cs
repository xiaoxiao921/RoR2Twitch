using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using R2API;
using RoR2;
using UnityEngine;

namespace Twitch.Unlockables
{
    // Token: 0x02000023 RID: 35
    public class SimpleUnlockable : ModdedUnlockable
    {
        // Token: 0x1700000F RID: 15
        // (get) Token: 0x060000C2 RID: 194 RVA: 0x00010612 File Offset: 0x0000E812
        public override string AchievementIdentifier
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_ID";

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x060000C3 RID: 195 RVA: 0x0001061A File Offset: 0x0000E81A
        public override string UnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_REWARD_ID";

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x060000C4 RID: 196 RVA: 0x00010622 File Offset: 0x0000E822
        public override string PrerequisiteUnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_PREREQ_ID";

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x060000C5 RID: 197 RVA: 0x0001062A File Offset: 0x0000E82A
        public override string AchievementNameToken
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_NAME";

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x060000C6 RID: 198 RVA: 0x00010632 File Offset: 0x0000E832
        public override string AchievementDescToken
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_DESC";

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x060000C7 RID: 199 RVA: 0x0001063A File Offset: 0x0000E83A
        public override string UnlockableNameToken
        {
            get;
        } = "ROB_TWITCH_SIMPLEUNLOCKABLE_UNLOCKABLE_NAME";

        public override Sprite Sprite => Resources.Load<Sprite>("Textures/AchievementIcons/texPlaceholderAchievement");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_DESC")
                            });
        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_SIMPLEUNLOCKABLE_ACHIEVEMENT_DESC")
                            });

        // Token: 0x060000C9 RID: 201 RVA: 0x0001064C File Offset: 0x0000E84C
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("TwitchBody");
        }

        // Token: 0x060000CA RID: 202 RVA: 0x00010668 File Offset: 0x0000E868
        public void CheckExpunge(DamageReport report)
        {
            bool flag = report == null;
            if (!flag)
            {
                bool flag2 = report.victimBody == null;
                if (!flag2)
                {
                    bool flag3 = report.attackerBody == null;
                    if (!flag3)
                    {
                        bool flag4 = report.attackerBodyIndex == BodyCatalog.FindBodyIndex("TwitchBody") && base.meetsBodyRequirement;
                        if (flag4)
                        {
                            bool flag5 = report.victimBody.GetBuffCount(Twitch.expungedDebuff) >= 40;
                            if (flag5)
                            {
                                base.Grant();
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x060000CB RID: 203 RVA: 0x000106E5 File Offset: 0x0000E8E5
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.onServerDamageDealt += CheckExpunge;
        }

        // Token: 0x060000CC RID: 204 RVA: 0x00010701 File Offset: 0x0000E901
        public override void OnUninstall()
        {
            base.OnUninstall();
            GlobalEventManager.onServerDamageDealt -= CheckExpunge;
        }
    }
}
