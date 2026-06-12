using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 플레이어 사망 연출 전담(카메라 틸트). UI는 직접 만지지 않고, 연출 완료를 이벤트로 알린다.
// HUD 숨김/게임오버 패널 표시는 GameManager(조율자)가 OnDied/OnDeathPresented를 보고 처리한다.
public class CharacterDead : MonoBehaviour
{
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;

    [SerializeField] private float tiltDuration = 1.5f;

    // 사망 연출(틸트) 완료 후 1회 발행 — GameManager가 구독해 게임오버 패널을 띄운다.
    public event Action OnDeathPresented;

    private bool triggered = false;

    private void Awake()
    {
        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        if (characterHealth != null)
            characterHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (characterHealth != null)
            characterHealth.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (triggered)
            return;

        triggered = true;
        DeathMoment().Forget();
    }

    private async UniTaskVoid DeathMoment()
    {
        await characterMoves.DeathCameraRotate(tiltDuration);
        OnDeathPresented?.Invoke();
    }
}
