using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Customization.Combiner;
using Main.Scripts.Customization.Configs;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Customization
{
    public class CharacterCustomizationSkinned : MonoBehaviour
    {
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer = default!;

        [Header("Don't forget enable Read/Write property in mesh asset")]
        
        [SerializeField]
        private Transform head = default!;

        [SerializeField]
        private Transform spine = default!;
        [SerializeField]
        private Transform chest = default!;
        [SerializeField]
        private Transform armLeftLower = default!;
        [SerializeField]
        private Transform armLeftUpper = default!;
        [SerializeField]
        private Transform armRightLower = default!;
        [SerializeField]
        private Transform armRightUpper = default!;

        [SerializeField]
        private Transform handLeft = default!;
        [SerializeField]
        private Transform handRight = default!;

        [SerializeField]
        private Transform hips = default!;
        [SerializeField]
        private Transform legLeftLower = default!;
        [SerializeField]
        private Transform legLeftUpper = default!;
        [SerializeField]
        private Transform legRightLower = default!;
        [SerializeField]
        private Transform legRightUpper = default!;

        [SerializeField]
        private Transform footLeft = default!;
        [SerializeField]
        private Transform footRight = default!;

        private CustomizationConfigsBank bank = default!;
        
        List<CombineMeshData> combineMeshes = new();

        private void Awake()
        {
            bank = GlobalResources.Instance.ThrowWhenNull().CustomizationConfigsBank;
        }

        public void ApplyCustomizationData(CustomizationData customizationData)
        {
            combineMeshes.Clear();
            
            if (customizationData.fullSetId >= 0)
            {
                ApplyFullSet(bank.FullSetConfigs.GetCustomizationConfig(customizationData.fullSetId), combineMeshes);
                CombineMeshes(combineMeshes);
                return;
            }

            ApplyHeadItem(bank.HeadConfigs.GetCustomizationConfig(customizationData.headId), combineMeshes);
            ApplyBodyItem(bank.BodyConfigs.GetCustomizationConfig(customizationData.bodyId), combineMeshes);
            ApplyHandsItem(bank.HandsConfigs.GetCustomizationConfig(customizationData.handsId), combineMeshes);
            ApplyLegsItem(bank.LegsConfigs.GetCustomizationConfig(customizationData.legsId), combineMeshes);
            ApplyFootsItem(bank.FootsConfigs.GetCustomizationConfig(customizationData.footsId), combineMeshes);

            CombineMeshes(combineMeshes);
        }

        private void ApplyHeadItem(CustomizationHeadItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = head,
                mesh = config.Head,
                rotation = Vector3.zero
            });
        }

        private void ApplyBodyItem(CustomizationBodyItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = spine,
                mesh = config.Spine,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = chest,
                mesh = config.Chest,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armLeftLower,
                mesh = config.ArmLeftLower,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armLeftUpper,
                mesh = config.ArmLeftUpper,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armRightLower,
                mesh = config.ArmRightLower,
                rotation = new Vector3(0, 90, 90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armRightUpper,
                mesh = config.ArmRightUpper,
                rotation = new Vector3(0, 90, 90)
            });
        }

        private void ApplyHandsItem(CustomizationHandsItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = handLeft,
                mesh = config.HandLeft,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = handRight,
                mesh = config.HandRight,
                rotation = new Vector3(0, 90, 90)
            });
        }

        private void ApplyLegsItem(CustomizationLegsItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = hips,
                mesh = config.Hips,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legLeftLower,
                mesh = config.LegLeftLower,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legLeftUpper,
                mesh = config.LegLeftUpper,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legRightLower,
                mesh = config.LegRightLower,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legRightUpper,
                mesh = config.LegRightUpper,
                rotation = new Vector3(0, 0, 180)
            });
        }

        private void ApplyFootsItem(CustomizationFootsItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = footLeft,
                mesh = config.FootLeft,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = footRight,
                mesh = config.FootRight,
                rotation = new Vector3(0, 0, 180)
            });
        }

        private void ApplyFullSet(CustomizationFullSetItemConfig config, List<CombineMeshData> combineMeshes)
        {
            combineMeshes.Add(new CombineMeshData()
            {
                transform = head,
                mesh = config.Head,
                rotation = Vector3.zero
            });

            combineMeshes.Add(new CombineMeshData()
            {
                transform = spine,
                mesh = config.Spine,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = chest,
                mesh = config.Chest,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armLeftLower,
                mesh = config.ArmLeftLower,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armLeftUpper,
                mesh = config.ArmLeftUpper,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armRightLower,
                mesh = config.ArmRightLower,
                rotation = new Vector3(0, 90, 90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = armRightUpper,
                mesh = config.ArmRightUpper,
                rotation = new Vector3(0, 90, 90)
            });

            combineMeshes.Add(new CombineMeshData()
            {
                transform = handLeft,
                mesh = config.HandLeft,
                rotation = new Vector3(0, -90, -90)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = handRight,
                mesh = config.HandRight,
                rotation = new Vector3(0, 90, 90)
            });

            combineMeshes.Add(new CombineMeshData()
            {
                transform = hips,
                mesh = config.Hips,
                rotation = Vector3.zero
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legLeftLower,
                mesh = config.LegLeftLower,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legLeftUpper,
                mesh = config.LegLeftUpper,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legRightLower,
                mesh = config.LegRightLower,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = legRightUpper,
                mesh = config.LegRightUpper,
                rotation = new Vector3(0, 0, 180)
            });

            combineMeshes.Add(new CombineMeshData()
            {
                transform = footLeft,
                mesh = config.FootLeft,
                rotation = new Vector3(0, 0, 180)
            });
            combineMeshes.Add(new CombineMeshData()
            {
                transform = footRight,
                mesh = config.FootRight,
                rotation = new Vector3(0, 0, 180)
            });
        }

        private void CombineMeshes(List<CombineMeshData> combineMeshes)
        {
            var skinnedMesh = SkinnedMeshCombiner.Combine(skinnedMeshRenderer.transform, combineMeshes);
            
            skinnedMeshRenderer.bones = combineMeshes.Map(combineMesh => combineMesh.transform);
            skinnedMeshRenderer.sharedMesh = skinnedMesh;
        }
    }
}