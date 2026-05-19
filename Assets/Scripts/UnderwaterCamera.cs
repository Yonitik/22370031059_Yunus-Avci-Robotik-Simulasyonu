using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderwaterCamera : MonoBehaviour
{
    [Header("Baðlantý")]
    public Volume postProcessVolume; // Sahnendeki Global Volume objesini buraya sürükle

    [Header("Su Altý Dalgalanmasý (Wobble)")]
    public float wobbleSpeed = 1.5f;        // Dalganýn hýzý (Nefes alýp verme ritmi gibi olmalý)
    public float wobbleIntensity = 0.15f;   // Dalganýn þiddeti (Çok artýrýrsan deniz tutar!)

    private LensDistortion lensDistortion;

    void Start()
    {
        // Volume profilinin içinden Lens Distortion efektini bulup kodu ona baðlýyoruz
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out lensDistortion))
        {
            lensDistortion.active = true;
        }
        else
        {
            Debug.LogWarning("[Su Altý] Global Volume'da 'Lens Distortion' efekti bulunamadý! Lütfen Add Override diyerek ekle.");
        }
    }

    void Update()
    {
        if (lensDistortion != null)
        {
            // Zaman faktörünü kullanarak iki farklý dalga (Sinüs ve Kosinüs) üretiyoruz
            float wave1 = Mathf.Sin(Time.time * wobbleSpeed);
            float wave2 = Mathf.Cos(Time.time * wobbleSpeed * 0.8f); // 0.8 ile asimetrik yapýyoruz ki organik dursun

            // Kameranýn kenarlarýný su basýncý yiyormuþ gibi içeri-dýþarý büker
            lensDistortion.intensity.value = wave1 * wobbleIntensity;

            // Ekraný jöle gibi hafifçe X ve Y ekseninde sündürür
            lensDistortion.xMultiplier.value = 1f + (wave1 * 0.05f);
            lensDistortion.yMultiplier.value = 1f + (wave2 * 0.05f);
        }
    }
}