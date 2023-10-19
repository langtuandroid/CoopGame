using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Customization.Combiner;
using Main.Scripts.Customization.Configs;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            if (customizationData.fullSetId >= 0)
            {
                ApplyFullSet(
                        bank.FullSetConfigs.GetCustomizationConfig(customizationData.fullSetId),
                        combineMeshes,
                        cancellationToken
                    )
                    .ContinueWith(() => { CombineMeshes(combineMeshes); });
                return;
            }

            UniTask.WhenAll(
                ApplyHeadItem(
                    bank.HeadConfigs.GetCustomizationConfig(customizationData.headId),
                    combineMeshes,
                    cancellationToken
                ),
                ApplyBodyItem(
                    bank.BodyConfigs.GetCustomizationConfig(customizationData.bodyId),
                    combineMeshes,
                    cancellationToken
                ),
                ApplyHandsItem(
                    bank.HandsConfigs.GetCustomizationConfig(customizationData.handsId),
                    combineMeshes,
                    cancellationToken
                ),
                ApplyLegsItem(
                    bank.LegsConfigs.GetCustomizationConfig(customizationData.legsId),
                    combineMeshes,
                    cancellationToken
                ),
                ApplyFootsItem(
                    bank.FootsConfigs.GetCustomizationConfig(customizationData.footsId),
                    combineMeshes,
                    cancellationToken
                )
            ).ContinueWith(() => { CombineMeshes(combineMeshes); });
        }

        private async UniTask ApplyHeadItem(
            CustomizationHeadItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var headLoader = Addressables.LoadAssetAsync<Mesh>(config.Head);
            try
            {
                var headMesh = await headLoader.WithCancellation(cancellationToken);
                combineMeshes.Add(new CombineMeshData()
                {
                    transform = head,
                    mesh = headMesh,
                    rotation = Vector3.zero
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(headLoader);
            }
        }

        private async UniTask ApplyBodyItem(
            CustomizationBodyItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var spineLoader = Addressables.LoadAssetAsync<Mesh>(config.Spine);
            var chestLoader = Addressables.LoadAssetAsync<Mesh>(config.Chest);
            var armLeftLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmLeftLower);
            var armLeftUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmLeftUpper);
            var armRightLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmRightLower);
            var armRightUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmRightUpper);

            try
            {
                await UniTask.WhenAll(
                    spineLoader.WithCancellation(cancellationToken),
                    chestLoader.WithCancellation(cancellationToken),
                    armLeftLowerLoader.WithCancellation(cancellationToken),
                    armLeftUpperLoader.WithCancellation(cancellationToken),
                    armRightLowerLoader.WithCancellation(cancellationToken),
                    armRightUpperLoader.WithCancellation(cancellationToken)
                ).ContinueWith(_ =>
                {
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = spine,
                        mesh = spineLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = chest,
                        mesh = chestLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armLeftLower,
                        mesh = armLeftLowerLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armLeftUpper,
                        mesh = armLeftUpperLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armRightLower,
                        mesh = armRightLowerLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armRightUpper,
                        mesh = armRightUpperLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(spineLoader);
                Addressables.Release(chestLoader);
                Addressables.Release(armLeftLowerLoader);
                Addressables.Release(armLeftUpperLoader);
                Addressables.Release(armRightLowerLoader);
                Addressables.Release(armRightUpperLoader);
            }
        }

        private async UniTask ApplyHandsItem(
            CustomizationHandsItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var handLeftLoader = Addressables.LoadAssetAsync<Mesh>(config.HandLeft);
            var handRightLoader = Addressables.LoadAssetAsync<Mesh>(config.HandRight);

            try
            {
                await UniTask.WhenAll(
                    handLeftLoader.WithCancellation(cancellationToken),
                    handRightLoader.WithCancellation(cancellationToken)
                ).ContinueWith(_ =>
                {
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = handLeft,
                        mesh = handLeftLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = handRight,
                        mesh = handRightLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(handLeftLoader);
                Addressables.Release(handRightLoader);
            }
        }

        private async UniTask ApplyLegsItem(
            CustomizationLegsItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var hipsLoader = Addressables.LoadAssetAsync<Mesh>(config.Hips);
            var legLeftLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.LegLeftLower);
            var legLeftUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.LegLeftUpper);
            var legRightLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.LegRightLower);
            var legRightUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.LegRightUpper);

            try
            {
                await UniTask.WhenAll(
                    hipsLoader.WithCancellation(cancellationToken),
                    legLeftLowerLoader.WithCancellation(cancellationToken),
                    legLeftUpperLoader.WithCancellation(cancellationToken),
                    legRightLowerLoader.WithCancellation(cancellationToken),
                    legRightUpperLoader.WithCancellation(cancellationToken)
                ).ContinueWith(_ =>
                {
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = hips,
                        mesh = hipsLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legLeftLower,
                        mesh = legLeftLowerLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legLeftUpper,
                        mesh = legLeftUpperLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legRightLower,
                        mesh = legRightLowerLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legRightUpper,
                        mesh = legRightUpperLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(hipsLoader);
                Addressables.Release(legLeftLowerLoader);
                Addressables.Release(legLeftUpperLoader);
                Addressables.Release(legRightLowerLoader);
                Addressables.Release(legRightUpperLoader);
            }
        }

        private async UniTask ApplyFootsItem(
            CustomizationFootsItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var footLeftLoader = Addressables.LoadAssetAsync<Mesh>(config.FootLeft);
            var footRightLoader = Addressables.LoadAssetAsync<Mesh>(config.FootRight);

            try
            {
                await UniTask.WhenAll(
                    footLeftLoader.WithCancellation(cancellationToken),
                    footRightLoader.WithCancellation(cancellationToken)
                ).ContinueWith(_ =>
                {
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = footLeft,
                        mesh = footLeftLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = footRight,
                        mesh = footRightLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(footLeftLoader);
                Addressables.Release(footRightLoader);
            }
        }

        private async UniTask ApplyFullSet(
            CustomizationFullSetItemConfig config,
            List<CombineMeshData> combineMeshes,
            CancellationToken cancellationToken
        )
        {
            var headLoader = Addressables.LoadAssetAsync<Mesh>(config.Head);

            var spineLoader = Addressables.LoadAssetAsync<Mesh>(config.Spine);
            var chestLoader = Addressables.LoadAssetAsync<Mesh>(config.Chest);
            var armLeftLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmLeftLower);
            var armLeftUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmLeftUpper);
            var armRightLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmRightLower);
            var armRightUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.ArmRightUpper);

            var handLeftLoader = Addressables.LoadAssetAsync<Mesh>(config.HandLeft);
            var handRightLoader = Addressables.LoadAssetAsync<Mesh>(config.HandRight);

            var hipsLoader = Addressables.LoadAssetAsync<Mesh>(config.Hips);
            var legLeftLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.LegLeftLower);
            var legLeftUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.LegLeftUpper);
            var legRightLowerLoader = Addressables.LoadAssetAsync<Mesh>(config.LegRightLower);
            var legRightUpperLoader = Addressables.LoadAssetAsync<Mesh>(config.LegRightUpper);

            var footLeftLoader = Addressables.LoadAssetAsync<Mesh>(config.FootLeft);
            var footRightLoader = Addressables.LoadAssetAsync<Mesh>(config.FootRight);

            try
            {
                await UniTask.WhenAll(
                    headLoader.WithCancellation(cancellationToken),
                    spineLoader.WithCancellation(cancellationToken),
                    chestLoader.WithCancellation(cancellationToken),
                    armLeftLowerLoader.WithCancellation(cancellationToken),
                    armLeftUpperLoader.WithCancellation(cancellationToken),
                    armRightLowerLoader.WithCancellation(cancellationToken),
                    armRightUpperLoader.WithCancellation(cancellationToken),
                    handLeftLoader.WithCancellation(cancellationToken),
                    handRightLoader.WithCancellation(cancellationToken),
                    hipsLoader.WithCancellation(cancellationToken),
                    legLeftLowerLoader.WithCancellation(cancellationToken),
                    legLeftUpperLoader.WithCancellation(cancellationToken),
                    legRightLowerLoader.WithCancellation(cancellationToken),
                    legRightUpperLoader.WithCancellation(cancellationToken),
                    footLeftLoader.WithCancellation(cancellationToken),
                    footRightLoader.WithCancellation(cancellationToken)
                ).ContinueWith(_ =>
                {
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = head,
                        mesh = headLoader.Result,
                        rotation = Vector3.zero
                    });

                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = spine,
                        mesh = spineLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = chest,
                        mesh = chestLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armLeftLower,
                        mesh = armLeftLowerLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armLeftUpper,
                        mesh = armLeftUpperLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armRightLower,
                        mesh = armRightLowerLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = armRightUpper,
                        mesh = armRightUpperLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });

                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = handLeft,
                        mesh = handLeftLoader.Result,
                        rotation = new Vector3(0, -90, -90)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = handRight,
                        mesh = handRightLoader.Result,
                        rotation = new Vector3(0, 90, 90)
                    });

                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = hips,
                        mesh = hipsLoader.Result,
                        rotation = Vector3.zero
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legLeftLower,
                        mesh = legLeftLowerLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legLeftUpper,
                        mesh = legLeftUpperLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legRightLower,
                        mesh = legRightLowerLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = legRightUpper,
                        mesh = legRightUpperLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });

                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = footLeft,
                        mesh = footLeftLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                    combineMeshes.Add(new CombineMeshData()
                    {
                        transform = footRight,
                        mesh = footRightLoader.Result,
                        rotation = new Vector3(0, 0, 180)
                    });
                });
            }
            catch (OperationCanceledException ex) { }
            finally
            {
                Addressables.Release(headLoader);
                Addressables.Release(spineLoader);
                Addressables.Release(chestLoader);
                Addressables.Release(armLeftLowerLoader);
                Addressables.Release(armLeftUpperLoader);
                Addressables.Release(armRightLowerLoader);
                Addressables.Release(armRightUpperLoader);
                Addressables.Release(handLeftLoader);
                Addressables.Release(handRightLoader);
                Addressables.Release(hipsLoader);
                Addressables.Release(legLeftLowerLoader);
                Addressables.Release(legLeftUpperLoader);
                Addressables.Release(legRightLowerLoader);
                Addressables.Release(legRightUpperLoader);
                Addressables.Release(footLeftLoader);
                Addressables.Release(footRightLoader);
            }
        }

        private void CombineMeshes(List<CombineMeshData> combineMeshes)
        {
            var rootTransformMatrix = skinnedMeshRenderer.transform.localToWorldMatrix;

            var skinnedMesh = SkinnedMeshCombiner.Combine(rootTransformMatrix, combineMeshes);

            skinnedMeshRenderer.bones = combineMeshes.Map(combineMesh => combineMesh.transform);
            skinnedMeshRenderer.sharedMesh = skinnedMesh;
        }
    }
}