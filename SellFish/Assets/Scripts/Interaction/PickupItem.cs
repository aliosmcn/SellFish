using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    public enum ItemType { SingleObj, CrateObj }

    [Header("Ayarlar")]
    public ItemType itemType = ItemType.SingleObj;

    [Header("Pozisyon Ayarı")]
    public Vector3 holdOffset = Vector3.zero; 
    public Quaternion holdRotation = Quaternion.identity;

    [Header("Olaylar")]
    public UnityEvent onUseAction;

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
        col.enabled = false;
    }

    public void OnDropped()
    {
        rb.isKinematic = false;
        col.enabled = true;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void TriggerAction()
    {
        onUseAction?.Invoke();
    }
}