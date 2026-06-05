using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfoPanel : MonoBehaviour
{
    [SerializeField] private Image weaponVisual;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text recoilText;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text reloadText;

    public void Show(WeaponData data)
    {
        gameObject.SetActive(true);

        if (weaponVisual != null) weaponVisual.sprite = data.weaponIcon;
        if (nameText != null) nameText.text = data.displayName;
        if (priceText != null) priceText.text = $"{data.price}G";
        if (damageText != null) damageText.text = $"{data.damage:F0}";
        if (fireRateText != null) fireRateText.text = $"{(1f / data.fireRate):F0}/s";
        if (recoilText != null) recoilText.text = $"{data.verticalRecoil:F1}";
        if (rangeText != null) rangeText.text = $"{data.range}";

        var ranged = data as RangedWeaponData;
        if (ammoText != null)
            ammoText.text = ranged != null ? $"{ranged.magazineSize} / {ranged.maxReserveAmmo}" : "";
        if (reloadText != null)
            reloadText.text = ranged != null ? $"{ranged.reloadTime}s" : "";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
