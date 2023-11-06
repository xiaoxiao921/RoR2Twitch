using System;
using RoR2;
using UnityEngine;

namespace Twitch
{
    // Token: 0x0200000F RID: 15
    public class ApplyHorns : MonoBehaviour
    {
        // Token: 0x06000070 RID: 112 RVA: 0x0000E488 File Offset: 0x0000C688
        private void Start()
        {
            model = base.GetComponentInChildren<CharacterModel>();
            childLocator = base.GetComponentInChildren<ChildLocator>();
            AddHorns();
        }

        // Token: 0x06000071 RID: 113 RVA: 0x0000E4B8 File Offset: 0x0000C6B8
        private void AddHorns()
        {
            bool flag = model;
            if (flag)
            {
                DisplayRuleGroup equipmentDisplayRuleGroup = model.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixRed.equipmentIndex);
                bool flag2 = equipmentDisplayRuleGroup.rules.Length > 1;
                if (flag2)
                {
                    Transform transform = childLocator.FindChild(equipmentDisplayRuleGroup.rules[0].childName);
                    bool flag3 = transform;
                    if (flag3)
                    {
                        Apply(model, equipmentDisplayRuleGroup.rules[0].followerPrefab, transform, equipmentDisplayRuleGroup.rules[0].localPos, Quaternion.Euler(equipmentDisplayRuleGroup.rules[0].localAngles), equipmentDisplayRuleGroup.rules[0].localScale);
                    }
                    Transform exists = childLocator.FindChild(equipmentDisplayRuleGroup.rules[1].childName);
                    bool flag4 = exists;
                    if (flag4)
                    {
                        Apply(model, equipmentDisplayRuleGroup.rules[1].followerPrefab, transform, equipmentDisplayRuleGroup.rules[1].localPos, Quaternion.Euler(equipmentDisplayRuleGroup.rules[1].localAngles), equipmentDisplayRuleGroup.rules[1].localScale);
                    }
                }
            }
            else
            {
                Debug.LogError("[LoLTwitchMod] AddHorns - no charactermodel");
            }
        }

        // Token: 0x06000072 RID: 114 RVA: 0x0000E620 File Offset: 0x0000C820
        private void Apply(CharacterModel characterModel, GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject gameObject = Instantiate<GameObject>(prefab.gameObject, parent);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation;
            gameObject.transform.localScale = localScale;
            LimbMatcher component = gameObject.GetComponent<LimbMatcher>();
            bool flag = component && childLocator;
            if (flag)
            {
                component.SetChildLocator(childLocator);
            }
        }

        // Token: 0x06000073 RID: 115 RVA: 0x0000E695 File Offset: 0x0000C895
        public ApplyHorns()
        {
        }

        // Token: 0x0400008C RID: 140
        private CharacterModel model;

        // Token: 0x0400008D RID: 141
        private ChildLocator childLocator;
    }
}
