using UnityEngine;

public class ThrowableWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private ThrowableWeaponData data;

    private CharacterStats stats;
    private Camera playerCamera;
    private int currentCount;
    private bool pinPulled;
    private bool wasFiring;

    public WeaponData Data => data;
    public int CurrentAmmo => currentCount;
    public int ReserveAmmo => 0;
    public bool IsReloading => false;
    public GameObject GameObject => gameObject;
    public Transform Root => transform;

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
        playerCamera = stats.GetComponentInChildren<Camera>();
        currentCount = data.maxCount;
    }

    public bool Use(FireContext ctx)
    {
        if (currentCount <= 0) return false;
        pinPulled = true;
        return true;
    }

    public void Tick(FireContext ctx)
    {
        if (wasFiring && !ctx.isFiring && pinPulled)
            Throw();
        wasFiring = ctx.isFiring;
    }

    public void CancelAction()
    {
        pinPulled = false;
    }

    public void TryReload() { }

    public void ResetAmmo()
    {
        currentCount = data.maxCount;
    }

    private void Throw()
    {
        if (data.grenadePrefab == null)
        {
            Debug.LogError($"{data.name}에 grenadePrefab이 없습니다.");
            pinPulled = false;
            return;
        }

        Vector3 spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        var go = Instantiate(data.grenadePrefab, spawnPos, playerCamera.transform.rotation);

        var grenadeCol = go.GetComponent<Collider>();
        if (grenadeCol != null)
            foreach (var c in stats.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(grenadeCol, c);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = playerCamera.transform.forward * data.throwForce;

        go.GetComponent<Grenade>().Init(data);

        currentCount--;
        pinPulled = false;
    }
}
