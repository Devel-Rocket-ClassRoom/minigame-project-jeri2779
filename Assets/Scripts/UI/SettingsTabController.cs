using UnityEngine;
using UnityEngine.UI;

// 설정창 탭 전환: 탭 토글 ↔ 내용 패널 1:1. ToggleGroup에 의존하지 않고 직접 라디오 동작을 강제한다.
// (MUIP CustomToggle이 Toggle.group 할당을 무시하는 문제 회피)
// 전환 시 isOn을 알림과 함께 변경하되 자기 리스너만 suppress로 무시 → MUIP 토글의 On/Off 강조 애니메이션은 정상 갱신.
public class SettingsTabController : MonoBehaviour
{
    [SerializeField] private Toggle[] tabToggles;
    [SerializeField] private GameObject[] tabPanels;
    [SerializeField] private int defaultTab = 0;

    private bool suppress;

    private void Awake()
    {
        for (int i = 0; i < tabToggles.Length; i++)
        {
            int index = i;
            if (tabToggles[i] == null) continue;
            tabToggles[i].onValueChanged.AddListener(on =>
            {
                if (suppress) return;
                if (on)
                    SelectTab(index);
                else
                {
                    // 활성 탭을 끄려는 클릭 → 다시 켬(최소 1개 유지)
                    suppress = true;
                    tabToggles[index].isOn = true;
                    suppress = false;
                }
            });
        }
    }

    private void OnEnable()
    {
        SelectTab(defaultTab);
    }

    public void SelectTab(int index)
    {
        suppress = true;
        for (int i = 0; i < tabToggles.Length; i++)
            if (tabToggles[i] != null) tabToggles[i].isOn = (i == index); // 알림 발생 → CustomToggle 애니메이션 갱신
        suppress = false;

        for (int i = 0; i < tabPanels.Length; i++)
            if (tabPanels[i] != null) tabPanels[i].SetActive(i == index);
    }
}
