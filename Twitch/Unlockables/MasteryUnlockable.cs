using System;
using R2API;
using RoR2;
using UnityEngine;

namespace Twitch.Unlockables
{
    public class MasteryUnlockable : ModdedUnlockable
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x060000AA RID: 170 RVA: 0x00010336 File Offset: 0x0000E536
        public override string AchievementIdentifier
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_ID";

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x060000AB RID: 171 RVA: 0x0001033E File Offset: 0x0000E53E
        public override string UnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_REWARD_ID";

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x060000AC RID: 172 RVA: 0x00010346 File Offset: 0x0000E546
        public override string PrerequisiteUnlockableIdentifier
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_PREREQ_ID";

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x060000AD RID: 173 RVA: 0x0001034E File Offset: 0x0000E54E
        public override string AchievementNameToken
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME";

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x060000AE RID: 174 RVA: 0x00010356 File Offset: 0x0000E556
        public override string AchievementDescToken
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC";

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x060000AF RID: 175 RVA: 0x0001035E File Offset: 0x0000E55E
        public override string UnlockableNameToken
        {
            get;
        } = "ROB_TWITCH_MASTERYUNLOCKABLE_UNLOCKABLE_NAME";

        public override Sprite Sprite => Resources.Load<Sprite>("Textures/AchievementIcons/texPlaceholderAchievement");

        public override Func<string> GetHowToUnlock { get; } = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC")
                            });
        public override Func<string> GetUnlocked { get; } = () => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME"),
                                Language.GetString("ROB_TWITCH_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC")
                            });

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("TwitchBody");
        }

        // Token: 0x060000B2 RID: 178 RVA: 0x0001038C File Offset: 0x0000E58C
        public void ClearCheck(Run run, RunReport runReport)
        {
            bool flag = run == null;
            if (!flag)
            {
                bool flag2 = runReport == null;
                if (!flag2)
                {
                    bool flag3 = !runReport.gameEnding;
                    if (!flag3)
                    {
                        bool isWin = runReport.gameEnding.isWin;
                        if (isWin)
                        {
                            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(runReport.ruleBook.FindDifficulty());
                            bool flag4 = difficultyDef != null && difficultyDef.countsAsHardMode;
                            if (flag4)
                            {
                                bool meetsBodyRequirement = base.meetsBodyRequirement;
                                if (meetsBodyRequirement)
                                {
                                    base.Grant();
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();
            Run.onClientGameOverGlobal += ClearCheck;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }
    }
}
