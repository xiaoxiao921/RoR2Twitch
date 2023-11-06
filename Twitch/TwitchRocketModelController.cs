using System;
using UnityEngine;

namespace Twitch
{
    // Token: 0x02000014 RID: 20
    public class TwitchRocketModelController : MonoBehaviour
    {
        // Token: 0x0600007E RID: 126 RVA: 0x0000E85A File Offset: 0x0000CA5A
        private void Awake()
        {
            rb = base.transform.root.GetComponentInChildren<Rigidbody>();
            base.InvokeRepeating("AlignModel", 0.05f, 0.05f);
        }

        // Token: 0x0600007F RID: 127 RVA: 0x0000E889 File Offset: 0x0000CA89
        private void FixedUpdate()
        {
            AlignModel();
        }

        // Token: 0x06000080 RID: 128 RVA: 0x0000E894 File Offset: 0x0000CA94
        private void AlignModel()
        {
            bool flag = rb;
            if (flag)
            {
                base.transform.rotation = Quaternion.LookRotation(base.transform.position + rb.velocity);
            }
        }

        // Token: 0x06000081 RID: 129 RVA: 0x0000E8DF File Offset: 0x0000CADF
        public TwitchRocketModelController()
        {
        }

        // Token: 0x04000093 RID: 147
        private Rigidbody rb;
    }
}
