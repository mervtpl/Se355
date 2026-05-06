using UnityEngine;
using UnityEngine.AI; // NavMesh özelliklerini (NavMeshAgent gibi) kullanabilmek için gerekli
using UnityEngine.InputSystem; // Yeni Input System paketini kullanabilmek için gerekli

public class PlayerController : MonoBehaviour
{
    public Camera cam; // Işın (Ray) göndermek için kullanılacak ana kamera
    public NavMeshAgent agent; // Karakterin hareketini sağlayan NavMeshAgent bileşeni

    void Update()
    {
        // Mouse'un sol tıkına bu karede (frame) basılıp basılmadığını kontrol eder
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Mouse'un ekrandaki o anki koordinatlarını (x, y) alır
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Kameradan, farenin bulunduğu noktaya doğru hayali bir ışın (Ray) oluşturur
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit hit; // Işının çarptığı yerin bilgilerini saklayacak değişken

            // Eğer ışın fiziksel bir nesneye (zemin vb.) çarparsa
            if (Physics.Raycast(ray, out hit))
            {
                // NavMeshAgent'a, ışının çarptığı koordinata (hit.point) gitmesini söyler
                agent.SetDestination(hit.point);
            }
        }
    }
}