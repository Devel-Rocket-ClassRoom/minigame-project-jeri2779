using UnityEngine;

/// <summary>
/// LPSP 팔 애니메이션 이벤트 수신기.
/// SK_FP_CH_Default_Root에 부착. 나중에 무기 시스템에서 Action을 구독해 사용.
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    public System.Action OnReloadEnded;
    public System.Action OnHolsterEnded;
    public System.Action OnInspectEnded;
    public System.Action<int> OnMagazineActiveChanged;
    public System.Action OnCasingEjected;

    // 애니메이션 이벤트 수신 메서드들
    private void OnEjectCasing() => OnCasingEjected?.Invoke();
    private void OnAmmunitionFill(int amount) { }
    private void OnSetActiveKnife(int active) { }
    private void OnGrenade() { }
    private void OnSetActiveMagazine(int active) => OnMagazineActiveChanged?.Invoke(active);
    private void OnAnimationEndedBolt() { }
    private void OnAnimationEndedReload() => OnReloadEnded?.Invoke();
    private void OnAnimationEndedGrenadeThrow() { }
    private void OnAnimationEndedMelee() { }
    private void OnAnimationEndedInspect() => OnInspectEnded?.Invoke();
    private void OnAnimationEndedHolster() => OnHolsterEnded?.Invoke();
    private void OnSlideBack(int back) { }
}
