using UnityEngine;

[System.Serializable]
public class AudioChannel
{
    public string name;
    public AudioSource source;
    [HideInInspector] public float[] spectrum = new float[1024];
    [HideInInspector] public float amplitude;
}

public class AuroraAudioInputMulti : MonoBehaviour
{
    public AudioChannel[] channels;
    [Range(64, 8192)] public int spectrumSize = 1024;

    void Update()
    {
        foreach (var channel in channels)
        {
            if (channel.source == null || !channel.source.isPlaying) continue;

            if (channel.spectrum == null || channel.spectrum.Length != spectrumSize)
            {
                channel.spectrum = new float[spectrumSize]; // Ensure it exists and has correct size
            }
            channel.source.GetSpectrumData(channel.spectrum, 0, FFTWindow.BlackmanHarris);
            float sum = 0f;
            for (int i = 0; i < channel.spectrum.Length; i++)
            {
                sum += channel.spectrum[i];
            }
            channel.amplitude = Mathf.Clamp01(sum * 10f);  // Amplify for better range
        }
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
        return null; // Return null if channel not found
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

        //// Optional: normalize or scale
        //for (int i = 0; i < size; i++)
        //{
        //    combined[i] = Mathf.Clamp01(combined[i] * 2f); // scale if needed
        //}

        return combined;
    }

}
