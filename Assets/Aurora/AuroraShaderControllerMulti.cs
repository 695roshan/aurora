using UnityEngine;

public class AuroraShaderControllerMulti : MonoBehaviour
{
    public AuroraAudioInputMulti audioInput;
    public Material auroraMaterial;

    private float smoothIntensity;
    private float smoothNoiseScale;
    private float smoothBlend;
    private float smoothDistortX, smoothDistortY;
    private float smoothGradientOffset;

    private float starTwinkleCooldown = 0f;
    private bool starIsBright = false;

    void Start()
    {
        auroraMaterial.SetFloat("_GradientOffset", -0.22f);
        auroraMaterial.SetFloat("_AuroraPower", 4f);
        auroraMaterial.SetFloat("_AuroraNoiseScale", 500f);
        auroraMaterial.SetFloat("_AuroraIntensity", 5f);
        auroraMaterial.SetFloat("_AuroraBlend", 2.5f);
        auroraMaterial.SetFloat("_AuroraTwirlStrength", 0.85f);
        auroraMaterial.SetVector("_AuroraTwirlOffset", new Vector2(20f, 20f));
    }

    void Update()
    {
        float vocalAmp = audioInput.GetChannelAmplitude("vocals");
        float guitarAmp = audioInput.GetChannelAmplitude("guitar");
        float hornAmp = audioInput.GetChannelAmplitude("horns");
        float bassAmp = audioInput.GetChannelAmplitude("bass");
        float drumsAmp = audioInput.GetChannelAmplitude("drums");
        float padAmp = audioInput.GetChannelAmplitude("pad"); // Unused, but left in for reference

        // === BASS → BLEND HEIGHT (2–6) ===
        float targetBlend = Mathf.Lerp(2f, 6f, bassAmp);
        smoothBlend = Mathf.Lerp(smoothBlend, targetBlend, Time.deltaTime * 1.2f);
        auroraMaterial.SetFloat("_AuroraBlend", smoothBlend);

        // === VOCALS → INTENSITY (5–20) ===
        float targetIntensity = Mathf.Lerp(5f, 20f, vocalAmp);
        smoothIntensity = Mathf.Lerp(smoothIntensity, targetIntensity, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraIntensity", smoothIntensity);

        // === BASS+ DRUMS→ STAR POWER (discrete toggle between 45 and 50) ===
        if (Mathf.Clamp01((bassAmp + drumsAmp)*0.5f) > 0.5f && starTwinkleCooldown <= 0f)
        {
            starIsBright = !starIsBright;
            starTwinkleCooldown = 0.5f; // 500ms toggle interval
        }
        starTwinkleCooldown -= Time.deltaTime;
        auroraMaterial.SetFloat("_StarPower", starIsBright ? 45f : 50f);

        // === DRUMS → DISTORTION (Y: 10–15), X stays fixed at 10 ===
        float targetDistort = Mathf.Lerp(10f, 15f, drumsAmp);
        smoothDistortY = Mathf.Lerp(smoothDistortY, targetDistort, Time.deltaTime * 6f);
        auroraMaterial.SetVector("_AuroraDistortMinMax", new Vector2(10f, smoothDistortY));

        // === HORNS → AURORA POWER (inverted: 0 → 5, 1 → 2.5) ===
        float targetPower = Mathf.Lerp(5f, 2.5f, Mathf.Clamp01(hornAmp));
        float currentPower = auroraMaterial.GetFloat("_AuroraPower");
        float smoothedPower = Mathf.Lerp(currentPower, targetPower, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraPower", smoothedPower);

        // === GUITAR + HORNS → NOISE SCALE (300–500) ===
        float detailEnergy = Mathf.Clamp01((guitarAmp + hornAmp) * 0.5f);
        float targetNoise = Mathf.Lerp(300f, 500f, detailEnergy);
        float currentNoise = auroraMaterial.GetFloat("_AuroraNoiseScale");
        smoothNoiseScale = Mathf.Lerp(currentNoise, targetNoise, Time.deltaTime * 6f); // Faster interpolation
        auroraMaterial.SetFloat("_AuroraNoiseScale", smoothNoiseScale);

        // === GUITAR → GRADIENT OFFSET (-1 to 0) ===
        float targetGradientOffset = Mathf.Lerp(-1f, 0f, guitarAmp);
        smoothGradientOffset = Mathf.Lerp(smoothGradientOffset, targetGradientOffset, Time.deltaTime * 5f);
        auroraMaterial.SetFloat("_GradientOffset", smoothGradientOffset);
    }
}
