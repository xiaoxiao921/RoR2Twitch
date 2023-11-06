using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using UnityEngine;

namespace Twitch
{
    // Token: 0x02000016 RID: 22
    public class MenuAnim : MonoBehaviour
    {
        // Token: 0x06000093 RID: 147 RVA: 0x0000FDA4 File Offset: 0x0000DFA4
        internal void OnEnable()
        {
            bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
            bool flag2 = flag;
            if (flag2)
            {
                base.StartCoroutine(SpawnAnim());
            }
        }

        // Token: 0x06000094 RID: 148 RVA: 0x0000FDEB File Offset: 0x0000DFEB
        private IEnumerator SpawnAnim()
        {
            Animator animator = base.GetComponentInChildren<Animator>();
            EffectManager.SpawnEffect(CastSmokescreenNoDelay.smokescreenEffectPrefab, new EffectData
            {
                origin = base.gameObject.transform.position
            }, false);
            Util.PlaySound(Sounds.TwitchExitStealth, base.gameObject);
            Util.PlaySound(Sounds.TwitchMenu, base.gameObject);
            PlayAnimation("Fullbody, Override", "Menu", "", 1f, animator);
            yield break;
        }

        // Token: 0x06000095 RID: 149 RVA: 0x0000FDFC File Offset: 0x0000DFFC
        private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            animator.SetFloat(playbackRateParam, 1f);
            animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
            animator.Update(0f);
            float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
            animator.SetFloat(playbackRateParam, length / duration);
        }
    }
}
