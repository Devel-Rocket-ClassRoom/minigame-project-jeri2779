using UnityEngine;
using UnityEngine.InputSystem;

// 인게임에서 Tab으로 조작 안내(ControlsGuideImage)를 즉시 켜고 끈다. 초반에 보고 끄는 용도.
// 대상이 꺼지면 그 위의 스크립트가 멈추므로, 항상 활성인 부모(PlayingHUD)에 붙여 참조로 토글한다.
public class ControlsGuideToggle : MonoBehaviour
{
    [SerializeField] private GameObject controlsGuide;

    private InputAction toggleAction;

    private void Awake()
    {
        toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/tab");
        toggleAction.performed += OnToggle;
    }

    private void Start()
    {
        // 게임 실행 시 항상 켜진 상태로 시작 (SaveData 비의존). 이후 Tab으로만 토글.
        if (controlsGuide != null)
            controlsGuide.SetActive(true);
    }

    private void OnEnable() => toggleAction.Enable();

    private void OnDisable() => toggleAction.Disable();

    private void OnDestroy()
    {
        toggleAction.performed -= OnToggle;
        toggleAction.Dispose();
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        if (controlsGuide != null)
            controlsGuide.SetActive(!controlsGuide.activeSelf);
    }
}
