using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

 
public class LeaderboardPanel : MonoBehaviour
{
    [SerializeField]
    private Transform rowContainer; // Scroll View/Viewport/Content

    [SerializeField]
    private GameObject rowPrefab; // FirebaseListRow

    [SerializeField]
    private TextMeshProUGUI bestScoreText;

    [SerializeField]
    private Button refreshButton;

    [SerializeField]
    private Toggle realtimeToggle;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private int topN = 10;

    private readonly List<FirebaseListRow> rows = new List<FirebaseListRow>();
    private bool pushed;

    private void Awake()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => Load().Forget());
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        if (realtimeToggle != null)
            realtimeToggle.onValueChanged.AddListener(OnRealtimeToggled);
    }

    private void OnEnable()
    {
        var ui = UIManager.Instance;
        if (ui != null)
        {
            ui.PushEscape(Close);
            pushed = true;
        }
        Load().Forget();

        // 토글이 켜진 채 다시 열면 실시간 구독 재개
        if (realtimeToggle != null && realtimeToggle.isOn)
            StartRealtime();
    }

    private void OnDisable()
    {
        // 패널이 닫히면 실시간 구독/리스너 정리
        StopRealtime();

        if (!pushed) return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    public void Close() => gameObject.SetActive(false);

    // 토글 ON: 실시간 구독, OFF: 새로고침 버튼으로만 갱신
    private void OnRealtimeToggled(bool isOn)
    {
        if (isOn)
            StartRealtime();
        else
            StopRealtime();
    }

    private void StartRealtime()
    {
        var mgr = LeaderBoardManager.Instance;
        if (mgr == null) return;

        mgr.OnLeaderboardUpdated -= Render; // 중복 구독 방지
        mgr.OnLeaderboardUpdated += Render;
        mgr.StartRealtimeListener(topN);
    }

    private void StopRealtime()
    {
        var mgr = LeaderBoardManager.Instance;
        if (mgr == null) return;

        mgr.OnLeaderboardUpdated -= Render;
        mgr.StopRealtimeListener();
    }

    private async UniTaskVoid Load()
    {
        if (LeaderBoardManager.Instance == null) return;
        await UniTask.WaitUntil(() => LeaderBoardManager.Instance.IsReady);

        List<LeaderboardEntry> list = await LeaderBoardManager.Instance.LoadLeaderboardAsync(topN);
        Render(list);
    }

    private void Render(List<LeaderboardEntry> list)
    {
        EnsureRows(list.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            if (i < list.Count)
            {
                var e = list[i];
                rows[i].gameObject.SetActive(true);
                rows[i].Set($"{i + 1}", string.IsNullOrEmpty(e.nickname) ? "-" : e.nickname, $"{e.score} (R{e.round})");
            }
            else
            {
                rows[i].gameObject.SetActive(false);
            }
        }

        if (bestScoreText != null)
            bestScoreText.text = list.Count > 0 ? $"TOP {list[0].score}" : "기록 없음";
    }

    private void EnsureRows(int count)
    {
        while (rows.Count < count)
        {
            var go = Instantiate(rowPrefab, rowContainer);
            rows.Add(go.GetComponent<FirebaseListRow>());
        }
    }
}
