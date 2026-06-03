using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private bool lockCursorOnGameplay = true;
    [SerializeField] private bool hideCursorOnGameplay = true;

    private readonly HashSet<Object> overlayOwners = new();

    public bool HasOpenOverlay => overlayOwners.Count > 0;

    public static UIManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var managerObject = new GameObject(nameof(UIManager));
        return managerObject.AddComponent<UIManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplyCursorState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowOverlay(GameObject panel, Object owner = null)
    {
        if (panel != null)
            panel.SetActive(true);

        RegisterOverlayOpened(owner != null ? owner : panel);
    }

    public void HideOverlay(GameObject panel, Object owner = null)
    {
        if (panel != null)
            panel.SetActive(false);

        RegisterOverlayClosed(owner != null ? owner : panel);
    }

    public void RegisterOverlayOpened(Object owner)
    {
        if (owner == null)
            return;

        overlayOwners.Add(owner);
        ApplyCursorState();
    }

    public void RegisterOverlayClosed(Object owner)
    {
        if (owner == null)
            return;

        overlayOwners.Remove(owner);
        ApplyCursorState();
    }

    public void ClearOverlays()
    {
        overlayOwners.Clear();
        ApplyCursorState();
    }

    public void ApplyCursorState()
    {
        if (HasOpenOverlay)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.visible = !hideCursorOnGameplay;
        Cursor.lockState = lockCursorOnGameplay ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
