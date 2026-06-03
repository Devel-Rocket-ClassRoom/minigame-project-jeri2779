// 피해를 받는 대상. 대미지 관련만 — 그 이상(체력 조회 등)은 절대 포함하지 않는다.
public interface IDamageable
{
    void TakeDamage(float damage);
}
