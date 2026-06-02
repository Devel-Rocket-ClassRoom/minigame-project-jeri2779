using UnityEngine;

// 게임 흐름 조율자. 진행/정지 상태를 소유하고, 라운드 실행은 RoundManager에 명령한다.
public class GameManager : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private CharacterHealth playerHealth;

    // 게임 진행 정지 상태 (게임오버/중단). 기존 EnemySpawner.isGameStopped 이관.
    public bool IsStopped { get; private set; }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += StopGame;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= StopGame;
    }

    // 메인메뉴 → 인트로 진입
    public void BeginIntro()
    {
        roundManager.BeginIntro();
    }

    // 인트로 후 게임 시작
    public void StartGame()
    {
        IsStopped = false;
        roundManager.BeginFirstRound();
    }

    // 사망 등으로 게임 흐름 정지
    public void StopGame()
    {
        IsStopped = true;
        roundManager.HaltRounds();
    }
}
