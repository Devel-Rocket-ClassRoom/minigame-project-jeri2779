using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HistoryPanel : MonoBehaviour
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
    private int limit = 10;

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
            ScoreManager.Instance?.StartHistoryListener(limit, Render);
    }

    private void OnDisable()
    {
        // 패널이 닫히면 실시간 리스너 정리
        ScoreManager.Instance?.StopHistoryListener();

        if (!pushed)
            return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    public void Close() => gameObject.SetActive(false);

    // 토글 ON: 실시간 구독, OFF: 새로고침 버튼으로만 갱신
    private void OnRealtimeToggled(bool isOn)
    {
        var mgr = ScoreManager.Instance;
        if (mgr == null) return;

        if (isOn)
            mgr.StartHistoryListener(limit, Render); // 내부에서 기존 리스너 정리 후 재구독
        else
            mgr.StopHistoryListener();
    }

    private async UniTaskVoid Load()
    {
        if (ScoreManager.Instance == null)
            return;
        await UniTask.WaitUntil(() => ScoreManager.Instance.IsReady);

        List<ScoreData> list = await ScoreManager.Instance.LoadHistoryAsync(limit);
        Render(list);
    }

    private void Render(List<ScoreData> list)
    {
        EnsureRows(list.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            if (i < list.Count)
            {
                var e = list[i];
                rows[i].gameObject.SetActive(true);
                rows[i].Set($"{e.score}", $"R{e.round}", FormatDuration(e.playTimeSec));
            }
            else
            {
                rows[i].gameObject.SetActive(false);
            }
        }

        if (bestScoreText != null)
            bestScoreText.text = ScoreManager.Instance.CachedBestScore > 0 ? $"최고 기록: {ScoreManager.Instance.CachedBestScore}" : "기록 없음";
    }

    private void EnsureRows(int count)
    {
        while (rows.Count < count)
        {
            var go = Instantiate(rowPrefab, rowContainer);
            rows.Add(go.GetComponent<FirebaseListRow>());
        }
    }
    public static string FormatDuration(float seconds)
{
    int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
    return $"{total / 60}:{total % 60:00}";  
}
}
