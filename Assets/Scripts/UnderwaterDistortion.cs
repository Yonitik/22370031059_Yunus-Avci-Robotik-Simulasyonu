using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderwaterDistortion : MonoBehaviour
{
    [Header("Post Process Baðlantýsý")]
    [Tooltip("Sadece sað kameranýn göreceði SagKameraPost katmanýndaki Volume objesini buraya sürükle.")]
    public Volume localVolume;

    [Header("Wobble (Bükülme) Ayarlarý")]
    public float waveSpeed = 1.5f;           // Dalgalanma hýzý
    public float distortionIntensity = 0.4f; // Ekranýn ne kadar þiddetli büküleceði (Distort)

    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        // Volume objesinin içindeki efektleri bul ve koda baðla
        if (localVolume != null)
        {
            localVolume.profile.TryGet(out lensDistortion);
            localVolume.profile.TryGet(out chromaticAberration);

            if (lensDistortion != null) lensDistortion.active = true;
            if (chromaticAberration != null) chromaticAberration.active = true;
        }
    }

    void Update()
    {
        // Zamaný kullanarak organik su dalgalarý (matematiksel sinüs dalgasý) üretiyoruz
        float wave1 = Mathf.Sin(Time.time * waveSpeed);
        float wave2 = Mathf.Cos(Time.time * waveSpeed * 0.8f);

        if (lensDistortion != null)
        {
            // 1. EKRAN BÜKÜLMESÝ (DISTORTION)
            // Ekranýn merkezinden dýþa doðru sürekli esneyip daralmasýný saðlar
            lensDistortion.intensity.value = wave1 * distortionIntensity;

            // 2. EKRANIN SÜNDÜRÜLMESÝ (WOBBLE)
            // X ve Y eksenlerinde ekraný jöle gibi asimetrik olarak sündürür
            lensDistortion.xMultiplier.value = 1f + (wave1 * 0.08f);
            lensDistortion.yMultiplier.value = 1f + (wave2 * 0.08f);
        }

        if (chromaticAberration != null)
        {
            // 3. KALIN CAM/SU EFEKTÝ (Renk Sapmasý)
            // Ekran büküldükçe köþelerdeki renkleri (Kýrmýzý/Mavi) birbirinden ayýrarak eski kamera hissi verir
            chromaticAberration.intensity.value = 0.1f + (Mathf.Abs(wave1) * 0.3f);
        }
    }
}