using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Inspector
    // Hand
    [SerializeField] private int _noneType = 0;
    // ¼ÒÃÑ
    [SerializeField] private int _arAmmo = 240;
    [SerializeField] private int _arAmmoMax = 240;
    // ±ÇÃÑ
    [SerializeField] private int _hgAmmo = 160;
    [SerializeField] private int _hgAmmoMax = 160;
    // ¼¦°Ç
    [SerializeField] private int _sgAmmo = 60;
    [SerializeField] private int _sgAmmoMax = 60;
    #endregion

    #region Property
    public int NoneType => _noneType;
    public int AmmoAR => _arAmmo;
    public int AmmoHG => _hgAmmo;
    public int AmmoSG => _sgAmmo;
    #endregion

    // ºÎÁ·ÇÑ ÃÑ¾Ë¸¸Å­ ¹Þ¾Æ¿À±â
    public void ReloadAmmo(Weapon.WeaponType weaponType, int curAmmo, int magazine, out int returnAmmo)
    {
        returnAmmo = 0;

        int requireAmmo = magazine - curAmmo;

        switch (weaponType)
        {
            case Weapon.WeaponType.None:
                break;
            case Weapon.WeaponType.HandGun:
                GetTypeAmmo(ref _hgAmmo, requireAmmo, out returnAmmo);
                break;
            case Weapon.WeaponType.Rifle:
                GetTypeAmmo(ref _arAmmo, requireAmmo, out returnAmmo);
                break;
            case Weapon.WeaponType.Shotgun:
                GetTypeAmmo(ref _sgAmmo, requireAmmo, out returnAmmo);
                break;
            case Weapon.WeaponType.Melee:
                break;
            case Weapon.WeaponType.Throw:
                break;
            default:
                break;

        }
    }

    private void GetTypeAmmo(ref int ammo, int requireAmmo, out int returnAmmo)
    {
        returnAmmo = 0;

        if (ammo == 0)
        {
            returnAmmo = 0;
        }
        else if (ammo >= requireAmmo)
        {
            returnAmmo = requireAmmo;
            ammo -= requireAmmo;
        }
        else if (ammo < requireAmmo)
        {
            returnAmmo = ammo;
            ammo = 0;
        }
    }

    // Ammo Recharge
    public void GetAmmoBox()
    {
        _arAmmo = _arAmmoMax;
        _hgAmmo = _hgAmmoMax;
        _sgAmmo = _sgAmmoMax;
    }
}
