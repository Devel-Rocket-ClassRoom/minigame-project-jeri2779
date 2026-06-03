// 체력 비율 조회 전용 인터페이스. 피해 인터페이스(IDamageable)와 분리한다.
// 상태 기반 피해 계산(예: 건강한 적 추가뎀)이 타겟의 체력비를 읽을 때만 사용.
public interface IHealthInfo
{
    // 현재 체력 비율 (0~1)
    float HealthRatio { get; }
}
