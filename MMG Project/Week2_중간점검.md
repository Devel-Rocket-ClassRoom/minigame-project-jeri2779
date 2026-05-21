# 2주차 작업 계획 — 중간 점검 (2026-05-21, 4일차 논의 반영)

> 1주차 빌드 직전 시점. 이 문서는 다음 세션에서 이어 작업하기 위한 통합 기록.
> 실제 진행에 따라 수정 가능. 최종 확정이 아님.

---

## 1. 2주차 비전

> **"IWeapon/EnemyBase 기반 구조 전환 + 무기/적 다양성 확보"**

### 어필 포인트
- 무기 추가 = 해당 스크립트 작성만 (기존 코드 수정 없음)
- 적 추가 = EnemyBase 자식 클래스만 작성
- 상황에 따라 인터페이스/상속을 다르게 선택한 설계 판단
- 1:1 직접 참조 점진적 제거

### 주차별 흐름
| 주차 | 어필 포인트 |
|---|---|
| 1주차 | 게임이 처음~끝까지 동작 |
| **2주차** | **IWeapon/EnemyBase 구조 전환 + 무기/적 다양성 확보** |
| 3주차 | 중앙관리 구조 + 비주얼 완성 + 리팩토링 |

---

## 2. 가용 시간

| 항목 | 시간 |
|---|---|
| 월~목 (4일) × 8h | 32h |
| 토·일 (3.5h × 2) | 7h |
| 명목 총합 | 39h |
| 손실 보정 (20~25%) | -8~10h |
| **실 가용 (평일)** | **약 22~24h** |
| **주말 (보강 시간)** | **약 5~7h** |

> 금요일 오전 빌드 → **목요일 밤까지 평일 마감**. 주말은 보강 시간.

---

## 3. 무기 시스템 설계 (4일차 확정)

### IWeapon 인터페이스
```csharp
public interface IWeapon
{
    bool TryFire();
    void TryReload();
    WeaponData Data { get; }
}
```

### 클래스별 역할

#### WeaponData (SO)
- 변하지 않는 수치 보관
- 필드: `damage`, `fireRate`, `magazineSize`, `reloadTime`, `range`, `verticalRecoil`, `maxReserveAmmo`, `price`, `weaponModelPrefab`, `category(enum)`
- 계산/상태/행동 없음

#### RangedWeapon : IWeapon (프리팹 부착)
- ammo 상태, isReloading, fireRate 쿨다운
- `TryFire()`: ammo 체크 → 감소 → 머즐플래시/애니메이터 → bool 반환
- Raycast, 데미지 계산, 외부 시스템 참조 없음

#### MeleeWeapon : IWeapon (프리팹 부착)
- 쿨다운 상태
- `TryFire()`: OverlapSphere → 범위 내 IDamageable에 데미지 직접 처리

#### ThrowableWeapon : IWeapon (프리팹 부착)
- 소지 개수 상태
- `TryFire()`: 투사체 Instantiate → 물리 발사

#### WeaponInventory (Player)
- 4슬롯 관리 (Primary/Secondary/Melee/Throwable)
- 1/2/3/4키 슬롯 전환
- 상점 구매 시 `EquipToSlot(slot, data)`: 기존 인스턴스 파괴 → 새 프리팹 Instantiate
- `CurrentWeapon : IWeapon` 프로퍼티 노출

#### CharacterCombat (← CharacterShooter 개명, Player)
- 입력 수신 → `inventory.CurrentWeapon.TryFire()` 호출
- 총기 성공 시: Raycast → 헤드샷 판정 → 데미지 계산 → `IDamageable.TakeDamage()`
- 반동: `CharacterMoves.AddRecoil()` 호출
- 근접/투척은 TryFire() 내부에서 데미지 자체 처리

### 책임 분리 원칙
| 작업 | 누가 안다 | 누가 계산 | 누가 적용 |
|---|---|---|---|
| 데미지 수치 | WeaponData.damage | CharacterCombat (총기) / 각 무기 (근접/투척) | IDamageable.TakeDamage |
| ammo 현재량 | RangedWeapon.currentAmmo | RangedWeapon.TryFire | RangedWeapon 자체 |
| fireRate 쿨다운 | WeaponData.fireRate | RangedWeapon.TryFire | RangedWeapon 자체 |
| 반동 수치 | WeaponData.verticalRecoil | 없음 | CharacterMoves.AddRecoil |
| 헤드샷 배율 | CharacterCombat (고정 2배) | CharacterCombat | 데미지 계산에 반영 |
| 공격력 배율 | CharacterStats.AttackMultiplier | CharacterCombat | 데미지 계산에 반영 |

---

## 4. 적 시스템 설계 (4일차 확정)

### EnemyBase 상속 구조
- 공통 필드(HP, NavMeshAgent, FSM)가 많아 인터페이스보다 상속이 적합
- `Attack()`만 override, 공통 FSM(Chase/Dead 등)은 EnemyBase가 처리

```
EnemyBase : MonoBehaviour
    ├─ MeleeEnemy  : EnemyBase  → override Attack() (근접 콜라이더)
    ├─ ChargeEnemy : EnemyBase  → override Attack() (돌진) + ChargeReady/Charge 상태 추가
    └─ RangedEnemy : EnemyBase  → override Attack() (투사체)
```

- 현재 EnemyController.cs → EnemyBase로 전환
- 적 추가 시 자식 클래스만 작성, EnemyBase 수정 없음

---

## 5. 작업 우선순위

### 🔴 위험 작업 (평일 집중)
| 순위 | 항목 | 추정 |
|---|---|---|
| 1 | IWeapon 도입 + RangedWeapon 분리 + CharacterCombat 개명 + WeaponInventory 실 발사 연결 | 5.5~9h |
| 2 | EnemyBase 전환 + ChargeEnemy 분리 (돌진형) | 4~5h |
| 3 | RangedEnemy 분리 (원거리형) | 5~7h |

### 🟢 안전 작업 (주말 + 시간 남으면)
| 항목 | 추정 |
|---|---|
| WeaponInventory 4슬롯 확장 | 1~2h |
| 상점 돈 차감 연결 | 1h |
| 상점 비용 표시 / 잠금 UI | 1~2h |
| HUD 탄약 표시 현재 무기 기준 갱신 | 30m~1h |
| 라운드 대기 카운트다운 UI | 1h |

### ❌ 2주차 제외 (3주차 이후)
- 적 외관 애셋 적용
- GameStateController 도입
- KillTracker 분리
- HUD 이벤트 구독 전환
- 발사/리로드 애니메이션 동기화
- 저장 시스템
- 사운드/이펙트

---

## 6. 일자별 일정

### Day 1 (월) — IWeapon + RangedWeapon 분리
- 오전: IWeapon 인터페이스 작성 + RangedWeapon.cs 작성
- 오후: CharacterCombat 개명 + IWeapon 참조로 교체
- **마감 체크**: 코드 컴파일 통과, 기존 사격 동작 유지

### Day 2 (화) — 무기 시스템 완성
- 오전: WeaponInventory 실 발사 연결, HUD 참조 갱신
- 오후: 풀 사격/리로드/슬롯 전환 검증
- **마감 체크**: 무기 시스템 완전 동작

### Day 3 (수) — EnemyBase + ChargeEnemy
- EnemyController → EnemyBase 전환
- ChargeEnemy 분리 (ChargeReady/Charge FSM)
- **마감 체크**: 돌진형 완전 동작

### Day 4 (목) — RangedEnemy + 빌드 마감
- 오전: RangedEnemy 분리 + 투사체 시스템
- 오후: 풀 플레이 검증
- **저녁: 빌드 마감**
- 폴백: 투사체 막히면 Raycast 즉발로 전환

### 토·일 (보강)
- WeaponInventory 4슬롯 확장
- 상점 돈 차감 연결, 비용 표시, HUD 갱신
- 여유 시: 근접/투척 MeleeWeapon/ThrowableWeapon 스텁 작성

---

## 7. 위험 요소 및 폴백

### IWeapon 도입 + CharacterCombat 전환
- **위험**: CharacterShooter → CharacterCombat 전환 중 사격 일시 불가 구간 발생
- **대응**: 인터페이스 도입 → 컴파일 확인 → 개명 순서로 단계별 진행

### RangedEnemy (가장 큰 위험)
- **위험**: 투사체 시스템 첫 도입, Day 4 마감과 겹침
- **폴백**: Raycast 즉발 공격 (Day 4 오전까지 투사체 안 되면 즉시 전환)

### 적 외관
- 2주차 빌드에서도 큐브 상태 유지 가능 — 3주차 메인 작업

---

## 8. 1주차 빌드에서 이월된 항목

- 상점 돈 차감 미연결 (무료 업그레이드 상태)
- 무기 전환은 시각만, 실 발사는 AR 기준
- HUD 탄약 항상 AR 기준
- 적 외관 큐브 상태
- Enemy.prefab Head Missing Script (빌드 전 정리 필요)

---

## 9. 미결정 사항

| 항목 | 비고 |
|---|---|
| 슬롯 전환 시 리로드 처리 | A안(중단) / B안(유지) — 작업 진입 시 결정 |
| 근접/투척 데미지 계산 주체 | TryFire() 내부 vs CharacterCombat — 작업 진입 시 결정 |
| MeleeWeapon/ThrowableWeapon 구현 시점 | 2주차 후반 또는 3주차 |

---

## 10. 3주차 윤곽 (참고)

### 메인 작업
- KillTracker 분리 + 폴링 → 이벤트 전환
- 외관 완성 (적 모델 + 애니메이션)
- 발사/리로드 애니메이션 동기화

### 보조 작업
- 사운드/이펙트
- 카메라 흔들림 / 피격 화면 효과
- 저장 시스템 (시간 남으면)

---

## 11. 다음 세션 시작점

### 작업 진입 전 확인할 것
- 슬롯 전환 시 리로드 처리 (A안 중단 / B안 유지) 결정
- WeaponCategory enum 항목 확정 (Primary / Secondary / Melee / Throwable)
- CharacterCombat 개명 시 씬 내 컴포넌트 참조 갱신 필요

---

> 이 문서는 1주차 빌드 직전 + 4일차 설계 논의 반영 통합본입니다.
> 위험 작업이 막힐 경우 폴백 옵션을 우선 적용합니다.
