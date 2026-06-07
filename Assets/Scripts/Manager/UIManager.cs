using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private bool lockCursorOnGameplay = true;
    [SerializeField] private bool hideCursorOnGameplay = true;

    private readonly HashSet<UnityEngine.Object> overlayOwners = new();

    // ESC 라우팅: 오버레이 스택(상단 우선) → 없으면 폴백(PausePanel 등)
    private readonly Stack<Action> escapeStack = new();
    private Action fallbackEscape;
    private InputAction escapeAction;

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

        escapeAction = new InputAction(binding: "<Keyboard>/escape");
        escapeAction.performed += OnEscapePerformed;
        escapeAction.Enable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (escapeAction != null)
        {
            escapeAction.performed -= OnEscapePerformed;
            escapeAction.Disable();
            escapeAction.Dispose();
        }
    }

    private void OnEscapePerformed(InputAction.CallbackContext _)
    {
        if (escapeStack.TryPeek(out var handler))
            handler();
        else
            fallbackEscape?.Invoke();
    }

    // 오버레이가 열릴 때 push, 닫힐 때 pop
    public void PushEscape(Action handler) => escapeStack.Push(handler);
    public void PopEscape() { if (escapeStack.Count > 0) escapeStack.Pop(); }

    // 항상 활성인 토글용 (PausePanel). 오버레이가 없을 때만 호출됨.
    public void SetFallbackEscape(Action handler) => fallbackEscape = handler;

    public void ShowOverlay(GameObject panel, UnityEngine.Object owner = null)
    {
        if (panel != null)
            panel.SetActive(true);

        RegisterOverlayOpened(owner != null ? owner : panel);
    }

    public void HideOverlay(GameObject panel, UnityEngine.Object owner = null)
    {
        if (panel != null)
            panel.SetActive(false);

        RegisterOverlayClosed(owner != null ? owner : panel);
    }

    public void RegisterOverlayOpened(UnityEngine.Object owner)
    {
        if (owner == null)
            return;

        overlayOwners.Add(owner);
        ApplyCursorState();
    }

    public void RegisterOverlayClosed(UnityEngine.Object owner)
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
