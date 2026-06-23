using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

 
public class CustomNamePanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI presentNameText;

    [SerializeField]
    private TMP_InputField newNameInput;

    [SerializeField]
    private Button applyButton;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private GameObject returnPanel; // 닫을 때 복귀할 패널

    private bool pushed;
    private bool busy; // 비동기 갱신 중 중복 클릭 방지

    private void Awake()
    {
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyClicked);
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

        var profile = ProfileManager.Instance != null ? ProfileManager.Instance.cachedProfile : null;
        if (presentNameText != null)
            presentNameText.text = profile != null ? profile.nickname : "-";
    }

    private void OnDisable()
    {
        if (!pushed)
            return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    private void OnApplyClicked() => Apply().Forget();

    private async UniTaskVoid Apply()
    {
        if (busy || ProfileManager.Instance == null)
            return;

        string nick = newNameInput != null ? newNameInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(nick))
            return;

        busy = true;
        var (success, error) = await ProfileManager.Instance.UpdateNickNameAsync(nick);
        busy = false;

        if (success)
        {
            if (presentNameText != null)
                presentNameText.text = nick;
            Close();
        }
        else
        {
            FirebaseLog.Warn("ProfileUI", $"닉네임 변경 실패: {error}");
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (returnPanel != null)
            returnPanel.SetActive(true);
    }
}
