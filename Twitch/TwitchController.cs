using System;
using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch
{
    // Token: 0x02000015 RID: 21
    public class TwitchController : MonoBehaviour
    {
        // Token: 0x06000082 RID: 130 RVA: 0x0000E8E8 File Offset: 0x0000CAE8
        private void Awake()
        {
            foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
            {
                bool flag = particleSystem.transform.parent.name == "StealthEffect";
                if (flag)
                {
                    stealthFX = particleSystem;
                }
                bool flag2 = particleSystem.transform.parent.name == "RevealEffect";
                if (flag2)
                {
                    revealFX = particleSystem;
                }
                bool flag3 = particleSystem.transform.parent.name == "AmbushEffect";
                if (flag3)
                {
                    ambushFX = particleSystem;
                }
                bool flag4 = particleSystem.transform.parent.name == "MiniAmbushEffect";
                if (flag4)
                {
                    miniAmbushFX = particleSystem;
                }
            }
        }

        // Token: 0x06000083 RID: 131 RVA: 0x0000E9B4 File Offset: 0x0000CBB4
        private void Start()
        {
            charBody = base.GetComponentInChildren<CharacterBody>();
            charMotor = base.GetComponentInChildren<CharacterMotor>();
            charHealth = base.GetComponentInChildren<HealthComponent>();
            childLocator = base.GetComponentInChildren<ChildLocator>();
            inputBank = base.GetComponentInChildren<InputBankTest>();
            bool flag = Twitch.how.Value && childLocator;
            if (flag)
            {
                childLocator.FindChild("Doge").gameObject.SetActive(true);
            }
            bool flag2 = charBody;
            if (flag2)
            {
                bool flag3 = childLocator;
                if (flag3)
                {
                    childLocator.FindChild("L_Shoulderpad").localScale = Vector3.zero;
                    childLocator.FindChild("L_Shoulderpad").name = "L_Shoulderpad1";
                    childLocator.FindChild("L_Glove").localScale = Vector3.zero;
                    childLocator.FindChild("L_Glove").name = "L_Glove1";
                }
            }
            CheckWeapon();
            SetWeaponDisplays(GetWeapon());
        }

        // Token: 0x06000084 RID: 132 RVA: 0x0000EADC File Offset: 0x0000CCDC
        public void RefundCooldown(float procCoefficient)
        {
            bool active = NetworkServer.active;
            if (active)
            {
                bool flag = charBody;
                if (flag)
                {
                    bool flag2 = charBody.skillLocator;
                    if (flag2)
                    {
                        bool flag3 = charBody.skillLocator.primary.skillDef.skillNameToken == "TWITCH_PRIMARY_CROSSBOW_NAME" && charBody.skillLocator.utility.skillDef.skillNameToken == "TWITCH_UTILITY_AMBUSH_NAME";
                        if (flag3)
                        {
                            charBody.skillLocator.utility.RunRecharge(0.5f * procCoefficient);
                        }
                    }
                }
            }
        }

        // Token: 0x06000085 RID: 133 RVA: 0x0000EB94 File Offset: 0x0000CD94
        private int GetWeapon()
        {
            bool flag = charBody && charBody.skillLocator;
            if (flag)
            {
                bool flag2 = charBody.skillLocator.primary.skillDef.skillNameToken == "TWITCH_PRIMARY_CROSSBOW_NAME";
                if (flag2)
                {
                    return 0;
                }
                bool flag3 = charBody.skillLocator.primary.skillDef.skillNameToken == "TWITCH_PRIMARY_SMG_NAME";
                if (flag3)
                {
                    return 1;
                }
                bool flag4 = charBody.skillLocator.primary.skillDef.skillNameToken == "TWITCH_PRIMARY_BAZOOKA_NAME";
                if (flag4)
                {
                    return 2;
                }
                bool flag5 = charBody.skillLocator.primary.skillDef.skillNameToken == "TWITCH_PRIMARY_SHOTGUN_NAME";
                if (flag5)
                {
                    return 3;
                }
            }
            return -1;
        }

        // Token: 0x06000086 RID: 134 RVA: 0x0000EC8B File Offset: 0x0000CE8B
        public void CheckWeapon()
        {
            EquipWeapon(GetWeapon());
            CheckCrosshair();
        }

        // Token: 0x06000087 RID: 135 RVA: 0x0000ECA4 File Offset: 0x0000CEA4
        private void EquipWeapon(int weapon)
        {
            bool flag = childLocator;
            if (flag)
            {
                bool flag2 = weapon == 0;
                if (flag2)
                {
                    childLocator.FindChild("Weapon").transform.localScale = new Vector3(1f, 1f, 1f);
                    childLocator.FindChild("Gun").gameObject.SetActive(false);
                    childLocator.FindChild("Bazooka").gameObject.SetActive(false);
                    childLocator.FindChild("Shotgun").gameObject.SetActive(false);
                }
                else
                {
                    bool flag3 = weapon == 1;
                    if (flag3)
                    {
                        childLocator.FindChild("Weapon").transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        childLocator.FindChild("Gun").gameObject.SetActive(true);
                        childLocator.FindChild("Bazooka").gameObject.SetActive(false);
                        childLocator.FindChild("Shotgun").gameObject.SetActive(false);
                    }
                    else
                    {
                        bool flag4 = weapon == 2;
                        if (flag4)
                        {
                            childLocator.FindChild("Weapon").transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            childLocator.FindChild("Gun").gameObject.SetActive(false);
                            childLocator.FindChild("Bazooka").gameObject.SetActive(true);
                            childLocator.FindChild("Shotgun").gameObject.SetActive(false);
                        }
                        else
                        {
                            bool flag5 = weapon == 3;
                            if (flag5)
                            {
                                childLocator.FindChild("Weapon").transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                                childLocator.FindChild("Gun").gameObject.SetActive(false);
                                childLocator.FindChild("Bazooka").gameObject.SetActive(false);
                                childLocator.FindChild("Shotgun").gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        private static ItemDisplayRule[] GetItemDisplayRule(ItemDisplayRuleSet itemDisplayRuleSet, UnityEngine.Object keyAsset)
        {
            for (int i = 0; i < itemDisplayRuleSet.keyAssetRuleGroups.Length; i++)
            {
                ref ItemDisplayRuleSet.KeyAssetRuleGroup ptr = ref itemDisplayRuleSet.keyAssetRuleGroups[i];

                if (ptr.keyAsset && ptr.keyAsset == keyAsset)
                {
                    return ptr.displayRuleGroup.rules;
                }
            }

            return default;
        }

        private static void SetItemDisplayRule(ItemDisplayRuleSet itemDisplayRuleSet, UnityEngine.Object keyAsset, ItemDisplayRule[] rules)
        {
            for (int i = 0; i < itemDisplayRuleSet.keyAssetRuleGroups.Length; i++)
            {
                ref ItemDisplayRuleSet.KeyAssetRuleGroup ptr = ref itemDisplayRuleSet.keyAssetRuleGroups[i];

                if (ptr.keyAsset && ptr.keyAsset == keyAsset)
                {
                    ptr.displayRuleGroup.rules = rules;
                }
            }
        }

        private void SetWeaponDisplays(int newWeapon)
        {
            ItemDisplayRule[] rules = default;
            var characterModel = GetComponentInChildren<CharacterModel>();
            if (!characterModel) return;
            ItemDisplayRuleSet itemDisplayRuleSet = characterModel.itemDisplayRuleSet;
            if (newWeapon == 0)
            {
                rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth);
                rules[0].childName = "Weapon";
                rules[0].localPos = new Vector3(78f, 34.4f, 0f);
                rules[0].localAngles = new Vector3(-90f, 180f, 90f);
                rules[0].localScale = new Vector3(12f, 12f, 12f);
                SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth, rules);

                rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning);
                rules[0].childName = "Weapon";
                rules[0].localPos = new Vector3(34.1f, 0f, 0f);
                rules[0].localAngles = new Vector3(90f, 90f, 0f);
                rules[0].localScale = new Vector3(24f, 24f, 24f);
                SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning, rules);

                rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus);
                rules[0].childName = "Weapon";
                rules[0].localPos = new Vector3(0f, 0f, 0f);
                rules[0].localAngles = new Vector3(0f, 0f, 0f);
                rules[0].localScale = new Vector3(16f, 16f, 16f);
                SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus, rules);

                rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine);
                rules[0].childName = "Weapon";
                rules[0].localPos = new Vector3(10f, 16f, 0f);
                rules[0].localAngles = new Vector3(90f, 90f, 0f);
                rules[0].localScale = new Vector3(8f, 8f, 8f);
                SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine, rules);
            }
            else
            {
                bool flag2 = newWeapon == 1;
                if (flag2)
                {
                    rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth);
                    rules[0].childName = "Gun";
                    rules[0].localPos = new Vector3(50.8f, 38f, 0f);
                    rules[0].localAngles = new Vector3(-90f, 0f, -90f);
                    rules[0].localScale = new Vector3(12f, 12f, 12f);
                    SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth, rules);

                    rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning);
                    rules[0].childName = "Gun";
                    rules[0].localPos = new Vector3(90.2f, 13.6f, 0f);
                    rules[0].localAngles = new Vector3(-90f, -90f, 0f);
                    rules[0].localScale = new Vector3(24f, 24f, 24f);
                    SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning, rules);

                    rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus);
                    rules[0].childName = "Gun";
                    rules[0].localPos = new Vector3(8.4f, 11.4f, 0f);
                    rules[0].localAngles = new Vector3(0f, 0f, 0f);
                    rules[0].localScale = new Vector3(16f, 16f, 16f);
                    SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus, rules);

                    rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine);
                    rules[0].childName = "Gun";
                    rules[0].localPos = new Vector3(41f, -17.4f, 0f);
                    rules[0].localAngles = new Vector3(0f, -90f, 0f);
                    rules[0].localScale = new Vector3(12f, 12f, 12f);
                    SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine, rules);
                }
                else
                {
                    bool flag3 = newWeapon == 2;
                    if (flag3)
                    {
                        rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth);
                        rules[0].childName = "Bazooka";
                        rules[0].localPos = new Vector3(0f, 0f, 4f);
                        rules[0].localAngles = new Vector3(0f, 0f, 0f);
                        rules[0].localScale = new Vector3(1f, 1f, 1f);
                        SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth, rules);

                        rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning);
                        rules[0].childName = "Bazooka";
                        rules[0].localPos = new Vector3(-1.72f, -1.8f, 0f);
                        rules[0].localAngles = new Vector3(0f, -90f, 0f);
                        rules[0].localScale = new Vector3(2.24f, 2.24f, 2.24f);
                        SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning, rules);

                        rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus);
                        rules[0].childName = "Bazooka";
                        rules[0].localPos = new Vector3(0f, -4.55f, 0.71f);
                        rules[0].localAngles = new Vector3(0f, 0f, 0f);
                        rules[0].localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus, rules);

                        rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine);
                        rules[0].childName = "Bazooka";
                        rules[0].localPos = new Vector3(0f, -2.51f, -3.02f);
                        rules[0].localAngles = new Vector3(75f, 0f, 0f);
                        rules[0].localScale = new Vector3(0.75f, 0.75f, 0.75f);
                        SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine, rules);
                    }
                    else
                    {
                        bool flag4 = newWeapon == 3;
                        if (flag4)
                        {

                            rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth);
                            rules[0].childName = "Shotgun";
                            rules[0].localPos = new Vector3(0f, -0.46f, 0.24f);
                            rules[0].localAngles = new Vector3(0f, 0f, 180f);
                            rules[0].localScale = new Vector3(0.06f, 0.1f, 0.075f);
                            SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.Behemoth, rules);

                            rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning);
                            rules[0].childName = "Shotgun";
                            rules[0].localPos = new Vector3(0f, -0.35f, 0.13f);
                            rules[0].localAngles = new Vector3(0f, 0f, -180f);
                            rules[0].localScale = new Vector3(0.105f, 0.18f, 0.14f);
                            SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.ChainLightning, rules);

                            rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus);
                            rules[0].childName = "Shotgun";
                            rules[0].localPos = new Vector3(0f, 0.16f, 0.08f);
                            rules[0].localAngles = new Vector3(0f, 0f, 0f);
                            rules[0].localScale = new Vector3(0.075f, 0.125f, 0.075f);
                            SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.NearbyDamageBonus, rules);

                            rules = GetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine);
                            rules[0].childName = "Shotgun";
                            rules[0].localPos = new Vector3(0f, -0.08f, -0.002f);
                            rules[0].localAngles = new Vector3(8f, 0f, 0f);
                            rules[0].localScale = new Vector3(0.045f, 0.075f, 0.045f);
                            SetItemDisplayRule(itemDisplayRuleSet, RoR2Content.Items.SecondarySkillMagazine, rules);
                        }
                    }
                }
            }

            itemDisplayRuleSet.GenerateRuntimeValues();
        }

        // Token: 0x06000089 RID: 137 RVA: 0x0000FA48 File Offset: 0x0000DC48
        public string GetMuzzleName()
        {
            bool flag = GetWeapon() == 0;
            string result;
            if (flag)
            {
                result = "Muzzle";
            }
            else
            {
                bool flag2 = GetWeapon() == 1;
                if (flag2)
                {
                    result = "GunMuzzle";
                }
                else
                {
                    bool flag3 = GetWeapon() == 2;
                    if (flag3)
                    {
                        result = "BazookaMuzzle";
                    }
                    else
                    {
                        bool flag4 = GetWeapon() == 3;
                        if (flag4)
                        {
                            result = "ShotgunMuzzle";
                        }
                        else
                        {
                            result = "";
                        }
                    }
                }
            }
            return result;
        }

        // Token: 0x0600008A RID: 138 RVA: 0x0000FABC File Offset: 0x0000DCBC
        public void GetAmbushBuff(float duration)
        {
            bool flag = charBody;
            if (flag)
            {
                bool flag2 = GetWeapon() != 2;
                if (flag2)
                {
                    charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/BanditCrosshair");
                }
                charBody.RecalculateStats();
            }
            bool flag3 = ambushFX;
            if (flag3)
            {
                ambushFX.Play();
            }
            hasAttacked = false;
            base.CancelInvoke();
            base.Invoke("AmbushBuffEnd", duration);
        }

        // Token: 0x0600008B RID: 139 RVA: 0x0000FB44 File Offset: 0x0000DD44
        public void GetMiniAmbushBuff(float duration)
        {
            bool flag = charBody;
            if (flag)
            {
                bool flag2 = GetWeapon() != 2;
                if (flag2)
                {
                    charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/BanditCrosshair");
                }
                charBody.RecalculateStats();
            }
            bool flag3 = miniAmbushFX;
            if (flag3)
            {
                miniAmbushFX.Play();
            }
            hasAttacked = false;
            base.CancelInvoke();
            base.Invoke("AmbushBuffEnd", duration);
        }

        // Token: 0x0600008C RID: 140 RVA: 0x0000FBCC File Offset: 0x0000DDCC
        private void AmbushBuffEnd()
        {
            CheckCrosshair();
            bool flag = charBody;
            if (flag)
            {
                charBody.RecalculateStats();
            }
        }

        // Token: 0x0600008D RID: 141 RVA: 0x0000FBFC File Offset: 0x0000DDFC
        public void AmbushAttack()
        {
            bool flag = !hasAttacked;
            if (flag)
            {
                bool flag2 = GetWeapon() == 1;
                if (flag2)
                {
                    Util.PlaySound(Sounds.TwitchAmbushGun, base.gameObject);
                }
                else
                {
                    Util.PlaySound(Sounds.TwitchAmbush, base.gameObject);
                }
            }
            hasAttacked = true;
        }

        // Token: 0x0600008E RID: 142 RVA: 0x0000FC58 File Offset: 0x0000DE58
        public void EnterStealth()
        {
            bool flag = stealthFX;
            if (flag)
            {
                stealthFX.Play();
            }
        }

        // Token: 0x0600008F RID: 143 RVA: 0x0000FC84 File Offset: 0x0000DE84
        public void ExitStealth()
        {
            bool flag = revealFX;
            if (flag)
            {
                revealFX.Play();
            }
        }

        // Token: 0x06000090 RID: 144 RVA: 0x0000FCB0 File Offset: 0x0000DEB0
        private void CheckCrosshair()
        {
            bool flag = charBody;
            if (flag)
            {
                bool flag2 = GetWeapon() == 0;
                if (flag2)
                {
                    charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/TreebotCrosshair");
                }
                else
                {
                    bool flag3 = GetWeapon() == 1;
                    if (flag3)
                    {
                        charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/StandardCrosshair");
                    }
                    else
                    {
                        bool flag4 = GetWeapon() == 2;
                        if (flag4)
                        {
                            charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/ToolbotGrenadeLauncherCrosshair");
                        }
                        else
                        {
                            bool flag5 = GetWeapon() == 3;
                            if (flag5)
                            {
                                charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/SMGCrosshair");
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000091 RID: 145 RVA: 0x0000FD6C File Offset: 0x0000DF6C
        public void UpdateGrenadeLifetime(float newLifetime)
        {
            bool flag = Twitch.grenadeProjectile;
            if (flag)
            {
                Twitch.grenadeProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = newLifetime;
            }
        }

        // Token: 0x06000092 RID: 146 RVA: 0x0000FD98 File Offset: 0x0000DF98
        public TwitchController()
        {
        }

        // Token: 0x04000094 RID: 148
        private bool hasAttacked;

        // Token: 0x04000095 RID: 149
        private ParticleSystem stealthFX;

        // Token: 0x04000096 RID: 150
        private ParticleSystem revealFX;

        // Token: 0x04000097 RID: 151
        private ParticleSystem ambushFX;

        // Token: 0x04000098 RID: 152
        private ParticleSystem miniAmbushFX;

        // Token: 0x04000099 RID: 153
        private CharacterBody charBody;

        // Token: 0x0400009A RID: 154
        private CharacterMotor charMotor;

        // Token: 0x0400009B RID: 155
        private HealthComponent charHealth;

        // Token: 0x0400009C RID: 156
        private ChildLocator childLocator;

        // Token: 0x0400009D RID: 157
        private InputBankTest inputBank;
    }
}
