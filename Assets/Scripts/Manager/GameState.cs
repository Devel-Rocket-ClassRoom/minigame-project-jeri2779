// 게임 흐름의 최상위 단계. GameManager가 소유하는 단일 진실원.
// 전체 상태 공간을 여기서 선언하고, 실제 전환 배선은 단계적으로 채운다.
public enum GameState
{
    MainMenu, // 기본값(0). 메인 메뉴 표시 중
    Intro, // 시작 연출 (플레이어 배치 ~ 첫 라운드 시작 전)
    Playing, // 라운드 진행 중
    Shopping, // 라운드 사이 상점 페이즈 (2단계: RoundManager 이벤트로 연결 예정)
    Paused, // 일시정지 (2단계: PausePanel 연결 예정)
    GameOver, // 플레이어 사망
    Clear, // 전체 라운드 클리어 (2단계: RoundManager 이벤트로 연결 예정)
}
