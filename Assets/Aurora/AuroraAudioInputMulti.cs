using UnityEngine;

[System.Serializable]
public class AudioChannel
{
    public string name;
    public AudioSource source;

    [HideInInspector] public float[] spectrum = new float[1024];
    [HideInInspector] public float[] samples = new float[1024];
    [HideInInspector] public float amplitude;  // RMS amplitude from waveform
    [HideInInspector] public float energy;  // sum of magnitudes across all frequencies —
                                                        // rough proxy for energy in the signal at that frame
                                                        // or the intensity of sound across frequencies.
}

public class AuroraAudioInputMulti : MonoBehaviour
{
    public AudioChannel[] channels;
    [Range(64, 8192)] public int spectrumSize = 1024; // number of samples to divide the total freq into

    private int sampleRate;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    void Update()
    {
        foreach (var channel in channels)
        {
            if (channel.source == null || !channel.source.isPlaying) continue;

            // --- Spectrum Data (Frequency Domain) ---
            if (channel.spectrum == null || channel.spectrum.Length != spectrumSize)
                channel.spectrum = new float[spectrumSize]; // Ensure it exists and has correct size

            channel.source.GetSpectrumData(channel.spectrum, 0, FFTWindow.BlackmanHarris); // fills channel.spectrum
            float sum = 0f;
            for (int i = 0; i < channel.spectrum.Length; i++)
            {
                sum += channel.spectrum[i];
            }
            channel.energy = Mathf.Clamp01(sum * 10f);  // Amplify for better range

            // --- Waveform Data (Time Domain) ---
            if (channel.samples == null || channel.samples.Length != spectrumSize)
                channel.samples = new float[spectrumSize];

            channel.source.GetOutputData(channel.samples, 0);

            // --- RMS Amplitude Calculation ---
            sum = 0f;
            for (int i = 0; i < channel.samples.Length; i++)
            {
                sum += channel.samples[i] * channel.samples[i];
            }
            float rms = Mathf.Sqrt(sum / channel.samples.Length);
            channel.amplitude = Mathf.Clamp01(rms * 10f); // Scale for usability
        }
    }

    public float GetChannelEnergy(string name)
    {
        foreach (var ch in channels)
        {
            if (ch.name.ToLower() == name.ToLower())
                return ch.energy;
        }
        return 0f;
    }

    public float GetChannelAmplitude(string name)
    {
        foreach (var ch in channels)
        {
            if (ch.name.ToLower() == name.ToLower())
                return ch.amplitude;
        }
        return 0f;
    }

    public float[] GetChannelSpectrum(string name)
    {
        foreach (var ch in channels)
        {
            if (ch.name.ToLower() == name.ToLower())
                return ch.spectrum;
        }
        return null;
    }

    public float[] GetCombinedSpectrum()
    {
        if (channels == null || channels.Length == 0) return null;

        int size = spectrumSize;
        float[] combined = new float[size];

        foreach (var channel in channels)
        {
            if (channel.spectrum == null || channel.spectrum.Length != size) continue;

            for (int i = 0; i < size; i++)
            {
                combined[i] += channel.spectrum[i];
            }
        }

        return combined;
    }

    public float[] GetFrequencies()
    {
        float[] freqs = new float[spectrumSize];
        for (int i = 0; i < spectrumSize; i++)
        {
            freqs[i] = i * (sampleRate / 2f) / spectrumSize;
        }
        return freqs;
    }
}
