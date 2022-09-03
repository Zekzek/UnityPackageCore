using System;

namespace Zekzek.Noise
{
    public static class Noise
    {
        private const uint BIT_NOISE_1 = 0x68E31DA4;
        private const uint BIT_NOISE_2 = 0xB5297A4D;
        private const uint BIT_NOISE_3 = 0x1B56C4E9;
        private const uint LARGE_PRIME_1 = 198491317;
        private const uint LARGE_PRIME_2 = 6542989;

        private static uint InternalState;

        // Based on Squirrel Eiserloh's 'Noise-Based RNG' GDC presentation available at https://www.youtube.com/watch?v=LWFzPP8ZbdU
        public static uint GetUint(int key1, int key2 = 0, int key3 = 0, uint seed = 0, uint offset = 0)
        {
            uint mangledBits = (uint)(key1 + key2 * LARGE_PRIME_1 + key3 * LARGE_PRIME_2);
            mangledBits *= BIT_NOISE_1;
            mangledBits += seed;
            mangledBits ^= (mangledBits >> 8);
            mangledBits += BIT_NOISE_2;
            mangledBits += offset;
            mangledBits ^= (mangledBits >> 8);
            mangledBits *= BIT_NOISE_3;
            mangledBits ^= (mangledBits >> 8);
            return mangledBits;
        }

        public static int GetPositiveInt(int key1, int key2 = 0, int key3 = 0, uint seed = 0, uint offset = 0)
        {
            int value = (int)GetUint(key1, key2, key3, seed, offset);
            return value < 0 ? -value : value;
        }

        public static float SteppedPercent(int stepSize, int rawX, int rawZ, uint offset)
        {
            float x = rawX / (float)stepSize;
            float z = rawZ / (float)stepSize;
            int floorX = (int)Math.Floor(x);
            int floorZ = (int)Math.Floor(z);
            float xRatio = x - floorX;
            float zRatio = z - floorZ;

            float floorXFloorZ = GetPercent(floorX, floorZ, seed: offset);
            float floorXCeilZ = GetPercent(floorX, floorZ + 1, seed: offset);
            float ceilXFloorZ = GetPercent(floorX + 1, floorZ, seed: offset);
            float ceilXCeilZ = GetPercent(floorX + 1, floorZ + 1, seed: offset);

            return
                ((1 - xRatio) * (1 - zRatio)) * floorXFloorZ +
                ((1 - xRatio) * zRatio) * floorXCeilZ +
                (xRatio * (1 - zRatio)) * ceilXFloorZ +
                (xRatio * zRatio) * ceilXCeilZ;
        }

        public static float GetPercent(int key1, int key2 = 0, int key3 = 0, uint seed = 0, uint offset = 0)
        {
            return GetUint(key1, key2, key3, seed, offset) / (float)uint.MaxValue;
        }

        public static float ScalePercent(float value, float minPercent, float maxPercent)
        {
            return minPercent + (maxPercent - minPercent) * value;
        }

        public static float Rand(int key1, int key2 = 0, int key3 = 0, uint seed = 0, uint offset = 0)
        {
            InternalState = GetUint(key1, key2, key3, seed, (uint)(offset + InternalState));
            return InternalState / (float)uint.MaxValue;
        }
    }
}