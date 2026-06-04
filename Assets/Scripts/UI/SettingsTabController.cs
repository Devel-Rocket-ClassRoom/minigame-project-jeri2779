using UnityEngine;
using UnityEngine.UI;

public class SettingsTabController : MonoBehaviour
{
    [SerializeField] private Toggle[] tabToggles;
    [SerializeField] private GameObject[] tabPanels;
    [SerializeField] private int defaultTab = 0;


    private const string AnimStateOn = "Toggle On";
    private const string AnimStateOff = "Toggle Off";


    private void Awake()
    {
        for (int i = 0; i < tabToggles.Length; i++)
        {
            int index = i;
            if (tabToggles[i] == null) continue;
            tabToggles[i].onValueChanged.AddListener(on =>
            {
                if (on) SelectTab(index);
            });
        }
    }

    private void OnEnable()
    {
        SelectTab(defaultTab);
    }

    public void SelectTab(int index)
    {
        
        if (index >= 0 && index < tabToggles.Length && tabToggles[index] != null)
            tabToggles[index].isOn = true;

        for (int i = 0; i < tabToggles.Length; i++)
        {
            if (tabToggles[i] == null) continue;
            var anim = tabToggles[i].GetComponent<Animator>();
            if (anim == null) continue;

            if (i == index)
                anim.Play(AnimStateOn); // 선택: 강조 애니 재생
            else
                anim.Play(AnimStateOff, 0, 1f); // 해제: 끝 프레임으로 즉시(애니 무처리)
        }

        for (int i = 0; i < tabPanels.Length; i++)
            if (tabPanels[i] != null) tabPanels[i].SetActive(i == index);
    }
}
