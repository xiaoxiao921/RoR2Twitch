using System;
using RoR2;
using UnityEngine;

namespace Twitch
{
    // Token: 0x02000011 RID: 17
    public class TwitchGrenadeTicker : MonoBehaviour
    {
        // Token: 0x06000076 RID: 118 RVA: 0x0000E6B7 File Offset: 0x0000C8B7
        private void OnEnable()
        {
            Util.PlaySound(Sounds.TwitchGrenadeTick, base.gameObject);
        }

        // Token: 0x06000077 RID: 119 RVA: 0x0000E6CB File Offset: 0x0000C8CB
        public TwitchGrenadeTicker()
        {
        }
    }
}
