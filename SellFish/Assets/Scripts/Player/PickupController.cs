using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Gerekli Referanslar")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraPoint; // Göz
    [SerializeField] private Transform holdPoint;   // El
    [SerializeField] private Transform ghostObject; // Preview objesi (Collider'ı SİLİNMİŞ olmalı)
    
    [Header("Ayarlar")]
    [SerializeField] private LayerMask itemLayer; // Eşyalar
    [SerializeField] private LayerMask surfaceLayer; // Masa/Yer
    [SerializeField] private float reach = 3f;
    [SerializeField] private float smoothSpeed = 15f; // Yumuşaklık hızı

    private PickupItem currentItem; // Elimizdeki
    private float targetYRotation;  // Döndürme açısı
    private MeshFilter ghostMesh;   // Hayaletin şeklini değiştirmek için

    private void Awake()
    {
        if (ghostObject) 
        {
            ghostMesh = ghostObject.GetComponent<MeshFilter>();
            ghostObject.gameObject.SetActive(false); // Başta gizle
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
        UpdateItemPosition(); // Eşyayı ele yumuşakça çek
        UpdateGhost();        // Hayaleti güncelle
    }

    // --- 1. MANTIK: HAREKET VE GÖRSEL ---
    private void UpdateItemPosition()
    {
        if (currentItem == null) return;

        // Pozisyonu yumuşat (Lerp)
        currentItem.transform.localPosition = Vector3.Lerp(
            currentItem.transform.localPosition, 
            currentItem.holdOffset, 
            Time.deltaTime * smoothSpeed
        );

        // Rotasyonu yumuşat (Slerp)
        Quaternion targetRot = Quaternion.Euler(0, targetYRotation, 0);
        currentItem.transform.localRotation = Quaternion.Slerp(
            currentItem.transform.localRotation, 
            targetRot, 
            Time.deltaTime * smoothSpeed
        );
    }

    private void UpdateGhost()
    {
        // Elimiz boşsa veya hayalet atanmamışsa gösterme
        if (currentItem == null || ghostObject == null)
        {
            if (ghostObject && ghostObject.gameObject.activeSelf) 
                ghostObject.gameObject.SetActive(false);
            return;
        }

        // Yere bakıyor muyuz?
        RaycastHit hit;
        if (Physics.Raycast(cameraPoint.position, cameraPoint.forward, out hit, reach, surfaceLayer))
        {
            if (!ghostObject.gameObject.activeSelf) ghostObject.gameObject.SetActive(true);

            ghostObject.position = hit.point;
            ghostObject.rotation = currentItem.transform.rotation; // Eşyanın duruşuyla aynı olsun
        }
        else
        {
            ghostObject.gameObject.SetActive(false); // Boşluğa bakıyorsak gizle
        }
    }

    // --- 2. INPUT: SOL TIK (ALMA / BIRAKMA / RESTOCK) ---
    private void OnLeftClick()
    {
        // A) ELİMİZ DOLUYSA
        if (currentItem != null)
        {
            // Kasaya mı bakıyoruz? (Geri koyma kontrolü)
            if (GetLookedItem(out PickupItem hitItem))
            {
                if (hitItem.type == PickupItem.ItemType.Crate && hitItem.crateContent.ID == currentItem.ID)
                {
                    // Eşyayı yok et (Kasaya girdi)
                    Destroy(currentItem.gameObject);
                    currentItem = null;
                    return;
                }
            }

            // Kasaya bakmıyorsak -> Yere Bırak
            PlaceItem();
        }
        // B) ELİMİZ BOŞSA
        else
        {
            if (GetLookedItem(out PickupItem hitItem))
            {
                // Tekil Eşya -> Direkt Al
                if (hitItem.type == PickupItem.ItemType.Single)
                {
                    Equip(hitItem);
                }
                // Kasa -> İçinden kopya üret ve Al
                else if (hitItem.type == PickupItem.ItemType.Crate && hitItem.crateContent != null)
                {
                    PickupItem newItem = Instantiate(hitItem.crateContent);
                    newItem.transform.position = hitItem.transform.position + Vector3.up; // Kasanın üstünde doğsun
                    Equip(newItem);
                }
            }
        }
    }

    // --- 3. INPUT: SAĞ TIK (KASA ALMA / AKSİYON) ---
    private void OnRightClick()
    {
        // Elimiz doluysa -> Özellik kullan (Spatula çevir vs)
        if (currentItem != null)
        {
            currentItem.onAction?.Invoke();
            return;
        }

        // Elimiz boşsa -> Sadece Kasayı al
        if (GetLookedItem(out PickupItem hitItem))
        {
            if (hitItem.type == PickupItem.ItemType.Crate)
            {
                Equip(hitItem);
            }
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
        currentItem.SetHeldState(true); // Fiziği kapat
        
        targetYRotation = 0; // Açıyı sıfırla

        // Hayaletin şeklini (Mesh) elimizdeki eşya yap
        if (ghostMesh != null) 
            ghostMesh.sharedMesh = item.GetComponent<MeshFilter>().sharedMesh;
    }

    private void PlaceItem()
    {
        currentItem.transform.SetParent(null);
        
        // Eğer hayalet açıksa, tam onun olduğu yere bırak
        if (ghostObject.gameObject.activeSelf)
        {
            currentItem.transform.position = ghostObject.position;
            currentItem.transform.rotation = ghostObject.rotation;
        }

        currentItem.SetHeldState(false); // Fiziği aç
        currentItem = null;
    }

    // Raycast işlemini kısaltan fonksiyon
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