using UnityEngine;
using UnityEngine.UI;

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
