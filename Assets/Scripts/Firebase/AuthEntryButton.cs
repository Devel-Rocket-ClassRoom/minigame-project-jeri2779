using Cysharp.Threading.Tasks;
using Michsky.UI.Dark;
using UnityEngine;
using UnityEngine.UI;

 
public class AuthEntryButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private ButtonManager buttonManager; // 라벨 텍스트(buttonText) 갱신용

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject userInfoPanel;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    private void OnEnable()
    {
        BindAuth().Forget();
    }

     
    private async UniTaskVoid BindAuth()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);
        if (!isActiveAndEnabled)
            return;
        AuthManager.Instance.LoginStateChanged -= OnLoginStateChanged; // 중복 구독 방지
        AuthManager.Instance.LoginStateChanged += OnLoginStateChanged;
        RefreshLabel(AuthManager.Instance.IsLoggedIn);
    }

    private void OnDisable()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.LoginStateChanged -= OnLoginStateChanged;
    }

    private void OnLoginStateChanged(bool loggedIn) => RefreshLabel(loggedIn);

    private void RefreshLabel(bool loggedIn)
    {
        if (buttonManager == null)
            return;
        buttonManager.buttonText = loggedIn ? "PROFILE" : "LOGIN";
        buttonManager.UpdateUI();
        
        if (transform.parent is RectTransform parent)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
    }

    private void OnClicked()
    {
        bool loggedIn = AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn;
        var target = loggedIn ? userInfoPanel : loginPanel;
        
        if (target != null) target.SetActive(true);
    }
}
