using UnityEngine;
using System.Collections.Generic;

public class ChunkData
{
    public int surfaceHits;
    public bool isExplored;
    public GameObject visualCube; // YENÝ: Oyundaki gerçek küp objesini hafýzada tutuyoruz
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Chunk (Bölge) Ayarlarý")]
    public float chunkSize = 10f;
    public int hitsToExploreChunk = 50;

    [Header("Hedef Hacim (Yüzde Hesabý Ýçin)")]
    public Vector3 expectedCaveSize = new Vector3(100f, 50f, 100f);
    private float totalExpectedChunks;

    [Header("Oyun Ýçi Görsellik (Sol Kamera)")]
    public bool showInGame = true;              // Oyunda küpleri göster
    public Material unexploredMaterial;         // Kýrmýzý Saydam Materyal
    public Material exploredMaterial;           // Yeþil Saydam Materyal

    public Dictionary<Vector3Int, ChunkData> chunkMap = new Dictionary<Vector3Int, ChunkData>();

    private int totalExploredChunks = 0;

    void Awake()
    {
        Instance = this;
        totalExpectedChunks = (expectedCaveSize.x / chunkSize) * (expectedCaveSize.y / chunkSize) * (expectedCaveSize.z / chunkSize);
        if (totalExpectedChunks <= 0) totalExpectedChunks = 1f;
    }

    public Vector3Int GetChunkCoordinate(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize),
            Mathf.FloorToInt(worldPos.z / chunkSize)
        );
    }

    public void RegisterSurfaceHit(Vector3 hitPoint)
    {
        Vector3Int chunkCoord = GetChunkCoordinate(hitPoint);

        // ODA (CHUNK) ÝLK DEFA KEÞFEDÝLÝYORSA:
        if (!chunkMap.ContainsKey(chunkCoord))
        {
            GameObject newCube = null;

            if (showInGame)
            {
                // Unity'nin varsayýlan küpünü kodla oluþturuyoruz
                newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Küpün dünyadaki tam merkezini ve boyutunu (10x10x10) ayarlýyoruz
                newCube.transform.position = new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z) * chunkSize + (Vector3.one * (chunkSize / 2f));
                newCube.transform.localScale = Vector3.one * chunkSize;

                // SADECE SOL KAMERADA GÖRÜNSÜN DÝYE: Katmanýný NoktaBulutu yapýyoruz!
                newCube.layer = LayerMask.NameToLayer("NoktaBulutu");

                // Lazerlerimizi engellemesin diye katý fizik özelliðini (Collider) siliyoruz
                Destroy(newCube.GetComponent<Collider>());

                // Ýlk rengini Kýrmýzý (Keþfedilmemiþ) yapýyoruz
                if (unexploredMaterial != null)
                    newCube.GetComponent<Renderer>().material = unexploredMaterial;
            }

            // Hafýzaya ekle
            chunkMap[chunkCoord] = new ChunkData
            {
                surfaceHits = 0,
                isExplored = false,
                visualCube = newCube
            };
        }

        ChunkData chunk = chunkMap[chunkCoord];

        if (chunk.isExplored) return;

        chunk.surfaceHits++;

        // ODA (CHUNK) TAMAMEN TARANDIYSA:
        if (chunk.surfaceHits >= hitsToExploreChunk)
        {
            chunk.isExplored = true;
            totalExploredChunks++;

            // Küpün rengini Yeþil (Keþfedilmiþ) olarak deðiþtir
            if (chunk.visualCube != null && exploredMaterial != null)
            {
                chunk.visualCube.GetComponent<Renderer>().material = exploredMaterial;
            }
        }
    }

    public float GetCompletionPercentage()
    {
        return (totalExploredChunks / totalExpectedChunks) * 100f;
    }
}