using UnityEngine;
using UnityEngine.UI;


public class PanelOpenButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private GameObject panel;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(Open);
    }

    private void Open()
    {
        if (panel != null)
            panel.SetActive(true);
    }
}
