using UnityEngine;

// [진단용] 빌드/에디터에서 FPS를 화면 좌상단에 표시한다.
// 씬 배치 불필요 — 게임 시작 시 자동 생성된다. 측정이 끝나면 이 파일을 삭제하면 됨.
public class FpsDisplay : MonoBehaviour
{
    private float smoothedDelta;
    private GUIStyle style;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("FpsDisplay");
        DontDestroyOnLoad(go);
        go.AddComponent<FpsDisplay>();
    }

    private void Update()
    {
        // 프레임 시간 평활화(튀는 값 완화)
        smoothedDelta += (Time.unscaledDeltaTime - smoothedDelta) * 0.1f;
    }

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.green;
        }

        float ms = smoothedDelta * 1000f;
        float fps = smoothedDelta > 0f ? 1f / smoothedDelta : 0f;
        GUI.Label(new Rect(12, 10, 500, 40), $"{fps:0.} FPS  ({ms:0.0} ms)", style);
    }
}
