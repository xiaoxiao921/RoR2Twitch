using System;
using EntityStates.ClayBruiser.Weapon;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.GolemMonster;
using RoR2;
using RoR2.Projectile;
using Twitch;
using UnityEngine;

namespace EntityStates.TwitchStates
{
    // Token: 0x02000002 RID: 2
    public class TwitchFireBolt : BaseSkillState
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.15f * duration;
            base.characterBody.SetAimTimer(2f);
            animator = base.GetModelAnimator();
            muzzleString = "Muzzle";
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

        // Token: 0x06000002 RID: 2 RVA: 0x0000217F File Offset: 0x0000037F
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000218C File Offset: 0x0000038C
        private void FireBolt()
        {
            bool flag = !hasFired;
            if (flag)
            {
                hasFired = true;
                bool flag2 = base.characterBody.HasBuff(Twitch.Twitch.ambushBuff);
                if (flag2)
                {
                    Util.PlaySound(Sounds.TwitchAttackLaser, base.gameObject);
                    EffectManager.SimpleMuzzleFlash(FireLaser.effectPrefab, base.gameObject, muzzleString, false);
                    base.AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                    bool isAuthority = base.isAuthority;
                    if (isAuthority)
                    {
                        float damage = TwitchFireBolt.damageCoefficient * damageStat;
                        float force = 50f;
                        float procCoefficient = 1f;
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
                        bulletAttack.maxSpread = 3f;
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
                        bulletAttack.spreadPitchScale = 0.25f;
                        bulletAttack.spreadYawScale = 0.25f;
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
                    bool isAuthority2 = base.isAuthority;
                    if (isAuthority2)
                    {
                        ProjectileManager.instance.FireProjectile(Twitch.Twitch.boltProjectile, aimRay2.origin, Util.QuaternionSafeLookRotation(aimRay2.direction), base.gameObject, TwitchFireBolt.damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, TwitchFireBolt.projectileSpeed);
                    }
                }
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x0000243C File Offset: 0x0000063C
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = base.fixedAge >= fireDuration;
            if (flag)
            {
                FireBolt();
            }
            bool flag2 = base.fixedAge >= duration && base.isAuthority;
            if (flag2)
            {
                outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002498 File Offset: 0x00000698
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000024AB File Offset: 0x000006AB
        public TwitchFireBolt()
        {
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000024CA File Offset: 0x000006CA
        // Note: this type is marked as 'beforefieldinit'.
        static TwitchFireBolt()
        {
        }

        // Token: 0x04000001 RID: 1
        public static float damageCoefficient = 2.25f;

        // Token: 0x04000002 RID: 2
        public float baseDuration = 0.75f;

        // Token: 0x04000003 RID: 3
        public float recoil = 1f;

        // Token: 0x04000004 RID: 4
        public static float projectileSpeed = 120f;

        // Token: 0x04000006 RID: 6
        private float duration;

        // Token: 0x04000007 RID: 7
        private float fireDuration;

        // Token: 0x04000008 RID: 8
        private bool hasFired;

        // Token: 0x04000009 RID: 9
        private Animator animator;

        // Token: 0x0400000A RID: 10
        private string muzzleString;

        // Token: 0x0400000B RID: 11
        private TwitchController twitchController;
    }
}
