using System;
using RoR2;
using UnityEngine;

namespace Twitch
{
    // Token: 0x02000013 RID: 19
    public class TwitchGrenadeController : MonoBehaviour
    {
        // Token: 0x0600007C RID: 124 RVA: 0x0000E7E0 File Offset: 0x0000C9E0
        public void EnableOverlay()
        {
            childLocator = base.GetComponent<ChildLocator>();
            bool flag = childLocator;
            if (flag)
            {
                grenadeOverlay = childLocator.FindChild("GrenadeOverlay").gameObject;
            }
            bool flag2 = grenadeOverlay;
            if (flag2)
            {
                grenadeOverlay.SetActive(true);
            }
            Util.PlaySound(Sounds.TwitchGrenadeTick, base.gameObject);
        }

        // Token: 0x0600007D RID: 125 RVA: 0x0000E851 File Offset: 0x0000CA51
        public TwitchGrenadeController()
        {
        }

        // Token: 0x04000091 RID: 145
        private GameObject grenadeOverlay;

        // Token: 0x04000092 RID: 146
        private ChildLocator childLocator;
    }
}
