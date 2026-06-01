using System;
using UnityEngine;

// 플레이어 제어 모드 (상호배타 — 한 번에 하나만)
public enum PlayerControlState
{
    Active, // 이동·전투 가능
    Frozen, // 봉쇄 (상점/인트로/정지) — 사망은 아님
    Dead, // 사망
}

// 플레이어의 "지금 조작 가능한가"를 단일 소유하는 제어 FSM.
// 게임 로직 없음 — 상태 보관 + 전이 + CharacterHealth.OnDied 미러링만 담당.
// 컴포넌트명을 enum(PlayerControlState)과 다르게 둔 이유: 동명 시 컴파일 충돌.
[RequireComponent(typeof(CharacterHealth))]
public class PlayerControl : MonoBehaviour
{
    public PlayerControlState ControlState { get; private set; } = PlayerControlState.Active;

    // 상태 전이 시 발행 (Phase 1 구독자 없음 — 소비자 생기면 연결)
    public event Action<PlayerControlState> OnControlStateChanged;

    private CharacterHealth health;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        SetControlState(PlayerControlState.Dead);
    }

    // 외부(상점/인트로/정지)에서 봉쇄 요청. Dead 중엔 무시 (사망 우선).
    public void Freeze()
    {
        if (ControlState == PlayerControlState.Dead)
            return;
        SetControlState(PlayerControlState.Frozen);
    }

    // 외부에서 봉쇄 해제. Dead 중엔 무시 (죽은 채 부활 방지).
    public void Unfreeze()
    {
        if (ControlState == PlayerControlState.Dead)
            return;
        SetControlState(PlayerControlState.Active);
    }

    public void SetControlState(PlayerControlState next)
    {
        if (ControlState == next)
            return;
        ControlState = next;
        Debug.Log($"[PlayerControl] {ControlState} 전환");
        OnControlStateChanged?.Invoke(next);
    }
}
