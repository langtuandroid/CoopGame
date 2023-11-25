using System;
using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.Data.Colliders;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public class OutsideChunk : IChunk
{
    public int HeightLevel;
    public OutsideChunkFillData FillData { get; }

    public OutsideChunk(int heightLevel, OutsideChunkFillData fillData)
    {
        FillData = fillData;
        HeightLevel = heightLevel;
    }

    public void AddChunkNavMesh(Vector2 position, float chunkSize, Polygon polygon)
    {
        if (HeightLevel > 1) return;

        var pointsList = ListPool<Vector2>.Get();

        switch (FillData.centerType)
        {
            case ChunkCenterType.None:
                break;
            case ChunkCenterType.LeftTop:
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.RightTop:
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.RightBottom:
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.LeftBottom:
                pointsList.Add(new Vector2(position.x, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));

                polygon.Add(pointsList);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ListPool<Vector2>.Release(pointsList);
    }

    public void AddDecoration(DecorationConfig decorationConfig, Vector2 Position)
    {
        throw new NotImplementedException();
    }

    public DecorationConfig? GetDecorationConfig()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetDecorationPosition()
    {
        throw new NotImplementedException();
    }

    public void SetOccupiedByDecoration(bool occupied)
    {
        throw new NotImplementedException();
    }

    public bool CanAddDecoration()
    {
        return false;
    }

    public void GetColliders(
        Vector2 chunkPosition,
        LevelGenerationConfig levelGenerationConfig,
        List<ColliderData> colliders
    )
    {
        var centerType = FillData.centerType;
        var chunkSize = levelGenerationConfig.ChunkSize;

        if (centerType != ChunkCenterType.None)
        {
            var colliderSize = new Vector2((float)Math.Sqrt(chunkSize * chunkSize + chunkSize * chunkSize), 1f);
            var centerOffset = colliderSize.y / 2f * (float)Math.Cos(Mathf.Deg2Rad * 45);

            var colliderData = centerType switch
            {
                ChunkCenterType.LeftTop => new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(centerOffset, -centerOffset),
                    Rotation = -45
                },
                ChunkCenterType.RightTop => new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(-centerOffset, -centerOffset),
                    Rotation = 45
                },
                ChunkCenterType.RightBottom => new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(-centerOffset, centerOffset),
                    Rotation = -45
                },
                ChunkCenterType.LeftBottom => new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(centerOffset, centerOffset),
                    Rotation = 45
                }
            };
            colliders.Add(colliderData);
        }
        else
        {
            var colliderSize = new Vector2(chunkSize, 1f);
            var positionOffset = (chunkSize - colliderSize.y) / 2f;
            if (FillData.TopSide)
            {
                colliders.Add(new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(0, positionOffset),
                    Rotation = 0
                });
            }

            if (FillData.RightSide)
            {
                colliders.Add(new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition + new Vector2(positionOffset, 0),
                    Rotation = 90
                });
            }

            if (FillData.BottomSide)
            {
                colliders.Add(new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition - new Vector2(0, positionOffset),
                    Rotation = 0
                });
            }

            if (FillData.LeftSide)
            {
                colliders.Add(new ColliderData
                {
                    Info = new ColliderInfo
                    {
                        Type = ColliderType.BOX,
                        Size = colliderSize
                    },
                    Position = chunkPosition - new Vector2(positionOffset, 0),
                    Rotation = 90
                });
            }
        }
    }
}
}