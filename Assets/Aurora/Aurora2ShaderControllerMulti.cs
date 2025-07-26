using UnityEngine;

public class Aurora2ShaderControllerMulti : MonoBehaviour
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
        smoothDistortY=10f;
        //auroraMaterial.SetFloat("_GradientOffset", -0.22f);
        auroraMaterial.SetFloat("_GradientOffset", 0.15f);
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
        float otherEnergy = audioInput.GetChannelEnergy("other");
        float bassEnergy = audioInput.GetChannelEnergy("bass");
        float drumsEnergy = audioInput.GetChannelEnergy("drums");

        // === BASS → BLEND HEIGHT (1.5–4) ===
        float targetBlend = Mathf.Lerp(1f, 4f, bassEnergy);
        smoothBlend = Mathf.Lerp(smoothBlend, targetBlend, Time.deltaTime * 1.2f);
        auroraMaterial.SetFloat("_AuroraBlend", smoothBlend);

        // === VOCALS → INTENSITY (15–40) ===
        float targetIntensity = Mathf.Lerp(15f, 40f, vocalEnergy);
        smoothIntensity = Mathf.Lerp(smoothIntensity, targetIntensity, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraIntensity", smoothIntensity);

        // === BASS → AURORA POWER (inverted: 0 → 5, 1 → 2.5) ===
        float targetPower = Mathf.Lerp(5f, 2.5f, Mathf.Clamp01(bassEnergy));
        float currentPower = auroraMaterial.GetFloat("_AuroraPower");
        float smoothedPower = Mathf.Lerp(currentPower, targetPower, Time.deltaTime * 2f);
        auroraMaterial.SetFloat("_AuroraPower", smoothedPower);

        // === GUITAR → GRADIENT OFFSET (-1 to 0.1) ===
        float targetGradientOffset = Mathf.Lerp(0.15f,0.3f, guitarEnergy);
        smoothGradientOffset = Mathf.Lerp(smoothGradientOffset, targetGradientOffset, Time.deltaTime * 3f);
        auroraMaterial.SetFloat("_GradientOffset", smoothGradientOffset);

        // === BASS + DRUMS → STAR POWER (discrete toggle between 45 and 50) ===
        if (Mathf.Clamp01((bassEnergy + drumsEnergy) *0.5f) > 0.5f && starTwinkleCooldown <= 0f)
        {
            starIsBright = !starIsBright;
            starTwinkleCooldown = 0.4f; // 400ms toggle interval
        }
        starTwinkleCooldown -= Time.deltaTime;
        auroraMaterial.SetFloat("_StarPower", starIsBright ? 45f : 50f);

        //=== BASS + OTHERS → NOISE SCALE (100–300) ===
        float detailEnergy = Mathf.Clamp01((bassEnergy + otherEnergy) * 0.5f);
        float targetNoise = Mathf.Lerp(100f, 300f, detailEnergy);
        float currentNoise = auroraMaterial.GetFloat("_AuroraNoiseScale");
        smoothNoiseScale = Mathf.Lerp(currentNoise, targetNoise, Time.deltaTime * 4f); // Faster interpolation
        auroraMaterial.SetFloat("_AuroraNoiseScale", smoothNoiseScale);

        // === BASS → DISTORTION (Y: 10–15), X stays fixed at 10 ===
        float targetDistort = Mathf.Lerp(10f, 15f, bassEnergy);
        smoothDistortY = Mathf.Lerp(smoothDistortY, targetDistort, Time.deltaTime * 2f);
        auroraMaterial.SetVector("_AuroraDistortionMinMax", new Vector2(10f, smoothDistortY));
    }
}