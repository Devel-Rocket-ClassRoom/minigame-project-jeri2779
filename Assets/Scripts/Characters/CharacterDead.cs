using System.Collections;
using UnityEngine;

public class CharacterDead : MonoBehaviour
{
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] private GameObject gameOverUI;

    [SerializeField] private float tiltDuration = 1.5f;

    private bool triggered = false;

    private void Update()
    {
        if (!triggered && characterHealth.State == CharacterHealth.CharacterState.Dead)
        {
            triggered = true;
            StartCoroutine(DeathMoment());
        }
    }

    private IEnumerator DeathMoment()
    {
        if (hud != null) hud.SetActive(false);
        if (weaponModel != null) weaponModel.SetActive(false);

        yield return StartCoroutine(characterMoves.DeathCameraRotate(tiltDuration));

        if (gameOverUI != null) gameOverUI.SetActive(true);
    }
}
