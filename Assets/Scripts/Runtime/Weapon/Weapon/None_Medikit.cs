using UnityEngine;

public class Medikit : Weapon
{
    private void Awake()
    {
        _id = 6;
        _handType = HandType.None;
        _weaponType = WeaponType.None;
        _name = "붕대";
        _ammo = 1;
        _magazine = 1;
        _fireDelay = 4.2f;

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Other/Medikit"));
        _infomation = "사용자의 체력을 회복시켜줍니다.";
    }
    private void Update()
    {
        if (_isFire)
        {
            if (_fireDelayTimer.GetCurrentTimerState)
            {
                if (_fireDelayTimer.AddTimer())
                {
                    _isFire = false;
                    _completeFire = true;
                }
            }
        }
    }

    public override void Fire(ref float curRecoil)
    {
        if (_isFire)
        {
            return;
        }

        _isFire = true;
        _completeFire = false;
        _fireDelayTimer.SetTimer(_fireDelay);
        CallSoundPlay(FireClips[0]);
        return;
    }

    public override void Reload(Inventory inventory)
    {
        // 소모품입니다.
        return;
    }
}
