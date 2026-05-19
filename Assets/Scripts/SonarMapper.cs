using UnityEngine;
using System.Collections.Generic;

public class SonarMapper : MonoBehaviour
{
    [Header("Sistem Ayarlarý")]
    public ParticleSystem pointCloudSystem;
    public LayerMask caveLayer;

    [Header("Tarama Ayarlarý")]
    public float scanRange = 50f;
    public int raysPerFrame = 20;

    [Header("Nokta Bulutu Optimizasyonu")]
    public float pointSpacing = 0.1f; // Her 10 cm'de (0.1 birim) en fazla 1 nokta
    private HashSet<Vector3Int> occupiedPoints = new HashSet<Vector3Int>();

    [Header("Renk ve Derinlik")]
    public Gradient depthGradient;
    public float minY = -20f;
    public float maxY = 10f;

    [Header("Lazer Görselliði (LIDAR)")]
    public bool showLasers = true;
    public Material laserMaterial;
    public float laserWidth = 0.05f;

    [Header("Yüzey Ýnþasý (Wireframe Scan)")]
    public bool enableSurfaceScan = true;
    public Material wireframeMaterial;

    private LineRenderer[] laserPool;

    // --- Wireframe Hafýzasý ---
    private Mesh scannedMesh;
    private GameObject scannedMeshObj;
    private List<Vector3> scannedVertices = new List<Vector3>();
    private List<int> scannedIndices = new List<int>();
    private HashSet<string> discoveredTriangles = new HashSet<string>();
    private bool needsMeshUpdate = false;

    void Start()
    {
        laserPool = new LineRenderer[raysPerFrame];
        for (int i = 0; i < raysPerFrame; i++)
        {
            GameObject laserObj = new GameObject("ScanLaser_" + i);
            laserObj.transform.SetParent(transform);
            laserObj.layer = LayerMask.NameToLayer("NoktaBulutu");

            LineRenderer lr = laserObj.AddComponent<LineRenderer>();
            if (laserMaterial != null) lr.material = laserMaterial;

            lr.startWidth = laserWidth;
            lr.endWidth = laserWidth;
            lr.positionCount = 2;
            lr.enabled = false;
            laserPool[i] = lr;
        }

        scannedMesh = new Mesh();
        scannedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        scannedMeshObj = new GameObject("ScannedSurfaceWireframe");
        scannedMeshObj.layer = LayerMask.NameToLayer("NoktaBulutu");

        MeshFilter filter = scannedMeshObj.AddComponent<MeshFilter>();
        MeshRenderer renderer = scannedMeshObj.AddComponent<MeshRenderer>();
        filter.mesh = scannedMesh;
        if (wireframeMaterial != null) renderer.material = wireframeMaterial;
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
                if (MapManager.Instance != null)
                {
                    MapManager.Instance.RegisterSurfaceHit(hit.point);
                }

                // Nokta çizimini yap
                CreateMapPoint(hit.point);

                if (enableSurfaceScan)
                {
                    MeshCollider meshCollider = hit.collider as MeshCollider;
                    if (meshCollider != null && meshCollider.sharedMesh != null)
                    {
                        string triKey = meshCollider.GetInstanceID() + "_" + hit.triangleIndex;
                        if (!discoveredTriangles.Contains(triKey))
                        {
                            discoveredTriangles.Add(triKey);
                            ExtractAndDrawTriangle(meshCollider, hit.triangleIndex);
                        }
                    }
                }

                if (showLasers)
                {
                    laserPool[i].enabled = true;
                    laserPool[i].SetPosition(0, transform.position);
                    laserPool[i].SetPosition(1, hit.point);

                    // Lazer rengi DÜNYA koordinatýna göre (Düzeltildi)
                    Color pointColor = depthGradient.Evaluate(Mathf.InverseLerp(minY, maxY, hit.point.y));
                    laserPool[i].startColor = pointColor;
                    laserPool[i].endColor = pointColor;
                }
            }
            else
            {
                if (showLasers)
                {
                    laserPool[i].enabled = true;
                    laserPool[i].SetPosition(0, transform.position);
                    laserPool[i].SetPosition(1, transform.position + (randomDirection.normalized * scanRange));

                    laserPool[i].startColor = new Color(1, 1, 1, 0.1f);
                    laserPool[i].endColor = new Color(1, 1, 1, 0.1f);
                }
            }
        }
    }

    void ExtractAndDrawTriangle(MeshCollider collider, int triangleIndex)
    {
        Mesh mesh = collider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3 p0 = vertices[triangles[triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[triangleIndex * 3 + 2]];

        Transform hitTransform = collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);

        int startIndex = scannedVertices.Count;
        scannedVertices.Add(p0);
        scannedVertices.Add(p1);
        scannedVertices.Add(p2);

        scannedIndices.Add(startIndex + 0); scannedIndices.Add(startIndex + 1);
        scannedIndices.Add(startIndex + 1); scannedIndices.Add(startIndex + 2);
        scannedIndices.Add(startIndex + 2); scannedIndices.Add(startIndex + 0);

        needsMeshUpdate = true;
    }

    void LateUpdate()
    {
        if (needsMeshUpdate && scannedVertices.Count > 0)
        {
            scannedMesh.SetVertices(scannedVertices);
            scannedMesh.SetIndices(scannedIndices, MeshTopology.Lines, 0);
            needsMeshUpdate = false;
        }
    }

    void CreateMapPoint(Vector3 hitPosition)
    {
        // Uzamsal Filtreleme (Spatial Hashing)
        Vector3Int cellPos = new Vector3Int(
            Mathf.RoundToInt(hitPosition.x / pointSpacing),
            Mathf.RoundToInt(hitPosition.y / pointSpacing),
            Mathf.RoundToInt(hitPosition.z / pointSpacing)
        );

        if (occupiedPoints.Contains(cellPos)) return;

        occupiedPoints.Add(cellPos);

        // Nokta rengi DÜNYA koordinatýna göre (HATA BURADAYDI, DÜZELTÝLDÝ!)
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