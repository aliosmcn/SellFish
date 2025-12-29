using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    // Obje Türleri
    public enum ItemType { SingleObj, CrateObj }

    [Header("Özellikler")]
    public ItemType itemType = ItemType.SingleObj; // Bu obje tekil mi, kasa mı?
    
    [Header("Pozisyon Ayarları")]
    public Vector3 holdOffset = Vector3.zero;
    public Quaternion holdRotation = Quaternion.identity;

    [Header("Sağ Tık Özelliği (Opsiyonel)")]
    // Örneğin Spatula için buraya "Çevirme" fonksiyonu bağlayacağız.
    public UnityEvent onSecondaryUse; 

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnPickedUp()
    {
        rb.isKinematic = true; 
        col.enabled = false; // İç içe girmesin diye collider kapatıyoruz
    }

    public void OnDropped()
    {
        rb.isKinematic = false;
        col.enabled = true;
        
        // Fırlatma yapmazsak olduğu yere düşsün diye hızı sıfırlıyoruz
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Sağ tıklayınca çalışacak fonksiyon (Örn: Spatula çevirme)
    public void UseItemAction()
    {
        onSecondaryUse?.Invoke();
    }
}