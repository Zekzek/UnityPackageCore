using System.Collections.Generic;
using UnityEngine;

public class MusicUtil
{
    public static readonly int SAMPLE_FREQUENCY = 44100;

    private static MusicUtil instance;
    public static MusicUtil Instance { get { if (instance == null) { instance = new MusicUtil(); } return instance; } }

    private readonly Dictionary<NoteParameters, AudioClip> generatedNotes = new Dictionary<NoteParameters, AudioClip>();

    public AudioClip GenerateSinNote(float frequency, float duration, float fade)
    {
        return GenerateSinNote(new NoteParameters { Duration = duration, Frequency = frequency, Fade = fade });
    }

    private AudioClip GenerateSinNote(NoteParameters noteParameters)
    {
        if (generatedNotes.ContainsKey(noteParameters)) {
            return generatedNotes[noteParameters];
        }

        float frequency = noteParameters.Frequency;
        float duration = noteParameters.Duration;
        float fade = noteParameters.Fade;
        
        float[] samples = new float[Mathf.RoundToInt(SAMPLE_FREQUENCY * duration)];
        for (int i = 0; i < samples.Length; i++) {
            float fadeMult = 1;
            if (i < samples.Length * fade)
                fadeMult = i / (samples.Length * fade);
            else if (i > samples.Length * (1f - fade))
                fadeMult = (samples.Length - i) / (samples.Length * fade);

            samples[i] = Mathf.Sin(Mathf.PI * 2 * i * frequency / SAMPLE_FREQUENCY) * fadeMult;
        }
        AudioClip clip = AudioClip.Create("Note", samples.Length, 1, SAMPLE_FREQUENCY, false);
        clip.SetData(samples, 0);

        generatedNotes.Add(noteParameters, clip);
        return clip;
    }

    private class NoteParameters
    {
        public float Frequency { get; set; }
        public float Duration { get; set; }
        public float Fade { get; set; }
    }
}