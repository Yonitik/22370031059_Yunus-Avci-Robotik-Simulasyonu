using UnityEngine;
using System.Collections;

public class AutonomousExplorer : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 3f;             // Forward hýzý
    public float turnSpeed = 40f;            // Dönüþ hýzý
    public float avoidanceThreshold = 8f;   // Çarpýþma riski algýlama mesafesi
    public float checkRange = 25f;          // Saða/Sola tarama mesafesi
    public LayerMask caveLayer;             // "Maðara" katmanýný seç

    [Header("Tarama Açýlarý")]
    public float sweepAngle = 70f;          // Saða/Sola kaç derecelik açýyla bakýlacak

    private bool isTurning = false;
    private Quaternion targetRotation;

    void Update()
    {
        // Dönmüyorsak, dümdüz git ve önünü kolla
        if (!isTurning)
        {
            MoveForwardAndCheckObstacles();
        }
        else
        {
            // Belirlenen yöne doðru dön
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Hedef açýya vardýk mý?
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isTurning = false;
            }
        }
    }

    void MoveForwardAndCheckObstacles()
    {
        // Önümüze bir ýþýn yolla
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, avoidanceThreshold, caveLayer))
        {
            // Çarpýþma riski! Dur ve etrafý kontrol et.
            FindNewPath();
        }
        else
        {
            // Yol açýk, ilerle
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    void FindNewPath()
    {
        float bestDistance = 0f;
        float bestAngle = 0f;
        bool foundPath = false;

        // Saða ve sola geniþ açýlý ýþýnlar atýp en uzak noktayý buluyoruz
        for (float currentAngle = -sweepAngle; currentAngle <= sweepAngle; currentAngle += 10f)
        {
            Quaternion rotationOffset = Quaternion.Euler(0, currentAngle, 0);
            Vector3 checkDirection = rotationOffset * transform.forward;

            RaycastHit checkHit;
            if (Physics.Raycast(transform.position, checkDirection, out checkHit, checkRange, caveLayer))
            {
                if (checkHit.distance > bestDistance)
                {
                    bestDistance = checkHit.distance;
                    bestAngle = currentAngle;
                    foundPath = true;
                }
            }
            else
            {
                // Iþýn bir þeye çarpmadýysa, bu yol en temiz yoldur!
                bestAngle = currentAngle;
                foundPath = true;
                break;
            }
        }

        if (foundPath)
        {
            // En iyi açýyý bulduk, o yöne dönmeyi baþlat
            targetRotation = transform.rotation * Quaternion.Euler(0, bestAngle, 0);
            isTurning = true;
        }
        else
        {
            // Hiçbir yer açýk deðilse (ki maðarada zor), 180 derece dön.
            targetRotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
            isTurning = true;
        }
    }

    // Editörde hata ayýklamak için ýþýnlarý çiz
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * avoidanceThreshold);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, (Quaternion.Euler(0, sweepAngle, 0) * transform.forward) * checkRange);
        Gizmos.DrawRay(transform.position, (Quaternion.Euler(0, -sweepAngle, 0) * transform.forward) * checkRange);
    }
}