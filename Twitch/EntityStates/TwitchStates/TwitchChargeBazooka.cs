using System;
using EntityStates.LemurianBruiserMonster;
using RoR2;
using RoR2.Projectile;
using Twitch;
using UnityEngine;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000006 RID: 6
    public class TwitchChargeBazooka : BaseSkillState
    {
        // Token: 0x0600001D RID: 29 RVA: 0x0000331C File Offset: 0x0000151C
        public override void OnEnter()
        {
            base.OnEnter();
            duration = minimumDuration / attackSpeedStat;
            releaseDuration = maximumDuration / attackSpeedStat;
            animator = base.GetModelAnimator();
            muzzleString = "Muzzle";
            twitchController = base.GetComponent<TwitchController>();
            hasFired = false;
            bool flag = twitchController;
            if (flag)
            {
                muzzleString = twitchController.GetMuzzleName();
            }
            Transform modelTransform = base.GetModelTransform();
            bool flag2 = modelTransform;
            if (flag2)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                bool flag3 = component;
                if (flag3)
                {
                    Transform transform = component.FindChild(muzzleString);
                    bool flag4 = transform && ChargeMegaFireball.chargeEffectPrefab;
                    if (flag4)
                    {
                        chargeInstance = UnityEngine.Object.Instantiate(ChargeMegaFireball.chargeEffectPrefab, transform.position, transform.rotation);
                        chargeInstance.transform.parent = transform;
                        chargeInstance.transform.localScale *= 0.25f;
                        chargeInstance.transform.localPosition = Vector3.zero;
                        ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
                        bool flag5 = component2;
                        if (flag5)
                        {
                            component2.newDuration = releaseDuration;
                        }
                    }
                }
            }
            Util.PlayAttackSpeedSound(Sounds.TwitchCharge, base.gameObject, attackSpeedStat);
        }

        // Token: 0x0600001E RID: 30 RVA: 0x000034A8 File Offset: 0x000016A8
        public override void OnExit()
        {
            base.OnExit();
            bool flag = chargeInstance;
            if (flag)
            {
                EntityState.Destroy(chargeInstance);
            }
        }

        // Token: 0x0600001F RID: 31 RVA: 0x000034DC File Offset: 0x000016DC
        private void FireBazooka()
        {
            bool flag = !hasFired;
            if (flag)
            {
                float num = (base.fixedAge - duration) / releaseDuration;
                Util.PlaySound(Sounds.TwitchAttackBazooka, base.gameObject);
                EffectManager.SimpleMuzzleFlash(FireMegaFireball.muzzleflashEffectPrefab, base.gameObject, muzzleString, true);
                bool flag2 = num >= 0.75f;
                if (flag2)
                {
                    base.PlayAnimation("Gesture, Override", "FireEmpoweredBolt", "FireBolt.playbackRate", duration * 2f);
                }
                else
                {
                    base.PlayAnimation("Gesture, Override", "FireExplosive", "FireExplosive.playbackRate", duration * 2f);
                }
                Ray aimRay = base.GetAimRay();
                bool isAuthority = base.isAuthority;
                if (isAuthority)
                {
                    float num2 = Mathf.Lerp(minDamageCoefficient, maxDamageCoefficient, num);
                    float num3 = Mathf.Lerp(minProcCoefficient, maxProcCoefficient, num);
                    float speedOverride = Mathf.Lerp(minSpeed, maxSpeed, num);
                    float num4 = Mathf.Lerp(minRecoil, maxRecoil, num);
                    base.AddRecoil(-2f * num4, -3f * num4, -1f * num4, 1f * num4);
                    base.characterBody.AddSpreadBloom(0.33f * num4);
                    ProjectileManager.instance.FireProjectile(Twitch.Twitch.bazookaProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, num2 * damageStat, force, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speedOverride);
                }
                TwitchFireBazooka nextState = new TwitchFireBazooka();
                outer.SetNextState(nextState);
            }
            hasFired = true;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000036A4 File Offset: 0x000018A4
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(0.5f);
            bool flag = base.fixedAge >= releaseDuration;
            if (flag)
            {
                FireBazooka();
            }
            bool flag2 = base.inputBank;
            if (flag2)
            {
                bool flag3 = base.fixedAge >= duration && base.isAuthority && !base.inputBank.skill1.down;
                if (flag3)
                {
                    FireBazooka();
                }
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003734 File Offset: 0x00001934
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00003748 File Offset: 0x00001948
        public TwitchChargeBazooka()
        {
        }

        // Token: 0x04000031 RID: 49
        public float maximumDuration = 2.25f;

        // Token: 0x04000032 RID: 50
        public float minimumDuration = 0.5f;

        // Token: 0x04000033 RID: 51
        public float maxDamageCoefficient = 22.5f;

        // Token: 0x04000034 RID: 52
        public float minDamageCoefficient = 1.5f;

        // Token: 0x04000035 RID: 53
        public float maxProcCoefficient = 0.8f;

        // Token: 0x04000036 RID: 54
        public float minProcCoefficient = 0.1f;

        // Token: 0x04000037 RID: 55
        public float maxSpeed = 200f;

        // Token: 0x04000038 RID: 56
        public float minSpeed = 10f;

        // Token: 0x04000039 RID: 57
        public float maxRecoil = 15f;

        // Token: 0x0400003A RID: 58
        public float minRecoil = 0.5f;

        // Token: 0x0400003B RID: 59
        public float force = 500f;

        // Token: 0x0400003C RID: 60
        private float releaseDuration;

        // Token: 0x0400003D RID: 61
        private float duration;

        // Token: 0x0400003E RID: 62
        private bool hasFired;

        // Token: 0x0400003F RID: 63
        private Animator animator;

        // Token: 0x04000040 RID: 64
        private string muzzleString;

        // Token: 0x04000041 RID: 65
        private TwitchController twitchController;

        // Token: 0x04000042 RID: 66
        private GameObject chargeInstance;
    }
}
