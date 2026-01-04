using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Gerekli Referanslar")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraPoint; // Göz
    [SerializeField] private Transform holdPoint;   // El
    [SerializeField] private Transform ghostObject; // Preview objesi
    
    [Header("Ayarlar")]
    [SerializeField] private LayerMask itemLayer; // Eşyalar
    [SerializeField] private LayerMask surfaceLayer; // Masa/Yer
    [SerializeField] private float reach = 3f;
    [SerializeField] private float smoothSpeed = 15f; 

    private PickupItem currentItem; 
    private float targetYRotation;  // Döndürme açısı
    private float itemHalfHeight;   // Objenin zeminden ne kadar yüksekte duracağı
    private MeshFilter ghostMesh;   

    private void Awake()
    {
        if (ghostObject) 
        {
            ghostMesh = ghostObject.GetComponent<MeshFilter>();
            ghostObject.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        inputReader.PrimaryActionEvent += OnLeftClick;
        inputReader.SecondaryActionEvent += OnRightClick;
        inputReader.RotateEvent += OnScroll;
    }

    private void OnDisable()
    {
        inputReader.PrimaryActionEvent -= OnLeftClick;
        inputReader.SecondaryActionEvent -= OnRightClick;
        inputReader.RotateEvent -= OnScroll;
    }

    private void Update()
    {
        UpdateItemPosition(); 
        UpdateGhost();        
    }

    // --- 1. EŞYA HAREKETİ (ELDEKİ) ---
    private void UpdateItemPosition()
    {
        if (currentItem == null) return;

        // Pozisyonu yumuşat
        currentItem.transform.localPosition = Vector3.Lerp(
            currentItem.transform.localPosition, 
            currentItem.holdOffset, 
            Time.deltaTime * smoothSpeed
        );

        // Rotasyonu yumuşat (Kamerayla birlikte eğilsin)
        Quaternion targetRot = Quaternion.Euler(0, targetYRotation, 0);
        currentItem.transform.localRotation = Quaternion.Slerp(
            currentItem.transform.localRotation, 
            targetRot, 
            Time.deltaTime * smoothSpeed
        );
    }

    // --- 2. HAYALET (GHOST) GÜNCELLEME ---
    private void UpdateGhost()
    {
        if (currentItem == null || ghostObject == null)
        {
            if (ghostObject && ghostObject.gameObject.activeSelf) 
                ghostObject.gameObject.SetActive(false);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(cameraPoint.position, cameraPoint.forward, out hit, reach, surfaceLayer))
        {
            if (!ghostObject.gameObject.activeSelf) ghostObject.gameObject.SetActive(true);

            // DÜZELTME 1: Yükseklik Ayarı
            // Vurulan noktaya (hit.point) objenin yarım boyunu ekliyoruz.
            ghostObject.position = hit.point + (Vector3.up * itemHalfHeight);

            // DÜZELTME 2: Dik Durma Ayarı
            // Kameranın Y açısını al (böylece oyuncunun baktığı yöne döner) + Scroll ile eklenen açı.
            // X ve Z'yi 0 yaparak TAM DİK durmasını sağlıyoruz.
            float finalY = cameraPoint.eulerAngles.y + targetYRotation;
            ghostObject.rotation = Quaternion.Euler(0, finalY, 0);

            // Boyut eşitleme
            ghostObject.localScale = currentItem.transform.localScale;
        }
        else
        {
            ghostObject.gameObject.SetActive(false);
        }
    }

    private void OnLeftClick()
    {
        if (currentItem != null)
        {
            // Kasa Kontrolü
            if (GetLookedItem(out PickupItem hitItem))
            {
                if (hitItem.type == PickupItem.ItemType.Crate && hitItem.crateContent.ID == currentItem.ID)
                {
                    Destroy(currentItem.gameObject);
                    currentItem = null;
                    return;
                }
            }
            // Yere Bırak
            PlaceItem();
        }
        else
        {
            // Yerden Al
            if (GetLookedItem(out PickupItem hitItem))
            {
                if (hitItem.type == PickupItem.ItemType.Single)
                {
                    Equip(hitItem);
                }
                else if (hitItem.type == PickupItem.ItemType.Crate && hitItem.crateContent != null)
                {
                    PickupItem newItem = Instantiate(hitItem.crateContent);
                    newItem.transform.position = hitItem.transform.position + Vector3.up; 
                    Equip(newItem);
                }
            }
        }
    }

    private void OnRightClick()
    {
        if (currentItem != null)
        {
            currentItem.onAction?.Invoke();
            return;
        }

        if (GetLookedItem(out PickupItem hitItem))
        {
            if (hitItem.type == PickupItem.ItemType.Crate) Equip(hitItem);
        }
    }

    private void OnScroll(float direction)
    {
        if (currentItem != null) targetYRotation += (direction > 0 ? 90 : -90);
    }

    // --- YARDIMCI METODLAR ---

    private void Equip(PickupItem item)
    {
        currentItem = item;
        currentItem.transform.SetParent(holdPoint);
        currentItem.SetHeldState(true);
        
        targetYRotation = 0; // Açıyı sıfırla
        
        // DÜZELTME: Objenin Yüksekliğini Hesapla
        // Collider'ın y eksenindeki yarı boyunu (extents.y) alıyoruz.
        // Bunu yaparken scale'i de hesaba katmak için bounds kullanıyoruz.
        // Ama eldeki eğimden etkilenmemesi için önce rotasyonu sıfırlayıp ölçüyoruz.
        currentItem.transform.localRotation = Quaternion.identity;
        Collider col = currentItem.GetComponent<Collider>();
        if (col != null)
        {
            itemHalfHeight = col.bounds.extents.y;
        }
        
        if (ghostMesh != null) 
            ghostMesh.sharedMesh = item.GetComponent<MeshFilter>().sharedMesh;
    }

    private void PlaceItem()
    {
        currentItem.transform.SetParent(null);
        
        if (ghostObject.gameObject.activeSelf)
        {
            // Hayalet aktifse tam olarak onun pozisyonuna ve rotasyonuna ışınla
            // Hayalet zaten hesaplanmış (Dik duran ve Yükseltilmiş) konumdadır.
            currentItem.transform.position = ghostObject.position;
            currentItem.transform.rotation = ghostObject.rotation;
        }

        currentItem.SetHeldState(false);
        currentItem = null;
        ghostObject.gameObject.SetActive(false);
    }

    private bool GetLookedItem(out PickupItem item)
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraPoint.position, cameraPoint.forward, out hit, reach, itemLayer))
        {
            return hit.collider.TryGetComponent(out item);
        }
        item = null;
        return false;
    }
}