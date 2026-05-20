using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class IgnoreFog : MonoBehaviour
{
    private bool originalFogState;

    void OnEnable()
    {
        // URP'nin çizim döngüsüne (Render Loop) kancamýzý atýyoruz
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        // Script veya kamera kapanýrsa kancayý sök (Hata vermemesi için kritik)
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        // Eðer þu an çizim yapan kamera, BU scriptin takýlý olduðu kamera ise:
        if (cam == GetComponent<Camera>())
        {
            originalFogState = RenderSettings.fog; // Oyunun asýl sis durumunu hafýzaya al
            RenderSettings.fog = false;            // Sisi bu kamera için geçici olarak KAPAT
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        // Sol kameranýn çizimi bittiðinde:
        if (cam == GetComponent<Camera>())
        {
            RenderSettings.fog = originalFogState; // Diðer kameralar (Sað) bozulmasýn diye sisi geri AÇ
        }
    }
}