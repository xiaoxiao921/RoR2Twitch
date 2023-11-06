using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using R2API;
using RoR2;
using UnityEngine;

namespace Twitch.Unlockables
{
    // Token: 0x02000022 RID: 34
    public class TarUnlockable : ModdedUnlockable
    {
        // Token: 0x17000008 RID: 8
        // (get) Token: 0x060000B6 RID: 182 RVA: 0x0001049E File Offset: 0x0000E69E
        public override string AchievementIdentifier
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_ID";

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x060000B7 RID: 183 RVA: 0x000104A6 File Offset: 0x0000E6A6
        public override string UnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_REWARD_ID";

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x060000B8 RID: 184 RVA: 0x000104AE File Offset: 0x0000E6AE
        public override string PrerequisiteUnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_PREREQ_ID";

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x060000B9 RID: 185 RVA: 0x000104B6 File Offset: 0x0000E6B6
        public override string AchievementNameToken
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_NAME";

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x060000BA RID: 186 RVA: 0x000104BE File Offset: 0x0000E6BE
        public override string AchievementDescToken
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_DESC";

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x060000BB RID: 187 RVA: 0x000104C6 File Offset: 0x0000E6C6
        public override string UnlockableNameToken
        {
            get;
        } = "ROB_TWITCH_TARUNLOCKABLE_UNLOCKABLE_NAME";

        public override Sprite Sprite => Resources.Load<Sprite>("");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_DESC")
                            });
        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_TARUNLOCKABLE_ACHIEVEMENT_DESC")
                            });

        // Token: 0x060000BD RID: 189 RVA: 0x000104D8 File Offset: 0x0000E6D8
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("TwitchBody");
        }

        // Token: 0x060000BE RID: 190 RVA: 0x000104F4 File Offset: 0x0000E6F4
        public void CheckDeath(DamageReport report)
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
                        bool flag4 = report.victimBodyIndex == BodyCatalog.FindBodyIndex("TwitchBody") && base.meetsBodyRequirement;
                        if (flag4)
                        {
                            bool flag5 = BodyCatalog.FindBodyIndex(report.attackerBody) == BodyCatalog.FindBodyIndex("ClayBossBody");
                            if (flag5)
                            {
                                base.Grant();
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x060000BF RID: 191 RVA: 0x00010571 File Offset: 0x0000E771
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.onCharacterDeathGlobal += CheckDeath;
        }

        // Token: 0x060000C0 RID: 192 RVA: 0x0001058D File Offset: 0x0000E78D
        public override void OnUninstall()
        {
            base.OnUninstall();
            GlobalEventManager.onCharacterDeathGlobal -= CheckDeath;
        }
    }
}
