using UnityEngine;

//메인메뉴 애니메이션 재생용(임시)
[RequireComponent(typeof(Animator))]
public class PanelIntroPlayer : MonoBehaviour
{
    [SerializeField] private string stateName = "Panel In";

    private void OnEnable()
    {
        GetComponent<Animator>().Play(stateName, 0, 0f);
    }
}
