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
    }

    private void OnDisable()
    {
        if (!pushed)
            return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    public void Close() => gameObject.SetActive(false);

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
