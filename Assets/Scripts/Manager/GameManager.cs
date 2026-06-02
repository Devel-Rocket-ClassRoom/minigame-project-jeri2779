using UnityEngine;

// 게임 흐름 조율자. 진행/정지 상태를 소유하고, 라운드 실행은 RoundManager에 명령한다.
public class GameManager : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private CharacterHealth playerHealth;

    // 게임 흐름의 최상위 단계. 이 한 변수가 단일 진실원.
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // 게임 진행 정지 여부 = 상태에서 파생(병존 플래그 제거). 기존 소비처 호환 유지.
    public bool IsStopped => CurrentState is GameState.GameOver or GameState.Clear;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += StopGame;
        if (roundManager != null)
            roundManager.OnAllRoundsCleared += HandleAllRoundsCleared;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= StopGame;
        if (roundManager != null)
            roundManager.OnAllRoundsCleared -= HandleAllRoundsCleared;
    }

    // 최종 클리어 → 게임 전체 상태를 Clear로 (IsStopped 파생도 true가 됨)
    private void HandleAllRoundsCleared()
    {
        CurrentState = GameState.Clear;
    }

    // 메인메뉴 → 인트로 진입
    public void BeginIntro()
    {
        CurrentState = GameState.Intro;
        roundManager.BeginIntro();
    }

    // 인트로 후 게임 시작
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        roundManager.BeginFirstRound();
    }

    // 사망 등으로 게임 흐름 정지
    public void StopGame()
    {
        CurrentState = GameState.GameOver;
        roundManager.HaltRounds();
    }
}
