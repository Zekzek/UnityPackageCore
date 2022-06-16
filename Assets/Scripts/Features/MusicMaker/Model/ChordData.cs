using System;
using UnityEngine;

namespace Zekzek.MusicMaker
{
    [Serializable]
    public class ChordData

    {
        /*****   JSON INTERFACE   *****/
        [SerializeField]
        private float duration;

        [SerializeField]
        private int noteOffset;

        [SerializeField]
        private bool rest;
        /***** END JSON INTERFACE *****/

        public enum CHORD_TYPE { NONE, TRIAD, WIDE_TRIAD }

        public static NoteData Key { get; set; }
        public static CHORD_TYPE Type { get; set; }

        public float Duration { get { return Mathf.Max(1f / 64f, duration); } }

        public NoteData[] Notes {
            get {
                if (rest)
                    return new NoteData[] { NoteData.REST };
                else if (Type == CHORD_TYPE.NONE)
                    return new NoteData[] { Key.GenerateOffsetNote(noteOffset) };
                else if (Type == CHORD_TYPE.TRIAD)
                    return new NoteData[] { Key.GenerateOffsetNote(noteOffset), Key.GenerateOffsetNote(noteOffset + 2), Key.GenerateOffsetNote(noteOffset + 4) };
                else if (Type == CHORD_TYPE.WIDE_TRIAD)
                    return new NoteData[] { Key.GenerateOffsetNote(noteOffset), Key.GenerateOffsetNote(noteOffset + 9), Key.GenerateOffsetNote(noteOffset + 18) };
                else
                    return new NoteData[] { NoteData.REST };
            }
        }
    }
}