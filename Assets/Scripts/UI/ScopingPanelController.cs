using UnityEngine;

public class ScopingPanelController : MonoBehaviour
{
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private WeaponInventory inventory;
    [SerializeField] private GameObject scopePanel;
    [SerializeField] private GameObject crosshair;

    private void Start()
    {
        scopePanel.SetActive(false);
    }

    private void Update()
    {
        var weapon = inventory.CurrentWeapon;
        var rd = weapon?.Data as RangedWeaponData;
        bool active = shooter.IsAiming && rd != null && rd.useScope;

        if (scopePanel.activeSelf != active)
        {
            scopePanel.SetActive(active);
            SetWeaponVisible(!active);
            if (crosshair != null) crosshair.SetActive(!active);
        }
    }

    private void SetWeaponVisible(bool visible)
    {
        var root = inventory.CurrentWeapon?.Root;
        if (root == null) return;
        foreach (var r in root.GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }
}
