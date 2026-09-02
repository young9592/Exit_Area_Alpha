using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Inspector
    // private Item _itemSlot = new Item[15];

    [SerializeField] private int _noneType = 0;
    [SerializeField] private int _arAmmo = 300;
    [SerializeField] private int _arAmmoMax = 300;
    [SerializeField] private int _hgAmmo = 160;
    [SerializeField] private int _hgAmmoMax = 160;
    #endregion

    #region Property
    public int NoneType => _noneType;
    public int AmmoAR => _arAmmo;
    public int AmmoHG => _hgAmmo;
    #endregion

    // 부족한 총알만큼 받아오기
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
        else if (_arAmmo >= requireAmmo)
        {
            returnAmmo = requireAmmo;
            _arAmmo -= requireAmmo;
        }
        else if (_arAmmo < requireAmmo)
        {
            returnAmmo = _arAmmo;
            _arAmmo = 0;
        }
    }

    // 레포데처럼 풀로 채우겠다.
    public void GetAmmo()
    {
        _arAmmo = _arAmmoMax;
        _hgAmmo = _hgAmmoMax;
    }
}
