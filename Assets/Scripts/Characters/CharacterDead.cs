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
    [SerializeField] private GameManager gameManager;

    [SerializeField] private float tiltDuration = 1.5f;

    private bool triggered = false;

    private void Awake()
    {
        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        if (characterHealth != null)
            characterHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (characterHealth != null)
            characterHealth.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (triggered)
            return;

        triggered = true;
        StartCoroutine(DeathMoment());
    }

    private IEnumerator DeathMoment()
    {
        gameManager.StopGame();
        weaponModel.SetActive(false);
        crosshair.SetActive(false);
        hud.SetActive(false);

        yield return StartCoroutine(characterMoves.DeathCameraRotate(tiltDuration));

        gameOverUI.Show();
    }
}
