using UnityEngine;

public class SonarMapper : MonoBehaviour
{
    [Header("Sistem Ayarlarý")]
    public ParticleSystem pointCloudSystem;
    public LayerMask caveLayer;

    [Header("Tarama Ayarlarý")]
    public float scanRange = 50f;
    public int raysPerFrame = 20;

    [Header("Renk ve Derinlik")]
    public Gradient depthGradient;
    public float minY = -20f;
    public float maxY = 10f;

    [Header("Lazer Görselliði (LIDAR)")]
    public bool showLasers = true;          // Lazerler açýk mý?
    public Material laserMaterial;          // Lazerin materyali
    public float laserWidth = 0.05f;        // Lazer kalýnlýðý

    private LineRenderer[] laserPool;       // Performans için Lazer Havuzu

    void Start()
    {
        // Oyun baþlarken saniyede atýlacak ýþýn sayýsý kadar LineRenderer (çizgi) hazýrlýyoruz
        laserPool = new LineRenderer[raysPerFrame];
        for (int i = 0; i < raysPerFrame; i++)
        {
            GameObject laserObj = new GameObject("ScanLaser_" + i);
            laserObj.transform.SetParent(transform);

            // Sadece Sol Kamerada (NoktaBulutu) görünsün istiyorsan katmaný ayarla:
            laserObj.layer = LayerMask.NameToLayer("NoktaBulutu");

            LineRenderer lr = laserObj.AddComponent<LineRenderer>();
            if (laserMaterial != null) lr.material = laserMaterial;

            lr.startWidth = laserWidth;
            lr.endWidth = laserWidth;
            lr.positionCount = 2; // Baþlangýç ve Bitiþ noktasý
            lr.enabled = false;

            laserPool[i] = lr;
        }
    }

    void Update()
    {
        for (int i = 0; i < raysPerFrame; i++)
        {
            Vector3 randomDirection = transform.forward + new Vector3(
                Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)
            );

            if (Physics.Raycast(transform.position, randomDirection.normalized, out RaycastHit hit, scanRange, caveLayer))
            {
                // Voxel Grid (Yüzde hesaplama)
                float distance = hit.distance;
                float step = MapManager.Instance.voxelSize;
                for (float d = 0; d < distance; d += step)
                {
                    Vector3 emptySpace = transform.position + (randomDirection.normalized * d);
                    MapManager.Instance.MarkVoxel(emptySpace, 1);
                }
                MapManager.Instance.MarkVoxel(hit.point, 2);

                // Noktayý çiz
                CreateMapPoint(hit.point);

                // LAZERÝ ÇÝZ (Hedefi vurduysa)
                if (showLasers)
                {
                    laserPool[i].enabled = true;
                    laserPool[i].SetPosition(0, transform.position); // Lazerin çýkýþ yeri (Robot)
                    laserPool[i].SetPosition(1, hit.point);          // Lazerin deðdiði yer (Duvar)

                    // Lazerin rengini de derinliðe göre uyumlu yapalým (Efsane durur)
                    Color pointColor = depthGradient.Evaluate(Mathf.InverseLerp(minY, maxY, hit.point.y));
                    laserPool[i].startColor = pointColor;
                    laserPool[i].endColor = pointColor;
                }
            }
            else
            {
                // LAZERÝ ÇÝZ (Boþluða gittiyse, sonsuzluða uzanan bir çizgi)
                if (showLasers)
                {
                    laserPool[i].enabled = true;
                    laserPool[i].SetPosition(0, transform.position);
                    laserPool[i].SetPosition(1, transform.position + (randomDirection.normalized * scanRange));

                    laserPool[i].startColor = new Color(1, 1, 1, 0.1f); // Boþluða gidenler soluk beyaz
                    laserPool[i].endColor = new Color(1, 1, 1, 0.1f);
                }
            }
        }
    }

    void CreateMapPoint(Vector3 hitPosition)
    {
        float depthNormalized = Mathf.InverseLerp(minY, maxY, hitPosition.y);
        Color pointColor = depthGradient.Evaluate(depthNormalized);

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = hitPosition,
            startColor = pointColor,
            startSize = 0.15f,
            startLifetime = 99999f
        };
        pointCloudSystem.Emit(emitParams, 1);
    }
}