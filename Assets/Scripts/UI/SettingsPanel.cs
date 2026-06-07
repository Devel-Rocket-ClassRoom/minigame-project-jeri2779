using UnityEngine;

// 설정 패널의 ESC 닫기 생명주기만 소유한다. 설정 "내용"은 SettingsController가 담당(역할 분리).
// 여는 경로가 여럿(메인메뉴/일시정지)이라 호출자에 push/pop을 맡기지 않고, 패널 활성 상태로 자기관리한다.
// 어떤 경로로 닫혀도(ESC/닫기버튼/부모 비활성) OnDisable이 스택을 정리한다.
public class SettingsPanel : MonoBehaviour
{
    private bool pushed;

    private void OnEnable()
    {
        var ui = UIManager.Instance;
        if (ui == null) return;
        ui.PushEscape(Close);
        pushed = true;
    }

    private void OnDisable()
    {
        if (!pushed) return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    // ESC 스택 또는 닫기 버튼이 호출. 실제 pop은 OnDisable이 책임진다.
    public void Close() => gameObject.SetActive(false);
}
