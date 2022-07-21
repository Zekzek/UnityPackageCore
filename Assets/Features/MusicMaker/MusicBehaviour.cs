using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.MusicMaker
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicBehaviour : MonoBehaviour
    {
        public static readonly int SAMPLE_FREQUENCY = 44100;
        private const float SPEED_CHANGE = 1.01f;
        private const float MAX_TEMPO_MODIFIER = 2f;
        private static float tempoModifier = 1f;

        public string songID;

        private AudioSource source;
        private float nextNoteTime = 1f;
        private SongData song;

        private IEnumerator<ChordData> chords;

        public static void SpeedUp()
        {
            tempoModifier = Mathf.Min(SPEED_CHANGE * tempoModifier, MAX_TEMPO_MODIFIER);
        }

        public static void SlowDown()
        {
            tempoModifier = Mathf.Max(tempoModifier / SPEED_CHANGE, 1f);
        }

        public static void ResetTempo()
        {
            tempoModifier = 1f;
        }

        private void Start()
        {
            source = GetComponent<AudioSource>();
            song = JsonContent.ContentUtil.LoadData<SongData>(songID);
            song.Init();
            chords = song.Chords;
        }

        // An ocateve is a pair of pitches with a 2:1 frequency ratio. They span 12 notes(semitones).
        // A fifth is a pair of pitches with a 3:2 frequency ratio. They span 7 notes(semitones). Diminished fifths span 6. Augmented fifths span 8.
        // A fourth is a pair of pitches with a 4:3 ratio frequency ratio. They span 5 note(semitones). Diminished fourths span 4. Augmented fourths span 6. 

        // 
        int[] circleOfFifths = new int[] { 40, 47, 42, 49, 44, 51, 46, 53, 48, 55, 50, 57 };
        int note = 0;
        private void Update()
        {
            if (nextNoteTime <= 0) {
                //Debug.Log("Play " + note + ": " + NoteData.GetFrequency(note));
                PlayNote(circleOfFifths[note]);

                //PlayNextNote();
                //if (++note >= circleOfFifths.Length) { note = 0; }
            }
            nextNoteTime -= Time.deltaTime * tempoModifier;
        }

        int key = 1;
        private void PlayNote(float key)
        {
            float duration = 0.2f;
            AudioClip noteClip = MusicUtil.Instance.GenerateSinNote(NoteData.GetFrequency(key), duration, 0.4f);
            source.PlayOneShot(noteClip, 1f);
            nextNoteTime += duration * 4;
        }

        private void PlayNextNote()
        {
            if (!chords.MoveNext()) {
                chords.Reset();
                chords.MoveNext();
            }

            float duration = (60f / song.BeatsPerMinute) * chords.Current.Duration / tempoModifier;

            foreach (NoteData note in chords.Current.Notes) {
                AudioClip noteClip = MusicUtil.Instance.GenerateSinNote(note.Frequency, duration, song.NoteFade);
                source.PlayOneShot(noteClip, 1f / chords.Current.Notes.Length);
            }

            nextNoteTime += duration;
        }

#if UNITY_EDITOR
        private string songJson;
        public string SongJson {
            get { return songJson; }
            set {
                songJson = value;
                source = GetComponent<AudioSource>();
                song = JsonUtility.FromJson<SongData>(songJson);
                song.Init();
                chords = song.Chords;
            }
        }
#endif
    }
}