using System.Reflection;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Inspector
    // private Item _itemSlot = new Item[15];

    [SerializeField] private int _gold = 0;
    [SerializeField] private int _AmmoHG = 150;
    [SerializeField] private int _AmmoHGMax = 150;
    [SerializeField] private int _AmmoAR = 300;
    [SerializeField] private int _AmmoARMax = 300;
    #endregion
    public void ReloaingAmmo(Weapon.WeaponType weaponType, int requireAmmo, out int returnAmmo)
    {
        returnAmmo = 0;

        switch (weaponType)
        {
            case Weapon.WeaponType.None:
                break;
            case Weapon.WeaponType.HandGun:
                break;
            case Weapon.WeaponType.Rifle:
                if (_AmmoAR == 0)
                {
                    returnAmmo = 0;
                }
                else if (_AmmoAR >= requireAmmo)
                {
                    returnAmmo = requireAmmo;
                    _AmmoAR -= requireAmmo;
                }
                else if (_AmmoAR < requireAmmo)
                {
                    returnAmmo = _AmmoAR;
                    _AmmoAR = 0;
                }
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

    // 레포데처럼 풀로 채우겠다.
    public void GetAmmo()
    {

    }
}
