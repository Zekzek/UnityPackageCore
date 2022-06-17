using System;
using UnityEngine;

namespace Zekzek.MusicMaker
{
    [Serializable]
    public class NoteData
    {
        /*****   JSON INTERFACE   *****/
        [SerializeField]
        private string key;

        [SerializeField]
        private int octive;
        /***** END JSON INTERFACE *****/

        public static readonly NoteData REST = new NoteData("REST", 0);
        private static readonly string[] KEYS = new string[] { "C", "D", "E", "F", "G", "A", "B" };
        //https://en.wikipedia.org/wiki/Piano_key_frequencies
        private static readonly float[] FREQUENCIES_AT_OCTIVE_4 = new float[] { 261.6256f, 293.6648f, 329.6276f, 349.2282f, 391.9954f, 440f, 493.8833f };

        public static float GetFrequency(float key)
        {
            return Mathf.Pow(2, (key - 49) / 12f) * 440;
        }

        public float Frequency {
            get {
                int keyIndex = Array.IndexOf(KEYS, key);
                if (keyIndex < 0)
                    return 0;

                float frequency = FREQUENCIES_AT_OCTIVE_4[keyIndex];
                frequency *= Mathf.Pow(2, octive - 4);
                return frequency;
            }
        }

        private NoteData(string key, int octive)
        {
            this.key = key;
            this.octive = octive;
        }

        public NoteData GenerateOffsetNote(int offset)
        {
            return FromKeyIndex(ToKeyIndex() + offset);
        }

        public NoteData GenerateOffsetOctive(int offset)
        {
            return GenerateOffsetNote(offset * KEYS.Length);
        }

        private int ToKeyIndex()
        {
            int keyIndex = Array.IndexOf(KEYS, key);
            if (keyIndex < 0)
                return 0;
            return octive * KEYS.Length + keyIndex;
        }

        private NoteData FromKeyIndex(int keyIndex)
        {
            return new NoteData(KEYS[keyIndex % KEYS.Length], keyIndex / KEYS.Length);
        }
    }
}