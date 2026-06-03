using System;
using UnityEngine;

// 플레이어 전투 이벤트 허브(③). 무기가 명중을 "발행"하고, 행동 컴포넌트(폭발 등)가 "구독"한다.
// 무기는 누가 듣는지 모르고, 행동은 누가 쐈는지 모른다 — 도메인 간 직접 명령을 없애는 seam.
public class PlayerCombatEvents : MonoBehaviour
{
    // 원거리 무기 명중 (월드 좌표). 폭발탄 등이 구독한다.
    public event Action<Vector3> OnRangedHit;

    public void RaiseRangedHit(Vector3 point) => OnRangedHit?.Invoke(point);
}
