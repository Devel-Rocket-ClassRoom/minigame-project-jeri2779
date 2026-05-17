using UnityEngine;

/// <summary>
/// LPSP 무기 애니메이션 이벤트 수신기.
/// P_LPSP_WEP_AR_01 등 무기 오브젝트에 부착.
/// </summary>
public class WeaponAnimationEventReceiver : MonoBehaviour
{
    public System.Action OnCasingEjected;

    private void OnEjectCasing() => OnCasingEjected?.Invoke();
}
