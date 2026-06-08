using UnityEngine;


// RoundManager는 명령(Freeze/PlaceForIntro/ResetForRound)만 내리고,
// 실제 위치/체력/탄약/스태미너 리셋은 플레이어가 자기 컴포넌트로 처리한다.
[RequireComponent(typeof(CharacterMoves))]
[RequireComponent(typeof(CharacterHealth))]
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private WeaponInventory weaponInventory;

    private CharacterMoves characterMoves;
    private CharacterHealth characterHealth;

    private void Awake()
    {
        characterMoves = GetComponent<CharacterMoves>();
        characterHealth = GetComponent<CharacterHealth>();
    }

    // 조작 봉쇄 (게임 시작 직후 / 클리어 등)
    public void Freeze()
    {
        characterMoves.SetMovable(false);
    }

    // 조작 허용 (라운드 시작 등) — 위치는 건드리지 않음
    public void Unfreeze()
    {
        characterMoves.SetMovable(true);
    }

    // 인트로 진입: 스폰 위치 배치 + 조작 허용
    public void PlaceForIntro()
    {
        characterMoves.Teleport(spawnPoint.position);
        characterMoves.SetMovable(true);
    }

    // 라운드 시작 전 리셋: 봉쇄 + 위치 + 체력/탄약/스태미너
    public void ResetForRound()
    {
        characterMoves.SetMovable(false);
        characterMoves.Teleport(spawnPoint.position);
        characterHealth.ResetHealth();
        weaponInventory?.ResetAmmo();
        characterMoves.ResetStamina();
    }
}
