using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private WeaponData data;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;

    private Button button;
    private ShopUI shopUI;

    private void Start()
    {
        if (data == null) return;
        button = GetComponent<Button>();
        shopUI = GetComponentInParent<ShopUI>(true);
        button.onClick.AddListener(() => shopUI.TryBuy(data));

        if (nameText != null) nameText.text = data.displayName;
        if (statusText != null) statusText.text = $"{data.price}G";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null) shopUI?.ShowWeaponInfo(data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopUI?.HideWeaponInfo();
    }

    public void Refresh(int money, WeaponInventoryNew inventory)
    {
        if (data == null) return;
        if (button == null) button = GetComponent<Button>();
        bool owned = inventory.HasWeapon(data);
        button.interactable = !owned && money >= data.price;
        if (statusText != null)
            statusText.text = owned ? "보유중" : $"{data.price}G";
    }
}
