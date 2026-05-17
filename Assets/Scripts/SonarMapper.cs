using UnityEngine;

public class SonarMapper : MonoBehaviour
{
    [Header("Sistem Ayarlarý")]
    public ParticleSystem pointCloudSystem; // Eklediðin Particle System
    public LayerMask caveLayer;             // "Maðara" layer'ýný seç

    [Header("Tarama Ayarlarý")]
    public float scanRange = 50f;           // Iþýnýn gideceði maksimum mesafe
    public int raysPerFrame = 20;           // Saniyede atýlacak ýþýn sayýsý (performansa göre artýrýlabilir)

    [Header("Renk ve Derinlik")]
    public Gradient depthGradient;          // Inspector'dan renk geçiþini ayarla (Örn: Mor -> Turkuaz -> Turuncu)
    public float minY = -20f;               // Maðaranýn en derin noktasý
    public float maxY = 10f;                // Maðaranýn en yüksek noktasý

    void Update()
    {
        // Her frame'de rastgele yönlere ýþýn yollayarak ortamý tarýyoruz
        for (int i = 0; i < raysPerFrame; i++)
        {
            // Robotun ön tarafýný baz alarak yarým küre þeklinde rastgele bir yön belirle
            Vector3 randomDirection = transform.forward + new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            );

            // Raycast at
            if (Physics.Raycast(transform.position, randomDirection.normalized, out RaycastHit hit, scanRange, caveLayer))
            {
                CreateMapPoint(hit.point);
            }
        }
    }

    void CreateMapPoint(Vector3 hitPosition)
    {
        // Vurulan noktanýn Y eksenine (derinliðine) göre 0 ile 1 arasý bir deðer hesapla
        float depthNormalized = Mathf.InverseLerp(minY, maxY, hitPosition.y);

        // Bu deðere göre Gradient'ten rengi al
        Color pointColor = depthGradient.Evaluate(depthNormalized);

        // Partikülü (noktayý) oluþtur
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = hitPosition,
            startColor = pointColor,
            startSize = 0.15f // Noktalarýn büyüklüðü
        };

        // Sistemi tetikle ve 1 adet nokta býrak
        pointCloudSystem.Emit(emitParams, 1);
    }
}