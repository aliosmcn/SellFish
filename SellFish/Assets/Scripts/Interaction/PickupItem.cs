using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    public enum ItemType { Single, Crate }

    [Header("Kimlik")]
    public string ID; // Örn: "Balik". Kasa ile eşleşme için şart.
    public ItemType type = ItemType.Single;
    
    [Header("Kasa İse Doldur")]
    public PickupItem crateContent; // Kasanın içinden çıkacak prefab

    [Header("Ayarlar")]
    public Vector3 holdOffset;
    public Quaternion holdRotation = Quaternion.identity;
    public UnityEvent onAction; // Sağ tık aksiyonu (Spatula çevir vb.)

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetHeldState(bool isHeld)
    {
        rb.isKinematic = isHeld;
        col.enabled = !isHeld;
        
        if (!isHeld) // Yere atıldığında
        {
            rb.linearVelocity = Vector3.zero; // Unity 6 (Eski sürümde velocity)
            rb.angularVelocity = Vector3.zero;
        }
    }
}