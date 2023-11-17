using Main.Scripts.LevelGeneration.Chunk;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public class HillOutsideChunkController : MonoBehaviour
{
    [SerializeField]
    private float chunkSize;
    [SerializeField]
    private float cornerSize;
    [SerializeField]
    private float heightStep;
    [SerializeField]
    private GameObject floor = null!;
    [SerializeField]
    private GameObject sideFlat = null!;
    [SerializeField]
    private GameObject cornerFlat = null!;
    [SerializeField]
    private GameObject cornerInner = null!;
    [SerializeField]
    private GameObject cornerOuter = null!;
    [SerializeField]
    private GameObject centerInner = null!;

    public void Init(OutsideChunk outsideChunk, ChunkConnectionType connectionTypes)
    {
        var hasTopSide = connectionTypes.HasFlag(ChunkConnectionType.TopSide);
        var hasRightSide = connectionTypes.HasFlag(ChunkConnectionType.RightSide);
        var hasBottomSide = connectionTypes.HasFlag(ChunkConnectionType.BottomSide);
        var hasLeftSide = connectionTypes.HasFlag(ChunkConnectionType.LeftSide);
        var hasRightTopCorner = connectionTypes.HasFlag(ChunkConnectionType.RightTopCorner);
        var hasRightBottomCorner = connectionTypes.HasFlag(ChunkConnectionType.RightBottomCorner);
        var hasLeftBottomCorner = connectionTypes.HasFlag(ChunkConnectionType.LeftBottomCorner);
        var hasLeftTopCorner = connectionTypes.HasFlag(ChunkConnectionType.LeftTopCorner);

        var hasLeftTopCenter = TrySpawnCenterInner(
            Quaternion.LookRotation(Vector3.back, Vector3.up),
            hasTopSide,
            hasLeftSide,
            hasRightSide,
            hasBottomSide,
            hasRightBottomCorner
        );
        var hasRightTopCenter = TrySpawnCenterInner(
            Quaternion.LookRotation(Vector3.left, Vector3.up),
            hasRightSide,
            hasTopSide,
            hasBottomSide,
            hasLeftSide,
            hasLeftBottomCorner
        );
        var hasRightBottomCenter = TrySpawnCenterInner(
            Quaternion.identity,
            hasBottomSide,
            hasRightSide,
            hasLeftSide,
            hasTopSide,
            hasLeftTopCorner
        );
        var hasLeftBottomCenter = TrySpawnCenterInner(
            Quaternion.LookRotation(Vector3.right, Vector3.up),
            hasLeftSide,
            hasBottomSide,
            hasTopSide,
            hasRightSide,
            hasRightTopCorner
        );

        var hasAnyCenterInner = hasLeftTopCenter || hasRightTopCenter || hasRightBottomCenter || hasLeftBottomCenter;

        var sideOffset = (chunkSize - cornerSize) / 2;

        if (!hasAnyCenterInner)
        {
            SpawnSide(
                new Vector3(0, 0, sideOffset),
                Quaternion.LookRotation(Vector3.back, Vector3.up),
                hasTopSide
            );
            SpawnSide(
                new Vector3(sideOffset, 0, 0),
                Quaternion.LookRotation(Vector3.left, Vector3.up),
                hasRightSide
            );
            SpawnSide(
                new Vector3(0, 0, -sideOffset),
                Quaternion.identity,
                hasBottomSide
            );
            SpawnSide(
                new Vector3(-sideOffset, 0, 0),
                Quaternion.LookRotation(Vector3.right, Vector3.up),
                hasLeftSide
            );
        }


        if (!hasAnyCenterInner || hasRightBottomCenter)
        {
            SpawnCorner(
                new Vector3(-sideOffset, 0, sideOffset),
                Quaternion.LookRotation(Vector3.back, Vector3.up),
                hasTopSide,
                hasLeftSide,
                hasLeftTopCorner
            );
        }

        if (!hasAnyCenterInner || hasLeftBottomCenter)
        {
            SpawnCorner(
                new Vector3(sideOffset, 0, sideOffset),
                Quaternion.LookRotation(Vector3.left, Vector3.up),
                hasRightSide,
                hasTopSide,
                hasRightTopCorner
            );
        }

        if (!hasAnyCenterInner || hasLeftTopCenter)
        {
            SpawnCorner(
                new Vector3(sideOffset, 0, -sideOffset),
                Quaternion.identity,
                hasBottomSide,
                hasRightSide,
                hasRightBottomCorner
            );
        }

        if (!hasAnyCenterInner || hasRightTopCenter)
        {
            SpawnCorner(
                new Vector3(-sideOffset, 0, -sideOffset),
                Quaternion.LookRotation(Vector3.right, Vector3.up),
                hasLeftSide,
                hasBottomSide,
                hasLeftBottomCorner
            );
        }

        if (!hasAnyCenterInner)
        {
            SpawnFloor(
                Vector3.zero,
                chunkSize - cornerSize * 2,
                chunkSize - cornerSize * 2
            );

            if (!hasTopSide)
            {
                SpawnFloor(
                    new Vector3(0, 0, sideOffset),
                    chunkSize - cornerSize * 2,
                    cornerSize
                );
            }

            if (!hasRightSide)
            {
                SpawnFloor(
                    new Vector3(sideOffset, 0, 0),
                    cornerSize,
                    chunkSize - cornerSize * 2
                );
            }

            if (!hasBottomSide)
            {
                SpawnFloor(
                    new Vector3(0, 0, -sideOffset),
                    chunkSize - cornerSize * 2,
                    cornerSize
                );
            }

            if (!hasLeftSide)
            {
                SpawnFloor(
                    new Vector3(-sideOffset, 0, 0),
                    cornerSize,
                    chunkSize - cornerSize * 2
                );
            }

            if (!hasTopSide && !hasLeftSide && !hasLeftTopCorner)
            {
                SpawnFloor(
                    new Vector3(-sideOffset, 0, sideOffset),
                    cornerSize,
                    cornerSize
                );
            }

            if (!hasTopSide && !hasRightSide && !hasRightTopCorner)
            {
                SpawnFloor(
                    new Vector3(sideOffset, 0, sideOffset),
                    cornerSize,
                    cornerSize
                );
            }

            if (!hasBottomSide && !hasRightSide && !hasRightBottomCorner)
            {
                SpawnFloor(
                    new Vector3(sideOffset, 0, -sideOffset),
                    cornerSize,
                    cornerSize
                );
            }

            if (!hasBottomSide && !hasLeftSide && !hasLeftBottomCorner)
            {
                SpawnFloor(
                    new Vector3(-sideOffset, 0, -sideOffset),
                    cornerSize,
                    cornerSize
                );
            }
        }

        transform.position += outsideChunk.HeightLevel * heightStep * Vector3.up;
    }


    private bool TrySpawnCenterInner(
        Quaternion rotation,
        bool hasBackLeft,
        bool hasBackRight,
        bool hasForwardLeft,
        bool hasForwardRight,
        bool hasForwardCorner
    )
    {
        if (hasBackLeft && hasBackRight && !hasForwardLeft && !hasForwardRight && !hasForwardCorner)
        {
            Instantiate(
                original: centerInner,
                position: transform.position,
                rotation: rotation,
                parent: transform
            );

            return true;
        }

        return false;
    }

    private void SpawnSide(
        Vector3 positionOffset,
        Quaternion rotation,
        bool hasBack
    )
    {
        var position = transform.position + positionOffset;
        if (hasBack)
        {
            Instantiate(
                original: sideFlat,
                position: position,
                rotation: rotation,
                parent: transform
            );
        }
    }


    private void SpawnCorner(
        Vector3 positionOffset,
        Quaternion rotation,
        bool hasBackLeft,
        bool hasBackRight,
        bool hasBackCorner
    )
    {
        var position = transform.position + positionOffset;
        if (hasBackLeft && hasBackRight)
        {
            Instantiate(
                original: cornerInner,
                position: position,
                rotation: rotation,
                parent: transform
            );
        }
        else if (hasBackLeft)
        {
            Instantiate(
                original: cornerFlat,
                position: position,
                rotation: rotation,
                parent: transform
            );
        }
        else if (hasBackRight)
        {
            Instantiate(
                original: cornerFlat,
                position: position,
                rotation: rotation * Quaternion.Euler(0, -90, 0),
                parent: transform
            );
        }
        else if (hasBackCorner)
        {
            Instantiate(
                original: cornerOuter,
                position: position,
                rotation: rotation,
                parent: transform
            );
        }
    }

    private void SpawnFloor(
        Vector3 positionOffset,
        float scaleX,
        float scaleY
    )
    {
        var position = transform.position + positionOffset;
        var floorObject = Instantiate(
            original: floor,
            position: position,
            rotation: Quaternion.identity,
            parent: transform
        );

        floorObject.transform.localScale = new Vector3(scaleX, 1, scaleY);
    }
}
}