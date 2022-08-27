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
            InternalState = GetUint(key1, key2, key3, seed, offset + InternalState);
            return InternalState / (float)uint.MaxValue;
        }
    }
}