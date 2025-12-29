using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraPoint; // Göz (Raycast çıkışı)
    [SerializeField] private Transform holdPoint;   // El (Eşyanın geleceği yer)
    
    [Header("Ayarlar")]
    [SerializeField] private LayerMask pickupLayer; // Hangi objeler tutulabilir?
    [SerializeField] private float pickupRange = 3f;

    private PickupItem currentItem; // Şu an elimizdeki eşya

    private void OnEnable()
    {
        inputReader.PrimaryActionEvent += HandleLeftClick;   // Sol Tık
        inputReader.SecondaryActionEvent += HandleRightClick; // Sağ Tık
        inputReader.RotateEvent += HandleScroll;             // Scroll
    }

    private void OnDisable()
    {
        inputReader.PrimaryActionEvent -= HandleLeftClick;
        inputReader.SecondaryActionEvent -= HandleRightClick;
        inputReader.RotateEvent -= HandleScroll;
    }

    // --- SOL TIK: Alma (Tekil) veya Bırakma (Her Şey) ---
    private void HandleLeftClick()
    {
        // 1. Eğer elimiz doluysa -> Eşyayı Bırak
        if (currentItem != null)
        {
            DropItem();
            return; 
        }

        // 2. Eğer elimiz boşsa -> Yerden "SingleObj" almaya çalış
        if (TryRaycast(out PickupItem item))
        {
            if (item.itemType == PickupItem.ItemType.SingleObj)
            {
                Pickup(item);
            }
        }
    }

    // --- SAĞ TIK: Alma (Kasa) veya Özellik Kullanma ---
    private void HandleRightClick()
    {
        // 1. Eğer elimiz doluysa -> Eşyanın özelliğini kullan (Örn: Spatula Çevir)
        if (currentItem != null)
        {
            currentItem.TriggerAction();
            return;
        }

        // 2. Eğer elimiz boşsa -> Yerden "CrateObj" almaya çalış
        if (TryRaycast(out PickupItem item))
        {
            if (item.itemType == PickupItem.ItemType.CrateObj)
            {
                Pickup(item);
            }
        }
    }

    // --- SCROLL: 90 Derece Çevirme ---
    private void HandleScroll(float direction)
    {
        if (currentItem != null)
        {
            // Yukarı kaydırınca +90, aşağı kaydırınca -90
            float angle = direction > 0 ? 90f : -90f;
            
            // Olduğu yerde döndür
            currentItem.transform.Rotate(Vector3.up, angle, Space.Self);
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---

    private bool TryRaycast(out PickupItem item)
    {
        RaycastHit hit;
        // Sadece tıklandığında Raycast atıyoruz (Performans için en iyisi)
        if (Physics.Raycast(cameraPoint.position, cameraPoint.forward, out hit, pickupRange, pickupLayer))
        {
            if (hit.collider.TryGetComponent(out item))
            {
                return true;
            }
        }
        item = null;
        return false;
    }

    private void Pickup(PickupItem item)
    {
        currentItem = item;
        currentItem.OnPickedUp();

        // Eşyayı HoldPoint'in çocuğu yap ve pozisyonunu sıfırla
        currentItem.transform.SetParent(holdPoint);
        currentItem.transform.localPosition = item.holdOffset;
        currentItem.transform.localRotation = item.holdRotation;
    }

    private void DropItem()
    {
        if (currentItem == null) return;

        // Eşyayı serbest bırak
        currentItem.transform.SetParent(null);
        currentItem.OnDropped();
        
        currentItem = null;
    }
}