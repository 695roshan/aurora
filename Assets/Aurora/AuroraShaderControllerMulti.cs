using UnityEngine;

public class AuroraShaderControllerMulti : MonoBehaviour
{
    public AuroraAudioInputMulti audioInput;
    public Material auroraMaterial;

    private float smoothIntensity;
    private float smoothNoiseScale;
    private float smoothBlend;
    private float smoothDistortY;
    private float smoothGradientOffset;

    private float starTwinkleCooldown = 0f;
    private bool starIsBright = false;

    void Start()
    {
        smoothDistortY = 10f;
        auroraMaterial.SetFloat("_GradientOffset", -0.22f);
        auroraMaterial.SetFloat("_AuroraPower", 4f);
        auroraMaterial.SetFloat("_AuroraNoiseScale", 100f);
        auroraMaterial.SetFloat("_AuroraIntensity", 5f);
        auroraMaterial.SetFloat("_AuroraBlend", 1.5f);
        auroraMaterial.SetFloat("_AuroraTwirlStrength", 0.85f);
        auroraMaterial.SetVector("_AuroraTwirlOffset", new Vector2(20f, 20f));
        auroraMaterial.SetVector("_AuroraDistortionMinMax", new Vector2(10f, 10f));
    }

    void Update()
    {
        float vocalEnergy = audioInput.GetChannelEnergy("vocals");
        float guitarEnergy = audioInput.GetChannelEnergy("guitar");
        float guitarAmp = audioInput.GetChannelAmplitude("guitar");
        float hornEnergy = audioInput.GetChannelEnergy("horns");
        float bassEnergy = audioInput.GetChannelEnergy("bass");
        float drumsEnergy = audioInput.GetChannelEnergy("drums");
        float padEnergy = audioInput.GetChannelEnergy("pad"); // Unused, but left in for reference

        // === BASS → BLEND HEIGHT (1.5–4) ===
        float targetBlend = Mathf.Lerp(1f, 4f, bassEnergy);
        smoothBlend = Mathf.Lerp(smoothBlend, targetBlend, Time.deltaTime * 1.2f);
        auroraMaterial.SetFloat("_AuroraBlend", smoothBlend);

        // === VOCALS → INTENSITY (5–20) ===
        float targetIntensity = Mathf.Lerp(5f, 20f, vocalEnergy);
        smoothIntensity = Mathf.Lerp(smoothIntensity, targetIntensity, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraIntensity", smoothIntensity);

        // === HORNS → AURORA POWER (inverted: 0 → 5, 1 → 2.5) ===
        float targetPower = Mathf.Lerp(5f, 2.5f, Mathf.Clamp01(hornEnergy));
        float currentPower = auroraMaterial.GetFloat("_AuroraPower");
        float smoothedPower = Mathf.Lerp(currentPower, targetPower, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraPower", smoothedPower);

        // === GUITAR → GRADIENT OFFSET (-1 to 0.1) ===
        float targetGradientOffset = Mathf.Lerp(-1f, 0.1f, guitarAmp);
        smoothGradientOffset = Mathf.Lerp(smoothGradientOffset, targetGradientOffset, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_GradientOffset", smoothGradientOffset);

        // === BASS + DRUMS→ STAR POWER (discrete toggle between 45 and 50) ===
        if (Mathf.Clamp01((bassEnergy + drumsEnergy) * 0.5f) > 0.5f && starTwinkleCooldown <= 0f)
        {
            starIsBright = !starIsBright;
            starTwinkleCooldown = 0.5f; // 500ms toggle interval
        }
        starTwinkleCooldown -= Time.deltaTime;
        auroraMaterial.SetFloat("_StarPower", starIsBright ? 45f : 50f);

        //=== GUITAR + HORNS → NOISE SCALE (100–300) ===
        float detailEnergy = Mathf.Clamp01((guitarEnergy + hornEnergy) * 0.5f);
        float targetNoise = Mathf.Lerp(100f, 300f, detailEnergy);
        float currentNoise = auroraMaterial.GetFloat("_AuroraNoiseScale");
        smoothNoiseScale = Mathf.Lerp(currentNoise, targetNoise, Time.deltaTime * 6f); // Faster interpolation
        auroraMaterial.SetFloat("_AuroraNoiseScale", smoothNoiseScale);

        // === HORNS → DISTORTION (Y: 10–15), X stays fixed at 10 ===
        //float guitarAndDrums = Mathf.Clamp01((guitarEnergy + drumsEnergy) * 0.5f);
        //float targetDistort = Mathf.Lerp(10f, 15f, guitarAndDrums);
        float targetDistort = Mathf.Lerp(10f, 15f, hornEnergy);
        smoothDistortY = Mathf.Lerp(smoothDistortY, targetDistort, Time.deltaTime * 2f);
        auroraMaterial.SetVector("_AuroraDistortionMinMax", new Vector2(10f, smoothDistortY));

        // === GUITAR → SKY COLOR (Green channel from 30 to 70) ===
        //float red = Mathf.Lerp(0.0f, 0.3f, bassAmp);         // Bass adds warmth
        //float green = Mathf.Lerp(5f / 255f, 80f / 255f, guitarAmp);
        ////float blue = Mathf.Lerp(0.3f, 1.0f, guitarEnergy);

        //Color targetColor = new Color(0f, green, 0f);
        //Color currentColor = auroraMaterial.GetColor("_SkyColor");
        //Color smoothColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 2f);
        //auroraMaterial.SetColor("_SkyColor", smoothColor);
    }
}