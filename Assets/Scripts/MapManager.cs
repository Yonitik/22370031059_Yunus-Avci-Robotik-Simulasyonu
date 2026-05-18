using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance; // Her yerden ulaþabilmek için Singleton

    [Header("Izgara (Grid) Ayarlarý")]
    public float voxelSize = 1f; // Her bir sanal küpün boyutu (1 metre)

    [Header("Hedef Hacim (Yüzde Hesabý Ýçin)")]
    // Maðaranýn tahmini geniþliði, yüksekliði ve derinliði
    // Yüzde 100 olmasý için taranmasý gereken toplam hacim
    public Vector3 expectedCaveSize = new Vector3(100f, 50f, 100f);
    private float totalExpectedVoxels;

    // Hafýza: 1 = Su (Ýçinden geçilebilir), 2 = Kaya (Engel)
    // Dictionary'de olmayan yerler "Bilinmiyor (0)" sayýlýr.
    public Dictionary<Vector3Int, byte> voxelGrid = new Dictionary<Vector3Int, byte>();

    void Awake()
    {
        Instance = this;
        // Toplam kaç tane sanal küp taramamýz gerektiðini hesaplýyoruz
        totalExpectedVoxels = (expectedCaveSize.x / voxelSize) * (expectedCaveSize.y / voxelSize) * (expectedCaveSize.z / voxelSize);
    }

    // Gerçek dünyadaki koordinatý, sanal ýzgara koordinatýna çevirir
    public Vector3Int WorldToGrid(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / voxelSize),
            Mathf.FloorToInt(pos.y / voxelSize),
            Mathf.FloorToInt(pos.z / voxelSize)
        );
    }

    // Lazerin deðdiði yerleri hafýzaya yazma fonksiyonu
    public void MarkVoxel(Vector3 pos, byte state)
    {
        Vector3Int gridPos = WorldToGrid(pos);

        // Eðer orasý önceden "Kaya (2)" olarak iþaretlendiyse, yanlýþlýkla "Su (1)" ile ezmeyelim.
        if (!voxelGrid.ContainsKey(gridPos) || voxelGrid[gridPos] != 2)
        {
            voxelGrid[gridPos] = state;
        }
    }

    // Tamamlanma yüzdesini döndürür
    public float GetCompletionPercentage()
    {
        return (voxelGrid.Count / totalExpectedVoxels) * 100f;
    }
}