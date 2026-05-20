using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Kameralar")]
    public Camera fakeCamera; // Nokta Bulutu Kamerasý (Sol)
    public Camera realCamera; // Gerçek Dünya Kamerasý (Sað)

    private int cameraMode = 0; // 0: Bölünmüþ, 1: Full Sahte, 2: Full Gerçek

    void Start()
    {
        UpdateCameraMode();
    }

    void Update()
    {
        // F tuþuna basýldýðýnda modlar arasýnda geçiþ yap (0 -> 1 -> 2 -> 0)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            cameraMode++;
            if (cameraMode > 2) cameraMode = 0;

            UpdateCameraMode();
        }
    }

    void UpdateCameraMode()
    {
        switch (cameraMode)
        {
            case 0:
                // 1. Mod: Bölünmüþ Ekran (Split Screen)
                fakeCamera.enabled = true;
                realCamera.enabled = true;
                fakeCamera.rect = new Rect(0f, 0f, 0.5f, 1f); // Sol yarý
                realCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f); // Sað yarý
                break;

            case 1:
                // 2. Mod: Sadece Sahte Kamera (Full Ekran)
                fakeCamera.enabled = true;
                realCamera.enabled = false;
                fakeCamera.rect = new Rect(0f, 0f, 1f, 1f); // Tam ekran
                break;

            case 2:
                // 3. Mod: Sadece Gerçek Kamera (Full Ekran)
                fakeCamera.enabled = false;
                realCamera.enabled = true;
                realCamera.rect = new Rect(0f, 0f, 1f, 1f); // Tam ekran
                break;
        }
    }
}