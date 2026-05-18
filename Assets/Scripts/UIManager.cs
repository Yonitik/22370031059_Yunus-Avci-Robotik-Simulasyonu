using UnityEngine;
using TMPro; // TextMeshPro kütüphanesi

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI completionText; // Inspector'dan o Text objesini buraya sürükle

    void Update()
    {
        // Yüzdeyi MapManager'dan alýp virgülden sonra 2 basamak olacak þekilde ekrana yaz
        float percent = MapManager.Instance.GetCompletionPercentage();
        completionText.text = "Keþif: %" + percent.ToString("F2");
    }
}