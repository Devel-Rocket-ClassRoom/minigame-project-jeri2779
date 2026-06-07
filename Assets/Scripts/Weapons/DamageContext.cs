// 한 번의 타격이 일어난 "상황". 무기가 채워서 PlayerDamageCalculator에 넘긴다.
// 수치 보너스가 아니라, 맞는 순간의 조건(헤드샷/근접/무기배율 등)을 담는다.
public struct DamageContext
{
    public float baseDamage; // 무기 기본 피해 (data.damage)
    public bool isHeadshot;
    public bool isMelee;
    public float weaponMultiplier; // 멜리 스윙/대체공격 배율 (원거리는 1)
    public float targetHealthRatio; // 맞는 대상의 현재 HP 비율 (0~1). 상태 기반 모디파이어용
    public WeaponType weaponType; // 발사 무기의 종류 (SR 헤드샷 증폭 등 종류별 계산용). 근접/기본은 None
}
