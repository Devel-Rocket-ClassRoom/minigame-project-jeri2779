using UnityEngine;
using UnityEngine.UI;

public class WeaponShopItem : MonoBehaviour
{
    [SerializeField] private WeaponData data;

    private void Start()
    {
        var shopUI = GetComponentInParent<ShopUI>(true);
        GetComponent<Button>().onClick.AddListener(() => shopUI.TryBuy(data));
    }
}
