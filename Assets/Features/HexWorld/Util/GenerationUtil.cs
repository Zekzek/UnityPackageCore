using System;
using System.Collections.Generic;
using UnityEngine;
using Zekzek.Combat;

namespace Zekzek.HexWorld
{
    public static class GenerationUtil
    {
        private static List<TerrainType> _terrainTypes = new List<TerrainType>();
        private static int _regionSize;

        public static void Init(int seed, int regionSize, params TerrainType[] terrains)
        {
            if (seed >= 0) { UnityEngine.Random.InitState(seed); }
            if (regionSize >= 0) { _regionSize = regionSize; }
            _terrainTypes.Clear();
            _terrainTypes.AddRange(terrains);
        }

        public static Vector3 GetColor(Vector2Int gridIndex)
        {
            GenerationParams generationParams = CalcGenerationParams(gridIndex.x, gridIndex.y);
            float heightRatio = 0.5f + generationParams.GridHeight / (float)WorldLocation.MAX_HEIGHT / 2f;
            return heightRatio * new Vector3(generationParams[GenerationParamType.Temperature], generationParams[GenerationParamType.Fertility], generationParams[GenerationParamType.Moisture]).normalized;
        }

        public static WorldObject InstantiateEntity(MovementSpeed speed, Vector2Int gridIndex, Vector2Int? facing = null)
        {
            // Ensure a tile has been generated under the entity
            HexWorld.Instance.GetIdsAt(gridIndex);

            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            GenerationParams generationParams = CalcGenerationParams(gridIndex.x, gridIndex.y);
            instance.AddComponent(new LocationComponent(instance.Id, new Vector3Int(gridIndex.x, generationParams.GridHeight, gridIndex.y), facing, speed));
            instance.AddComponent(InstantiateRelationship(generationParams));
            instance.AddComponent(InstantiateStats(generationParams));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        private static RelationshipComponent InstantiateRelationship(GenerationParams generationParams)
        {
            RelationshipComponent relationship = new RelationshipComponent(0);
            relationship.AddDefaultAffinity(RelationshipType.Trust, generationParams[GenerationParamType.Trust]);
            relationship.AddDefaultAffinity(RelationshipType.Affection, generationParams[GenerationParamType.Affection]);
            return relationship;
        }

        private static StatComponent InstantiateStats(GenerationParams generationParams)
        {
            StatComponent stats = new StatComponent(0);
            stats.StatBlock.AddAmount(StatType.Health, 100);
            return stats;
        }


        public static void InstantiateAtGridIndex(int x, int z)
        {
            GenerationParams generationParams = CalcGenerationParams(x, z);
            Vector3Int gridPosition = new Vector3Int(x, generationParams.GridHeight, z);

            InstantiateTile(gridPosition, generationParams);
            CheckInstantiateTileDecoration(gridPosition, generationParams);
            CheckInstantiateTileEntity(gridPosition, generationParams);
        }

        private static void InstantiateTile(Vector3Int gridPosition, GenerationParams generationParams)
        {
            float heightRatio = 0.5f + generationParams.GridHeight / (float)WorldLocation.MAX_HEIGHT / 2f;
            WorldObject tile = new WorldObject(WorldObjectType.Tile);
            tile.AddComponent(new LocationComponent(tile.Id, gridPosition));
            tile.AddComponent(new PlatformComponent(tile.Id, heightRatio * new Vector3(generationParams[GenerationParamType.Temperature], generationParams[GenerationParamType.Fertility], generationParams[GenerationParamType.Moisture]).normalized));
            tile.AddComponent(new TargetableComponent(tile.Id));
            HexWorld.Instance.Add(tile);
        }

        private static void CheckInstantiateTileDecoration(Vector3Int gridPosition, GenerationParams generationParams)
        {
            Vector2Int facing = FacingUtil.GetFacing(generationParams[GenerationParamType.Rotation] * 360);

            float chaosFactor = Noise.Noise.GetPercent(gridPosition.x, gridPosition.z);
            if (chaosFactor * (generationParams[GenerationParamType.Fertility] + generationParams[GenerationParamType.Moisture]) > 0.75f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Bush);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                decoration.AddComponent(new DisplayComponent(decoration.Id, generationParams[GenerationParamType.Quantity], generationParams[GenerationParamType.Color]));
                HexWorld.Instance.Add(decoration);
            } else if (chaosFactor * (generationParams[GenerationParamType.Fertility] + generationParams[GenerationParamType.Moisture]) < 0.25f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Rock);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                decoration.AddComponent(new DisplayComponent(decoration.Id, generationParams[GenerationParamType.Quantity], generationParams[GenerationParamType.Color]));
                HexWorld.Instance.Add(decoration);
            }
        }

        private static void CheckInstantiateTileEntity(Vector3Int gridPosition, GenerationParams generationParams)
        {
            float chaosFactor = Noise.Noise.GetPercent(gridPosition.x, gridPosition.z);
            if (chaosFactor * (generationParams[GenerationParamType.Fertility] + generationParams[GenerationParamType.Moisture]) > 1.5f) {
                Vector2Int facing = FacingUtil.GetFacing(generationParams[GenerationParamType.Rotation] * 360);
                InstantiateEntity(new MovementSpeed(1,1,1,1,1,1,1,1,1), new Vector2Int(gridPosition.x, gridPosition.z), facing);
            }
        }

        private static GenerationParams CalcTileGenerationParams(TerrainType terrainType, int x, int z)
        {
            switch (terrainType) {
                case TerrainType.Flat: return new GenerationParams(5, x, z);
                case TerrainType.Chaos: return new GenerationParams(Noise.Noise.GetPercent(x, z) * WorldLocation.MAX_HEIGHT, x, z);
                case TerrainType.Desert: return CalcDesertTerrain(x, z);
                case TerrainType.Forest: return CalcForestTerrain(x, z);
                case TerrainType.Hills: return CalcHillsTerrain(x, z);
            }
            throw new System.Exception("No tile generation logic found for " + terrainType);
        }

        private static GenerationParams CalcGenerationParams(int x, int z)
        {
            // Convert to region space
            float regionX = x / (float)_regionSize;
            int regionXFloor = Mathf.FloorToInt(regionX);
            int regionXCeil = Mathf.CeilToInt(regionX);
            float regionZ = z / (float)_regionSize;
            int regionZFloor = Mathf.FloorToInt(regionZ);
            int regionZCeil = Mathf.CeilToInt(regionZ);

            // Capture regions around the point
            TerrainType lowXLowZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXFloor, 0.3f * regionZFloor) * _terrainTypes.Count)];
            TerrainType lowXHighZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXFloor, 0.3f * regionZCeil) * _terrainTypes.Count)];
            TerrainType highXLowZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXCeil, 0.3f * regionZFloor) * _terrainTypes.Count)];
            TerrainType highXHighZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXCeil, 0.3f * regionZCeil) * _terrainTypes.Count)];            

            // Lerp the lower X terrains, skip the lerp if terrain matches
            GenerationParams lowX;
            if (lowXLowZTerrain == lowXHighZTerrain) {
                lowX = CalcTileGenerationParams(lowXLowZTerrain, x, z);
            } else {
                lowX = GenerationParams.Lerp(
                    CalcTileGenerationParams(lowXLowZTerrain, x, z),
                    CalcTileGenerationParams(lowXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the higher X terrains, skip the lerp if terrain matches
            GenerationParams highX;
            if (highXLowZTerrain == highXHighZTerrain) {
                highX = CalcTileGenerationParams(highXLowZTerrain, x, z);
            } else {
                highX = GenerationParams.Lerp(
                    CalcTileGenerationParams(highXLowZTerrain, x, z),
                    CalcTileGenerationParams(highXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the final reult
            return GenerationParams.Lerp(lowX, highX, CalculateSmoothLerpTime(regionX - regionXFloor));
        }        

        private static GenerationParams CalcDesertTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.06f * x, 0.03f * z),
                        CalcLerpNoise(0.03f * x, 0.06f * z));

            GenerationParams parameters = new GenerationParams(heightNoise * WorldLocation.MAX_HEIGHT, x, z);
            parameters.Increase(GenerationParamType.Temperature);
            parameters.Decrease(GenerationParamType.Moisture, GenerationParamType.Fertility);
            return parameters;
        }

        private static GenerationParams CalcForestTerrain(int x, int z)
        {
            float heightNoise = Combine(0.8f,
                        CalcLerpNoise(0.2f * x, 0.1f * z),
                        CalcLerpNoise(0.1f * x, 0.2f * z),
                        Combine(0.5f,
                            CalcLerpNoise(0.01f * x, 0.03f * z),
                            CalcLerpNoise(0.03f * x, 0.01f * z)));

            GenerationParams parameters = new GenerationParams(heightNoise * WorldLocation.MAX_HEIGHT, x, z);
            parameters.Increase(GenerationParamType.Moisture, GenerationParamType.Fertility);
            return parameters;
        }

        private static GenerationParams CalcHillsTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.04f * x, 0.08f * z),
                        CalcLerpNoise(0.08f * x, 0.04f * z));

            GenerationParams parameters = new GenerationParams(heightNoise * WorldLocation.MAX_HEIGHT, x, z);
            return parameters;
        }

        private static float IncreasePercent(float value) { return 1f - (value * value); }
        private static float DecreasePercent(float value) { return value * value; }

        private static float CalcPercent(int x, int z, int offset, float clusterRatio) {
            float isolateRatio = 1.0001f - (clusterRatio * clusterRatio * clusterRatio);
            return CalcLerpNoise(offset + isolateRatio * x, offset + isolateRatio * z);
        }

        private static float CalcLerpNoise(float x, float z)
        {
            int floorX = Mathf.FloorToInt(x);
            int floorZ = Mathf.FloorToInt(z);

            float lowXLowZ = Noise.Noise.GetPercent(floorX, floorZ);
            float lowXHighZ = Noise.Noise.GetPercent(floorX, floorZ + 1);
            float highXLowZ = Noise.Noise.GetPercent(floorX + 1, floorZ);
            float highXHighZ = Noise.Noise.GetPercent(floorX + 1, floorZ + 1);

            float lowX = Mathf.Lerp(lowXLowZ, lowXHighZ, CalculateSmoothLerpTime(z - floorZ));
            float highX = Mathf.Lerp(highXLowZ, highXHighZ, CalculateSmoothLerpTime(z - floorZ));
            return Mathf.Lerp(lowX, highX, CalculateSmoothLerpTime(x - floorX));
        }

        // https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise
        private static float CalculateSmoothLerpTime(float time)
        {
            return 6 * time * time * time * time * time - 15 * time * time * time * time + 10 * time * time * time;
        }

        // Combine values while working to avoid loosing peaks and valleys
        private static float Combine(float favorExtremes, params float[] values)
        {
            float total = 0;
            float min = 0;
            float max = 1;

            foreach (float value in values) {
                total += value;
                if (value < min) { min = value; }
                if (value > max) { max = value; }
            }

            float extreme = min > 1 - max ? max : min;

            return Mathf.Lerp(total / values.Length, extreme, favorExtremes);
        }

        // Seasons: modify temperature and moisture based on time of year
        // Weather: precipitation, wind, etc with frequency based on local moisture values (windiness?)
        private enum GenerationParamType
        {
            Rotation,
            Temperature,
            Moisture,
            Fertility,
            Trust,
            Affection,
            Quantity,
            Color
        }

        private class GenerationParams
        {
            public float Height { get; set; }
            public int GridHeight => (int)Height;

            private static int parameterCount = Enum.GetNames(typeof(GenerationParamType)).Length;
            private float[] parameters = new float[parameterCount];
            
            private Dictionary<int, float> clusterMap = new Dictionary<int, float>() {
                { (int)GenerationParamType.Rotation, 0.95f },
                { (int)GenerationParamType.Moisture, 0.96f },
                { (int)GenerationParamType.Temperature, 0.97f },
                { (int)GenerationParamType.Fertility, 0.98f },
                { (int)GenerationParamType.Trust, 0.5f },
                { (int)GenerationParamType.Affection, 0.6f },
                { (int)GenerationParamType.Quantity, 0.95f },
                { (int)GenerationParamType.Color, 0.9f },
            };
            
            public float this[GenerationParamType index] { get => parameters[(int)index]; set => parameters[(int)index] = value; }
            
            private GenerationParams() { }

            public GenerationParams(float height, int x, int z)
            {
                Height = height;

                int offset = 0;
                for(int i = 0; i < parameterCount; i++) {
                    parameters[i] = CalcPercent(x, z, offset++, clusterMap.ContainsKey(i) ? clusterMap[i] : 0.5f);
                }
            }

            public void Increase(params GenerationParamType[] types)
            {
                foreach (GenerationParamType type in types) {
                    this[type] = IncreasePercent(this[type]); 
                }
            }

            public void Decrease(params GenerationParamType[] types)
            {
                foreach (GenerationParamType type in types) {
                    this[type] = DecreasePercent(this[type]);
                }
            }

            public static GenerationParams Lerp(GenerationParams a, GenerationParams b, float ratio) {
                GenerationParams combined = new GenerationParams() { Height = Mathf.Lerp(a.Height, b.Height, ratio) };

                for (int i = 0; i < parameterCount; i++) {
                    combined.parameters[i] = Mathf.Lerp(a.parameters[i], b.parameters[i], ratio);
                }

                return combined;
            }
        }
    }
}