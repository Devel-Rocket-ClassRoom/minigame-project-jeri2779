using UnityEngine;

public interface IWeapon
{
    WeaponData Data { get; }
    int CurrentAmmo { get; }
    int ReserveAmmo { get; }
    bool IsReloading { get; }
    Transform Root { get; }
    GameObject GameObject { get; }

    void Init(CharacterStats stats);
    bool Use(FireContext ctx);
    void TryReload();
    void CancelAction();
    void ResetAmmo();
}
