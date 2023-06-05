using Main.Scripts.Core.Resources;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Customization.Configs;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Customization
{
    public class CharacterCustomization : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter head = default!;

        [SerializeField]
        private MeshFilter spine = default!;
        [SerializeField]
        private MeshFilter chest = default!;
        [SerializeField]
        private MeshFilter armLeftLower = default!;
        [SerializeField]
        private MeshFilter armLeftUpper = default!;
        [SerializeField]
        private MeshFilter armRightLower = default!;
        [SerializeField]
        private MeshFilter armRightUpper = default!;

        [SerializeField]
        private MeshFilter handLeft = default!;
        [SerializeField]
        private MeshFilter handRight = default!;

        [SerializeField]
        private MeshFilter hips = default!;
        [SerializeField]
        private MeshFilter legLeftLower = default!;
        [SerializeField]
        private MeshFilter legLeftUpper = default!;
        [SerializeField]
        private MeshFilter legRightLower = default!;
        [SerializeField]
        private MeshFilter legRightUpper = default!;

        [SerializeField]
        private MeshFilter footLeft = default!;
        [SerializeField]
        private MeshFilter footRight = default!;

        private CustomizationConfigsBank bank = default!;

        private void Awake()
        {
            bank = GlobalResources.Instance.ThrowWhenNull().CustomizationConfigsBank;
        }

        public void ApplyCustomizationData(CustomizationData customizationData)
        {
            if (customizationData.fullSetId >= 0)
            {
                ApplyFullSet(bank.FullSetConfigs.GetCustomizationConfig(customizationData.fullSetId));
                return;
            }

            ApplyHeadItem(bank.HeadConfigs.GetCustomizationConfig(customizationData.headId));
            ApplyBodyItem(bank.BodyConfigs.GetCustomizationConfig(customizationData.bodyId));
            ApplyHandsItem(bank.HandsConfigs.GetCustomizationConfig(customizationData.handsId));
            ApplyLegsItem(bank.LegsConfigs.GetCustomizationConfig(customizationData.legsId));
            ApplyFootsItem(bank.FootsConfigs.GetCustomizationConfig(customizationData.footsId));
        }

        private void ApplyHeadItem(CustomizationHeadItemConfig config)
        {
            head.sharedMesh = config.Head;
        }

        private void ApplyBodyItem(CustomizationBodyItemConfig config)
        {
            spine.sharedMesh = config.Spine;
            chest.sharedMesh = config.Chest;

            armLeftLower.sharedMesh = config.ArmLeftLower;
            armLeftUpper.sharedMesh = config.ArmLeftUpper;

            armRightLower.sharedMesh = config.ArmRightLower;
            armRightUpper.sharedMesh = config.ArmRightUpper;
        }

        private void ApplyHandsItem(CustomizationHandsItemConfig config)
        {
            handLeft.sharedMesh = config.HandLeft;
            handRight.sharedMesh = config.HandRight;
        }

        private void ApplyLegsItem(CustomizationLegsItemConfig config)
        {
            hips.sharedMesh = config.Hips;

            legLeftLower.sharedMesh = config.LegLeftLower;
            legLeftUpper.sharedMesh = config.LegLeftUpper;

            legRightLower.sharedMesh = config.LegRightLower;
            legRightUpper.sharedMesh = config.LegRightUpper;
        }

        private void ApplyFootsItem(CustomizationFootsItemConfig config)
        {
            footLeft.sharedMesh = config.FootLeft;
            footRight.sharedMesh = config.FootRight;
        }

        private void ApplyFullSet(CustomizationFullSetItemConfig config)
        {
            head.sharedMesh = config.Head;

            spine.sharedMesh = config.Spine;
            chest.sharedMesh = config.Chest;
            armLeftLower.sharedMesh = config.ArmLeftLower;
            armLeftUpper.sharedMesh = config.ArmLeftUpper;
            armRightLower.sharedMesh = config.ArmRightLower;
            armRightUpper.sharedMesh = config.ArmRightUpper;

            handLeft.sharedMesh = config.HandLeft;
            handRight.sharedMesh = config.HandRight;

            hips.sharedMesh = config.Hips;
            legLeftLower.sharedMesh = config.LegLeftLower;
            legLeftUpper.sharedMesh = config.LegLeftUpper;
            legRightLower.sharedMesh = config.LegRightLower;
            legRightUpper.sharedMesh = config.LegRightUpper;

            footLeft.sharedMesh = config.FootLeft;
            footRight.sharedMesh = config.FootRight;
        }
    }
}