using UnityEngine;

public class None : Weapon
{
    private void Awake()
    {
        _id = 0;
        _handType = HandType.None;
        _weaponType = WeaponType.None;
        _name = "맨손";
    }

    public override void Fire(ref float curRecoil)
    {
        // 맨손입니다.
        return;
    }

    public override void Reload(Inventory inventory)
    {
        // 맨손입니다.
        return;
    }
}
