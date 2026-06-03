using UnityEngine;

// 게임 흐름 조율자. 진행/정지 상태를 소유하고, 라운드 실행은 RoundManager에 명령한다.
public class GameManager : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private CharacterHealth playerHealth;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private CharacterDead deathPresenter;

    // 게임 흐름의 최상위 단계. 이 한 변수가 단일 진실원.
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // 게임 진행 정지 여부 = 상태에서 파생(병존 플래그 제거). 기존 소비처 호환 유지.
    public bool IsStopped => CurrentState is GameState.GameOver or GameState.Clear;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
        if (roundManager != null)
            roundManager.OnAllRoundsCleared += HandleAllRoundsCleared;
        if (deathPresenter != null)
            deathPresenter.OnDeathPresented += HandleDeathPresented;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
        if (roundManager != null)
            roundManager.OnAllRoundsCleared -= HandleAllRoundsCleared;
        if (deathPresenter != null)
            deathPresenter.OnDeathPresented -= HandleDeathPresented;
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

    // 플레이어 사망 — 흐름 정지 + 게임플레이 UI 숨김(틸트 전). 패널 표시는 연출 완료 후.
    private void HandlePlayerDied()
    {
        StopGame();
        if (gameOverUI != null)
            gameOverUI.HideGameplayUI();
    }

    // 사망 연출(틸트) 완료 — 게임오버 패널 표시
    private void HandleDeathPresented()
    {
        if (gameOverUI != null)
            gameOverUI.Show();
    }

    // 사망 등으로 게임 흐름 정지
    public void StopGame()
    {
        CurrentState = GameState.GameOver;
        roundManager.HaltRounds();
    }
}
