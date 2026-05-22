using UnityEngine;
using System.Collections;

public class ProceduralWeaponAnimator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Transform slideTransform;
    [SerializeField] private Transform magazineTransform;

    [Header("Fire Recoil")]
    [SerializeField] private Vector3 fireRecoilPos = new Vector3(0, 0, -0.1f);
    [SerializeField] private Vector3 fireRecoilRot = new Vector3(-5f, 0, 0);
    [SerializeField] private float fireSlideBackDist = 0.05f;
    [SerializeField] private float fireRecoverSpeed = 10f;

    [Header("Draw Animation")]
    [SerializeField] private Vector3 drawOffsetPos = new Vector3(0, -0.5f, 0);
    [SerializeField] private float drawSpeed = 5f;

    [Header("Reload Animation")]
    [SerializeField] private Vector3 reloadDownPos = new Vector3(0, -0.3f, -0.1f);
    [SerializeField] private Vector3 reloadDownRot = new Vector3(20f, 0, 0);
    [SerializeField] private float reloadActionTime = 0.5f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 slideOriginalPos;
    private Vector3 magazineOriginalPos;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private float slideTargetZ;

    private Coroutine currentRoutine;
    private bool isReloading;

    private void Awake()
    {
        if (weaponRoot == null) weaponRoot = transform;
        
        originalPos = weaponRoot.localPosition;
        originalRot = weaponRoot.localRotation;
        
        targetPos = originalPos;
        targetRot = originalRot;

        if (slideTransform != null)
        {
            slideOriginalPos = slideTransform.localPosition;
            slideTargetZ = slideOriginalPos.z;
        }

        if (magazineTransform != null)
        {
            magazineOriginalPos = magazineTransform.localPosition;
        }
    }

    private void Update()
    {
        if (!isReloading)
        {
            weaponRoot.localPosition = Vector3.Lerp(weaponRoot.localPosition, targetPos, Time.deltaTime * fireRecoverSpeed);
            weaponRoot.localRotation = Quaternion.Slerp(weaponRoot.localRotation, targetRot, Time.deltaTime * fireRecoverSpeed);
            
            // Gradually return to original state
            targetPos = Vector3.Lerp(targetPos, originalPos, Time.deltaTime * fireRecoverSpeed);
            targetRot = Quaternion.Slerp(targetRot, originalRot, Time.deltaTime * fireRecoverSpeed);
        }

        if (slideTransform != null)
        {
            Vector3 sPos = slideTransform.localPosition;
            sPos.z = Mathf.Lerp(sPos.z, slideTargetZ, Time.deltaTime * fireRecoverSpeed);
            slideTransform.localPosition = sPos;
            
            slideTargetZ = Mathf.Lerp(slideTargetZ, slideOriginalPos.z, Time.deltaTime * fireRecoverSpeed);
        }
    }

    public void PlayFire()
    {
        if (isReloading) return;

        // Apply instant recoil
        targetPos += weaponRoot.localRotation * fireRecoilPos;
        targetRot *= Quaternion.Euler(fireRecoilRot);

        if (slideTransform != null)
        {
            slideTargetZ = slideOriginalPos.z - fireSlideBackDist;
        }
    }

    public void PlayDraw()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DrawRoutine());
    }

    private IEnumerator DrawRoutine()
    {
        isReloading = false;
        weaponRoot.localPosition = originalPos + drawOffsetPos;
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * drawSpeed;
            weaponRoot.localPosition = Vector3.Lerp(originalPos + drawOffsetPos, originalPos, t);
            yield return null;
        }
        weaponRoot.localPosition = originalPos;
    }

    public void PlayReload(float totalTime)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ReloadRoutine(totalTime));
    }

    private IEnumerator ReloadRoutine(float totalTime)
    {
        isReloading = true;
        float halfTime = totalTime * 0.5f;

        // Move Down
        float t = 0;
        Vector3 downPos = originalPos + reloadDownPos;
        Quaternion downRot = originalRot * Quaternion.Euler(reloadDownRot);

        while (t < 1f)
        {
            t += Time.deltaTime / (halfTime * 0.5f);
            weaponRoot.localPosition = Vector3.Lerp(originalPos, downPos, t);
            weaponRoot.localRotation = Quaternion.Slerp(originalRot, downRot, t);
            yield return null;
        }

        // Action (Simulate mag swap if needed)
        if (magazineTransform != null)
        {
            magazineTransform.localPosition = magazineOriginalPos + Vector3.down * 0.2f;
            yield return new WaitForSeconds(halfTime * 0.5f);
            magazineTransform.localPosition = magazineOriginalPos;
        }
        else
        {
            yield return new WaitForSeconds(halfTime * 0.5f);
        }

        // Move Up
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / (halfTime * 0.5f);
            weaponRoot.localPosition = Vector3.Lerp(downPos, originalPos, t);
            weaponRoot.localRotation = Quaternion.Slerp(downRot, originalRot, t);
            yield return null;
        }

        weaponRoot.localPosition = originalPos;
        weaponRoot.localRotation = originalRot;
        isReloading = false;
    }
}
