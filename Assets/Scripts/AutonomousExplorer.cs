using UnityEngine;
using UnityEngine.InputSystem;

public class AutonomousExplorer : MonoBehaviour
{
    [Header("Yönetim Modu")]
    public bool isAIEnabled = true; // Tikliyse kendi gezer, deðilse sen sürersin

    [Header("Denizaltý Fiziði (Momentum & Drift)")]
    public float maxSpeed = 5f;
    public float acceleration = 1.5f;
    public float turnSpeed = 2f;
    public float gravity = 0.2f;
    public float waterDriftAmount = 0.5f;

    [Header("Çarpýþma ve Sekme (Bounce)")]
    public float collisionRadius = 1.5f;
    public float bounceForce = 0.6f;

    [Header("Sensörler ve Kaçýþ")]
    public float sensorLength = 20f;
    public LayerMask caveLayer;

    [Header("Yapay Zeka (Zeki Keþif)")]
    public float targetUpdateInterval = 2f;
    private float targetTimer = 0f;
    private Vector3 globalTargetPosition;

    private Vector3 currentVelocity;
    private Vector3 targetDirection;

    void Start()
    {
        currentVelocity = transform.forward * maxSpeed;
        globalTargetPosition = transform.position + transform.forward * 20f;
    }

    void Update()
    {
        // --- H TUÞU ÝLE MOD DEÐÝÞÝMÝ ---
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            isAIEnabled = !isAIEnabled;
            Debug.Log("Denizaltý Kontrolü: " + (isAIEnabled ? "OTONOM (Yapay Zeka)" : "MANUEL (Oyuncu)"));
        }

        // Hangi moddaysak onun beynini çalýþtýr
        if (isAIEnabled)
        {
            RunAI();
        }
        else
        {
            RunManualControl();
        }

        // Fizik ve çarpýþmalar iki modda da ortaktýr!
        ApplyPhysicsAndCollision();
    }

    void RunAI()
    {
        targetTimer -= Time.deltaTime;
        if (targetTimer <= 0f)
        {
            FindGlobalTarget();
            targetTimer = targetUpdateInterval;
        }

        DetermineBestDirection();

        float noiseX = Mathf.PerlinNoise(Time.time * 0.3f, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0f, Time.time * 0.3f) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(Time.time * 0.3f, Time.time * 0.3f) * 2f - 1f;
        Vector3 drift = new Vector3(noiseX, noiseY, noiseZ) * waterDriftAmount;

        float desiredSpeed = maxSpeed;
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        if (angleToTarget > 10f) desiredSpeed = Mathf.Lerp(maxSpeed, maxSpeed * 0.3f, angleToTarget / 90f);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit sensorHit, sensorLength, caveLayer))
        {
            desiredSpeed = Mathf.Min(desiredSpeed, maxSpeed * (sensorHit.distance / sensorLength));
        }
        desiredSpeed = Mathf.Max(desiredSpeed, maxSpeed * 0.25f);

        Vector3 desiredVelocity = (targetDirection + drift).normalized * desiredSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);

        if (currentVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    void RunManualControl()
    {
        float moveInput = 0f;
        float yawInput = 0f;
        float pitchInput = 0f;

        // Klavye Girdileri
        if (Keyboard.current.iKey.isPressed) moveInput = 1f;  // Ýleri
        if (Keyboard.current.kKey.isPressed) moveInput = -1f; // Geri
        if (Keyboard.current.jKey.isPressed) yawInput = -1f;  // Sola Dön
        if (Keyboard.current.lKey.isPressed) yawInput = 1f;   // Saða Dön
        if (Keyboard.current.uKey.isPressed) pitchInput = -1f;// Yukarý Bak
        if (Keyboard.current.oKey.isPressed) pitchInput = 1f; // Aþaðý Bak

        // Burnunu Çevir
        transform.Rotate(pitchInput * turnSpeed * 30f * Time.deltaTime, yawInput * turnSpeed * 30f * Time.deltaTime, 0f, Space.Self);

        // Denizaltýnýn yan yatmasýný engelle (Roll kilitlenir)
        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.z = 0f;
        transform.eulerAngles = currentEuler;

        // Gaza Bas (Manuel Momentum)
        Vector3 desiredVelocity = transform.forward * moveInput * maxSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);
    }

    void ApplyPhysicsAndCollision()
    {
        currentVelocity.y -= gravity * Time.deltaTime;
        float moveDistance = currentVelocity.magnitude * Time.deltaTime;

        if (Physics.SphereCast(transform.position, collisionRadius, currentVelocity.normalized, out RaycastHit hit, moveDistance + 0.2f, caveLayer))
        {
            currentVelocity = Vector3.Reflect(currentVelocity, hit.normal) * bounceForce;
            transform.position = hit.point + hit.normal * (collisionRadius + 0.1f);
        }
        else
        {
            transform.position += currentVelocity * Time.deltaTime;
        }
    }

    void FindGlobalTarget()
    {
        Vector3 bestPoint = Vector3.zero;
        float bestScore = -Mathf.Infinity;

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.onUnitSphere * 40f;

            if (MapManager.Instance != null)
            {
                Vector3Int chunkCoord = MapManager.Instance.GetChunkCoordinate(randomPos);

                bool isExplored = false;
                if (MapManager.Instance.chunkMap.TryGetValue(chunkCoord, out ChunkData data))
                {
                    isExplored = data.isExplored;
                }

                if (!isExplored)
                {
                    Vector3 dirToPoint = randomPos - transform.position;

                    if (!Physics.Raycast(transform.position, dirToPoint.normalized, dirToPoint.magnitude, caveLayer))
                    {
                        float score = Vector3.Dot(transform.forward, dirToPoint.normalized);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestPoint = randomPos;
                        }
                    }
                }
            }
        }

        globalTargetPosition = (bestPoint != Vector3.zero) ? bestPoint : transform.position + transform.forward * 20f;
    }

    void DetermineBestDirection()
    {
        Vector3[] directions = new Vector3[]
        {
            transform.forward,
            transform.forward + transform.right * 0.6f,
            transform.forward - transform.right * 0.6f,
            transform.forward + transform.up * 0.6f,
            transform.forward - transform.up * 0.6f,
            transform.up,
            -transform.up,
            transform.forward + transform.right * 0.6f + transform.up * 0.6f,
            transform.forward - transform.right * 0.6f + transform.up * 0.6f,
            transform.forward + transform.right * 0.6f - transform.up * 0.6f,
            transform.forward - transform.right * 0.6f - transform.up * 0.6f
        };

        Vector3 desiredDir = (globalTargetPosition - transform.position).normalized;
        Vector3 bestDir = transform.forward;
        float bestScore = -Mathf.Infinity;
        bool allBlocked = true;

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, sensorLength, caveLayer))
            {
                if (hit.distance > sensorLength * 0.4f)
                {
                    float score = Vector3.Dot(dir.normalized, desiredDir) * (hit.distance / sensorLength);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDir = dir.normalized;
                        allBlocked = false;
                    }
                }
            }
            else
            {
                float score = Vector3.Dot(dir.normalized, desiredDir) + 2f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = dir.normalized;
                    allBlocked = false;
                }
            }
        }

        targetDirection = allBlocked ? (-transform.forward + transform.up * 0.5f).normalized : bestDir;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        Gizmos.color = Color.blue;
        if (Application.isPlaying) Gizmos.DrawWireSphere(globalTargetPosition, 2f);
    }
}