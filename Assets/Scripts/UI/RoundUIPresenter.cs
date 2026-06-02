using UnityEngine;

// 라운드/클리어 UI 표시 전담. RoundManager의 이벤트만 받아서 보여준다.
// RoundManager는 UI를 모르고, 패널 토글/클리어 화면은 여기서만 한다.
public class RoundUIPresenter : MonoBehaviour
{
    [SerializeField]
    private RoundManager roundManager;

    [SerializeField]
    private GameObject roundClearPanel;

    [SerializeField]
    private GameObject finalClearText;

    [SerializeField]
    private GameClearUI gameClearUI;

    private void OnEnable()
    {
        if (roundManager == null)
            return;
        roundManager.OnRoundCleared += HandleRoundCleared;
        roundManager.OnRoundClearHidden += HandleRoundClearHidden;
        roundManager.OnAllRoundsCleared += HandleAllRoundsCleared;
    }

    private void OnDisable()
    {
        if (roundManager == null)
            return;
        roundManager.OnRoundCleared -= HandleRoundCleared;
        roundManager.OnRoundClearHidden -= HandleRoundClearHidden;
        roundManager.OnAllRoundsCleared -= HandleAllRoundsCleared;
    }

    // 클리어 배너 표시 (최종이면 최종 클리어 텍스트도)
    private void HandleRoundCleared(bool isFinal)
    {
        if (roundClearPanel != null)
            roundClearPanel.SetActive(true);
        if (isFinal && finalClearText != null)
            finalClearText.SetActive(true);
    }

    // 클리어 배너 숨김
    private void HandleRoundClearHidden()
    {
        if (roundClearPanel != null)
            roundClearPanel.SetActive(false);
        if (finalClearText != null)
            finalClearText.SetActive(false);
    }

    // 최종 클리어 화면
    private void HandleAllRoundsCleared()
    {
        if (gameClearUI != null)
            gameClearUI.Show();
    }
}
