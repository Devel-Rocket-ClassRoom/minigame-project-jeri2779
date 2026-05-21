using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] weapons;
    private int currentIndex = 0;

    private void Start()
    {
        EquipSlot(0);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) EquipSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) EquipSlot(1);
    }

    public void EquipSlot(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(i == index);
        currentIndex = index;
    }
}
