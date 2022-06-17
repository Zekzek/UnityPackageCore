using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.MusicMaker
{
    [Serializable]
    public class SongData
    {
        /*****   JSON INTERFACE   *****/
        [SerializeField]
        private float beatsPerBar; //time signature (top)

        [SerializeField]
        private float quarterNotesPerBar; //time signature (bottom)

        [SerializeField]
        private float noteFade;

        [SerializeField]
        private string tempo;

        [SerializeField]
        private NoteData key; //relative key for all chords in the song

        [SerializeField]
        private string chordType; //relative key for all chords in the song

        [SerializeField]
        private ChordData[] chords; //all chords in the song
        /***** END JSON INTERFACE *****/

        public const float TEMPO_LARGHISSIMO = 20;
        public const float TEMPO_GRAVE = 30;
        public const float TEMPO_LARGO = 50;
        public const float TEMPO_ADAGIO = 70;
        public const float TEMPO_MODERATO = 110;
        public const float TEMPO_ALLEGRO = 140;
        public const float TEMPO_PRESTO = 180;

        public float NoteFade { get { return Mathf.Clamp(noteFade, 0.1f, 0.05f); } }

        public float BeatsPerMinute {
            get {
                float baseTempo;
                if ("LARGHISSIMO".Equals(tempo)) baseTempo = TEMPO_LARGHISSIMO;
                else if ("GRAVE".Equals(tempo)) baseTempo = TEMPO_GRAVE;
                else if ("LARGO".Equals(tempo)) baseTempo = TEMPO_LARGO;
                else if ("ADAGIO".Equals(tempo)) baseTempo = TEMPO_ADAGIO;
                else if ("MODERATO".Equals(tempo)) baseTempo = TEMPO_MODERATO;
                else if ("ALLEGRO".Equals(tempo)) baseTempo = TEMPO_ALLEGRO;
                else if ("PRESTO".Equals(tempo)) baseTempo = TEMPO_PRESTO;
                else baseTempo = TEMPO_MODERATO;

                return baseTempo * beatsPerBar / quarterNotesPerBar;
            }
        }

        public IEnumerator<ChordData> Chords { get { return ((IEnumerable<ChordData>)chords).GetEnumerator(); } }

        public void Init()
        {
            ChordData.Key = key;
            if ("TRIAD".Equals(chordType)) ChordData.Type = ChordData.CHORD_TYPE.TRIAD;
            if ("WIDE_TRIAD".Equals(chordType)) ChordData.Type = ChordData.CHORD_TYPE.WIDE_TRIAD;
            else ChordData.Type = ChordData.CHORD_TYPE.NONE;
        }
    }
}