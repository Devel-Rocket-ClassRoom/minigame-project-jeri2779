using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// WeaponList 안의 "종류별 패시브 해금" 버튼 1개에 붙인다.
// 무기 구매(WeaponShopItem)의 형제 — 스탯강화(StatUpgradeItem)와는 다른 도메인(WeaponType).
// 설명은 버튼에 공간이 부족하므로 마우스오버 시 공용 패널에 띄운다(WeaponShopItem→WeaponInfoPanel과 동일 패턴).
public class WeaponTypePassiveItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private WeaponType weaponType = WeaponType.None;
    [SerializeField] private int price = 30000;
    [SerializeField] private string displayName;            // 비우면 weaponType 이름 사용
    [SerializeField, TextArea] private string description;  // 마우스오버 설명창에 표시
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;           // 가격 / 해금상태

    private Button button;
    private ShopUI shopUI;

    private string Label => string.IsNullOrEmpty(displayName) ? weaponType.ToString() : displayName;

    private void Start()
    {
        button = GetComponent<Button>();
        shopUI = GetComponentInParent<ShopUI>(true);
        button.onClick.AddListener(() => shopUI.TryUnlockWeaponType(weaponType, price));

        if (nameText != null)
            nameText.text = Label;
    }

    public void OnPointerEnter(PointerEventData eventData) => shopUI?.ShowPassiveInfo(Label, description);

    public void OnPointerExit(PointerEventData eventData) => shopUI?.HidePassiveInfo();

    public void Refresh(ShopController shop)
    {
        if (button == null) button = GetComponent<Button>();
        bool unlocked = shop.IsWeaponTypeUnlocked(weaponType);
        button.interactable = shop.CanUnlockWeaponType(weaponType, price);
        if (statusText != null)
            statusText.text = unlocked ? "해금됨" : $"{price}G";
    }
}
