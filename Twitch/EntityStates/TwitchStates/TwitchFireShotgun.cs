using System;
using EntityStates.ClayBruiser.Weapon;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using Twitch;
using UnityEngine;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000004 RID: 4
    public class TwitchFireShotgun : BaseSkillState
    {
        // Token: 0x0600000F RID: 15 RVA: 0x00002B34 File Offset: 0x00000D34
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.1f * duration;
            base.characterBody.SetAimTimer(2f);
            animator = base.GetModelAnimator();
            muzzleString = "Muzzle";
            hasFired = false;
            twitchController = base.GetComponent<TwitchController>();
            bool flag = twitchController;
            if (flag)
            {
                muzzleString = twitchController.GetMuzzleName();
            }
            bool flag2 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
            if (flag2)
            {
                base.PlayAnimation("Gesture, Override", "FireEmpoweredBolt", "FireBolt.playbackRate", 1.5f * duration);
            }
            else
            {
                base.PlayAnimation("Gesture, Override", "FireExplosive", "FireExplosive.playbackRate", duration);
            }
            Util.PlaySound(Sounds.TwitchAttackStart, base.gameObject);
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002C32 File Offset: 0x00000E32
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002C3C File Offset: 0x00000E3C
        private void FireBullet()
        {
            bool flag = !hasFired;
            if (flag)
            {
                hasFired = true;
                bool flag2 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
                bool flag3 = flag2;
                if (flag3)
                {
                    Util.PlayAttackSpeedSound(Sounds.TwitchFireShotgun, base.gameObject, attackSpeedStat);
                    base.AddRecoil(-2f * beamRecoil, -3f * beamRecoil, -1f * beamRecoil, 1f * beamRecoil);
                    base.characterBody.AddSpreadBloom(0.33f * bulletRecoil);
                    EffectManager.SimpleMuzzleFlash(FireShotgun.effectPrefab, base.gameObject, muzzleString, false);
                    bool isAuthority = base.isAuthority;
                    if (isAuthority)
                    {
                        float damage = TwitchFireShotgun.damageCoefficient * damageStat;
                        float force = 10f;
                        float procCoefficient = 0.4f;
                        bool isCrit = base.RollCrit();
                        Ray aimRay = base.GetAimRay();
                        BulletAttack bulletAttack = new BulletAttack();
                        bulletAttack.bulletCount = (uint)projectileCount;
                        bulletAttack.aimVector = aimRay.direction;
                        bulletAttack.origin = aimRay.origin;
                        bulletAttack.damage = damage;
                        bulletAttack.damageColorIndex = DamageColorIndex.Default;
                        bulletAttack.damageType = DamageType.BlightOnHit;
                        bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                        bulletAttack.maxDistance = 40f;
                        bulletAttack.force = force;
                        bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
                        bulletAttack.minSpread = 0f;
                        bulletAttack.maxSpread = 5f;
                        bulletAttack.isCrit = isCrit;
                        bulletAttack.owner = base.gameObject;
                        bulletAttack.muzzleName = muzzleString;
                        bulletAttack.smartCollision = true;
                        bulletAttack.procChainMask = default(ProcChainMask);
                        bulletAttack.procCoefficient = procCoefficient;
                        bulletAttack.radius = 1.5f;
                        bulletAttack.sniper = false;
                        bulletAttack.stopperMask = LayerIndex.world.mask;
                        bulletAttack.weapon = null;
                        bulletAttack.tracerEffectPrefab = TwitchFireShotgun.bulletTracerEffectPrefab;
                        bulletAttack.spreadPitchScale = 0.5f;
                        bulletAttack.spreadYawScale = 0.5f;
                        bulletAttack.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                        bulletAttack.hitEffectPrefab = MinigunFire.bulletHitEffectPrefab;
                        bulletAttack.HitEffectNormal = MinigunFire.bulletHitEffectNormal;
                        bulletAttack.Fire();
                    }
                }
                else
                {
                    Util.PlaySound(Sounds.TwitchFireShotgun, base.gameObject);
                    base.AddRecoil(-2f * bulletRecoil, -3f * bulletRecoil, -1f * bulletRecoil, 1f * bulletRecoil);
                    base.characterBody.AddSpreadBloom(0.33f * bulletRecoil);
                    EffectManager.SimpleMuzzleFlash(FireShotgun.effectPrefab, base.gameObject, muzzleString, false);
                    bool isAuthority2 = base.isAuthority;
                    if (isAuthority2)
                    {
                        float damage2 = TwitchFireShotgun.damageCoefficient * damageStat;
                        float force2 = 10f;
                        float procCoefficient2 = 0.4f;
                        bool isCrit2 = base.RollCrit();
                        Ray aimRay2 = base.GetAimRay();
                        BulletAttack bulletAttack2 = new BulletAttack();
                        bulletAttack2.bulletCount = (uint)projectileCount;
                        bulletAttack2.aimVector = aimRay2.direction;
                        bulletAttack2.origin = aimRay2.origin;
                        bulletAttack2.damage = damage2;
                        bulletAttack2.damageColorIndex = DamageColorIndex.Default;
                        bulletAttack2.damageType = DamageType.BlightOnHit;
                        bulletAttack2.falloffModel = BulletAttack.FalloffModel.None;
                        bulletAttack2.maxDistance = 40f;
                        bulletAttack2.force = force2;
                        bulletAttack2.hitMask = LayerIndex.CommonMasks.bullet;
                        bulletAttack2.minSpread = 0f;
                        bulletAttack2.maxSpread = 30f;
                        bulletAttack2.isCrit = isCrit2;
                        bulletAttack2.owner = base.gameObject;
                        bulletAttack2.muzzleName = muzzleString;
                        bulletAttack2.smartCollision = false;
                        bulletAttack2.procChainMask = default(ProcChainMask);
                        bulletAttack2.procCoefficient = procCoefficient2;
                        bulletAttack2.radius = 1.5f;
                        bulletAttack2.sniper = false;
                        bulletAttack2.stopperMask = LayerIndex.world.mask;
                        bulletAttack2.weapon = null;
                        bulletAttack2.tracerEffectPrefab = TwitchFireShotgun.bulletTracerEffectPrefab;
                        bulletAttack2.spreadPitchScale = 0.5f;
                        bulletAttack2.spreadYawScale = 0.5f;
                        bulletAttack2.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                        bulletAttack2.hitEffectPrefab = MinigunFire.bulletHitEffectPrefab;
                        bulletAttack2.HitEffectNormal = MinigunFire.bulletHitEffectNormal;
                        bulletAttack2.Fire();
                    }
                }
            }
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00003050 File Offset: 0x00001250
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.fixedAge >= fireDuration;
            if (flag)
            {
                FireBullet();
            }
            bool flag2 = base.fixedAge >= duration && base.isAuthority;
            if (flag2)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000030AC File Offset: 0x000012AC
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x000030BF File Offset: 0x000012BF
        public TwitchFireShotgun()
        {
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000030F0 File Offset: 0x000012F0
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchFireShotgun()
        {
        }

        // Token: 0x0400001B RID: 27
        public static float damageCoefficient = 0.9f;

        // Token: 0x0400001C RID: 28
        public float baseDuration = 0.9f;

        // Token: 0x0400001D RID: 29
        public int projectileCount = 4;

        // Token: 0x0400001E RID: 30
        public float bulletRecoil = 3f;

        // Token: 0x0400001F RID: 31
        public float beamRecoil = 2.5f;

        // Token: 0x04000020 RID: 32
        public static GameObject bulletTracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoShotgun");

        // Token: 0x04000022 RID: 34
        private float duration;

        // Token: 0x04000023 RID: 35
        private float fireDuration;

        // Token: 0x04000024 RID: 36
        private bool hasFired;

        // Token: 0x04000025 RID: 37
        private Animator animator;

        // Token: 0x04000026 RID: 38
        private string muzzleString;

        // Token: 0x04000027 RID: 39
        private TwitchController twitchController;
    }
}
