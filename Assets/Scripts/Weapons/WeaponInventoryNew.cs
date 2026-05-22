using UnityEngine;

public class WeaponInventoryNew : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private WeaponData defaultSecondaryData;
    [SerializeField] private WeaponData defaultMeleeData;

    // WARN: enum 순서 변경 시 슬롯 인덱스 밀림 — 추후 Dictionary로 교체 검토
    private readonly IWeapon[] slots = new IWeapon[4];
    private int currentSlotIndex = 1;
    private float drawEndTime;

    public IWeapon CurrentWeapon => slots[currentSlotIndex];
    public bool IsDrawing => Time.time < drawEndTime;

    public bool HasWeapon(WeaponData data)
    {
        int slotIndex = (int)data.category;
        return slots[slotIndex] != null && slots[slotIndex].Data == data;
    }

    private void Start()
    {
        if (defaultSecondaryData != null) EquipByCategory(defaultSecondaryData);
        if (defaultMeleeData != null) EquipByCategory(defaultMeleeData);
    }

    public void EquipByCategory(WeaponData data)
    {
        int slotIndex = (int)data.category;

        if (currentSlotIndex == slotIndex)
            CurrentWeapon?.CancelAction();

        if (slots[slotIndex] != null)
            Destroy(slots[slotIndex].GameObject);

        if (data.weaponModelPrefab == null)
        {
            Debug.LogError($"{data.name}에 weaponModelPrefab이 없습니다.");
            return;
        }
        var go = Instantiate(data.weaponModelPrefab, weaponSocket, false);
        var weapon = go.GetComponent<IWeapon>();
        if (weapon == null)
        {
            Debug.LogError($"{data.weaponModelPrefab.name}에 IWeapon 구현체가 없습니다.");
            Destroy(go);
            return;
        }
        weapon.Init(characterStats);
        slots[slotIndex] = weapon;

        go.transform.localPosition = data.viewModelPosition;
        go.transform.localRotation = Quaternion.Euler(data.viewModelRotation);

        bool willBeActive = currentSlotIndex == slotIndex;
        go.SetActive(willBeActive);
        if (willBeActive) drawEndTime = Time.time + data.drawDuration;
    }

    public void DiscardSlot(int slotIndex)
    {
        if (slotIndex == (int)WeaponCategory.Melee) return;
        if (slots[slotIndex] == null) return;

        Destroy(slots[slotIndex].GameObject);
        slots[slotIndex] = null;

        if (currentSlotIndex == slotIndex)
            SwitchToFirstAvailable();
    }

    public void SwitchSlot(int slotIndex)
    {
        if (slotIndex == currentSlotIndex) return;
        if (slots[slotIndex] == null) return;

        slots[currentSlotIndex]?.GameObject.SetActive(false);
        CurrentWeapon?.CancelAction();

        currentSlotIndex = slotIndex;

        slots[currentSlotIndex].GameObject.SetActive(true);
        drawEndTime = Time.time + CurrentWeapon.Data.drawDuration;
    }

    public void ResetAmmo()
    {
        foreach (var weapon in slots)
            weapon?.ResetAmmo();
    }

    private void SwitchToFirstAvailable()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                SwitchSlot(i);
                return;
            }
        }
    }
}
