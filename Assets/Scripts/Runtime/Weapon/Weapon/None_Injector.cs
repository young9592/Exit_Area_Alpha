using UnityEngine;

public class Injector : Weapon
{
    private void Awake()
    {
        _id = 7;
        _handType = HandType.None;
        _weaponType = WeaponType.None;
        _name = "약물 주사기";
        _ammo = 1;
        _magazine = 1;
        _fireDelay = 2.1f;

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Other/Injector"));

        _infomation =
            "30초간 다음의 효과를 받습니다.\n" +
            "체력이 재생됩니다.\n" +
            "이동속도가 상승합니다.";
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
