using System.Collections;
using UnityEngine;

public class CharacterDead : MonoBehaviour
{
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private GameOverUI gameOverUI;

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
        gameOverUI.HideGameplayUI();   // 즉시: HUD/크로스헤어/무기모델 off

        yield return StartCoroutine(characterMoves.DeathCameraRotate(tiltDuration));

        gameOverUI.Show();             // 틸트 후: 게임오버 패널
    }
}
