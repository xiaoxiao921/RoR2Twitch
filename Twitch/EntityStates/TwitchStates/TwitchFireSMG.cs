using System;
using EntityStates.ClayBruiser.Weapon;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.GolemMonster;
using RoR2;
using Twitch;
using UnityEngine;
using RoR2.Projectile;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000003 RID: 3
    public class TwitchFireSMG : BaseSkillState
    {
        // Token: 0x06000008 RID: 8 RVA: 0x000024EC File Offset: 0x000006EC
        public override void OnEnter()
        {
            base.OnEnter();
            duration = TwitchFireSMG.baseDuration / attackSpeedStat;
            fireDuration = 0.2f * duration;
            base.characterBody.SetAimTimer(2f);
            animator = base.GetModelAnimator();
            muzzleString = "Muzzle";
            hasFired = 0;
            twitchController = base.GetComponent<TwitchController>();
            bool flag = twitchController;
            if (flag)
            {
                muzzleString = twitchController.GetMuzzleName();
            }

            Twitch.Twitch.LaserTracerMaterial.SetColor(152, Twitch.Twitch.laserTracerColor);
            bool flag2 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
            if (flag2)
            {
                base.PlayAnimation("Gesture, Override", "FireEmpoweredBolt", "FireBolt.playbackRate", 1.5f * duration);
            }
            else
            {
                base.PlayAnimation("Gesture, Override", "FireBolt", "FireBolt.playbackRate", 2f * duration);
            }
            bool flag3 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff) && twitchController;
            if (flag3)
            {
                twitchController.AmbushAttack();
            }
            else
            {
                Util.PlaySound(Sounds.TwitchAttackStart, base.gameObject);
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002621 File Offset: 0x00000821
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000262C File Offset: 0x0000082C
        private void FireBullet()
        {
            bool flag = hasFired < TwitchFireSMG.projectileCount;
            if (flag)
            {
                bool flag2 = hasFired == 0 && !base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
                if (flag2)
                {
                    Util.PlaySound(Sounds.TwitchAttackGun, base.gameObject);
                }
                hasFired++;
                lastFired = Time.time + fireInterval / attackSpeedStat;
                bool flag3 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
                if (flag3)
                {
                    Util.PlaySound(Sounds.TwitchAttackGunLaser, base.gameObject);
                    EffectManager.SimpleMuzzleFlash(FireLaser.effectPrefab, base.gameObject, muzzleString, false);
                    base.AddRecoil(-1f * beamRecoil, -2f * beamRecoil, -1f * beamRecoil, 1f * beamRecoil);
                    if (base.isAuthority)
                    {
                        float damage = damageCoefficient * damageStat;
                        float force = 10f;
                        float procCoefficient = 0.75f;
                        bool isCrit = base.RollCrit();
                        Ray aimRay = base.GetAimRay();
                        BulletAttack bulletAttack = new BulletAttack();
                        bulletAttack.bulletCount = 1U;
                        bulletAttack.aimVector = aimRay.direction;
                        bulletAttack.origin = aimRay.origin + new Vector3 (0f, 0.25f, 0f);
                        bulletAttack.damage = damage;
                        bulletAttack.damageColorIndex = DamageColorIndex.Default;
                        bulletAttack.damageType = DamageType.BlightOnHit;
                        bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                        bulletAttack.maxDistance = 512f;
                        bulletAttack.force = force;
                        bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
                        bulletAttack.minSpread = 0f;
                        bulletAttack.maxSpread = 5f;
                        bulletAttack.isCrit = isCrit;
                        bulletAttack.owner = base.gameObject;
                        bulletAttack.muzzleName = muzzleString;
                        bulletAttack.smartCollision = false;
                        bulletAttack.procChainMask = default(ProcChainMask);
                        bulletAttack.procCoefficient = procCoefficient;
                        bulletAttack.radius = 1f;
                        bulletAttack.sniper = false;
                        bulletAttack.stopperMask = LayerIndex.world.mask;
                        bulletAttack.weapon = null;
                        bulletAttack.tracerEffectPrefab = Twitch.Twitch.laserTracer;
                        bulletAttack.spreadPitchScale = 0.1f;
                        bulletAttack.spreadYawScale = 0.1f;
                        bulletAttack.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                        bulletAttack.hitEffectPrefab = MinigunFire.bulletHitEffectPrefab;
                        bulletAttack.HitEffectNormal = MinigunFire.bulletHitEffectNormal;
                        bulletAttack.Fire();
                    }
                }
                else
                {
                    Util.PlaySound(Sounds.TwitchAttack, base.gameObject);
                    base.characterBody.AddSpreadBloom(0.75f);
                    Ray aimRay2 = base.GetAimRay();
                    EffectManager.SimpleMuzzleFlash(FirePistol2.muzzleEffectPrefab, base.gameObject, muzzleString, false);
                    if (base.isAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(Twitch.Twitch.boltProjectile, aimRay2.origin + new Vector3(0f, 0.25f, 0f), Util.QuaternionSafeLookRotation(aimRay2.direction), base.gameObject, TwitchFireBolt.damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, TwitchFireBolt.projectileSpeed);
                    }
                }
            }
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002A54 File Offset: 0x00000C54
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.fixedAge >= fireDuration && Time.time > lastFired;
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

        // Token: 0x0600000C RID: 12 RVA: 0x00002AC0 File Offset: 0x00000CC0
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002AD3 File Offset: 0x00000CD3
        public TwitchFireSMG()
        {
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002B08 File Offset: 0x00000D08
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchFireSMG()
        {
        }

        // Token: 0x0400000C RID: 12
        public float damageCoefficient = 0.85f;

        // Token: 0x0400000D RID: 13
        public static float baseDuration = 1f;

        // Token: 0x0400000E RID: 14
        public float fireInterval = 0.1f;

        // Token: 0x0400000F RID: 15
        public static int projectileCount = 3;

        // Token: 0x04000010 RID: 16
        public float bulletRecoil = 0.75f;

        // Token: 0x04000011 RID: 17
        public float beamRecoil = 1f;

        // Token: 0x04000012 RID: 18
        public static GameObject bulletTracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerEngiTurret");

        // Token: 0x04000014 RID: 20
        private float duration;

        // Token: 0x04000015 RID: 21
        private float fireDuration;

        // Token: 0x04000016 RID: 22
        private int hasFired;

        // Token: 0x04000017 RID: 23
        private float lastFired;

        // Token: 0x04000018 RID: 24
        private Animator animator;

        // Token: 0x04000019 RID: 25
        private string muzzleString;

        // Token: 0x0400001A RID: 26
        private TwitchController twitchController;
    }
}
