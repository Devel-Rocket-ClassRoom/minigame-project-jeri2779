using UnityEngine;

// Firebase 연동 추적용 로거 (테스트 전용).
// 모든 로그에 [FB:카테고리] 접두사 → Unity 콘솔 검색창에 "FB" 입력 시 한 번에 필터.
//
// ── 테스트 끝나면 제거하는 법 ──
//   ① 즉시 끄기   : FirebaseLog.Enabled = false;  (코드 한 줄, 전체 로그 침묵)
//   ② 완전 제거   : 이 파일 삭제 + 각 매니저의 FirebaseLog.* 호출 제거
public static class FirebaseLog
{
    // 테스트 중 true. 배포/검증 완료 시 false 한 줄로 전체 끔.
    public static bool Enabled = true;

    public static void Log(string category, string message)
    {
        if (!Enabled)
            return;
        Debug.Log($"[FB:{category}] {message}");
    }

    public static void Warn(string category, string message)
    {
        if (!Enabled)
            return;
        Debug.LogWarning($"[FB:{category}] {message}");
    }

    // 에러는 실제 실패 추적용이라 Enabled와 무관하게 항상 출력.
    public static void Error(string category, string message)
    {
        Debug.LogError($"[FB:{category}] {message}");
    }
}
