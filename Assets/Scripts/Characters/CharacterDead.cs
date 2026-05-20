using System.Collections;
using UnityEngine;

public class CharacterDead : MonoBehaviour
{
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private EnemySpawner enemySpawner;

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
        if (enemySpawner != null) enemySpawner.StopGame();
        if (hud != null) hud.SetActive(false);
        if (weaponModel != null) weaponModel.SetActive(false);
        if (crosshair != null) crosshair.SetActive(false);

        yield return StartCoroutine(characterMoves.DeathCameraRotate(tiltDuration));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameOverUI != null) gameOverUI.Show();
    }
}
