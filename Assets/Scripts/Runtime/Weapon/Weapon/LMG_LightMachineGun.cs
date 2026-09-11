using UnityEngine;

public class LightMachineGun : Weapon
{
    private enum ReloadState
    {
        BoxOpen,
        BeltUnload,
        BeltLoad,
        BoxClose,
        Bolt,
        None
    }

    private ReloadState _reloadState = ReloadState.None;

    private void Awake()
    {
        _id = 9;
        _name = "LightMachineGun";
        _damage = 38f;
        _fireDelay = 0.06f;
        _recoil = 0.8f;
        _recoilMin = 0.5f;
        _recoilMax = 10f;
        _ammo = 100;
        _magazine = 100;
        _returnAmmo = 0;

        _pelletCount = 1;
        _handType = HandType.Rifle;
        _weaponType = WeaponType.LMG;

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Fire/LMG01_Fire_01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Fire/LMG01_Fire_02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Fire/LMG01_Fire_03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Fire/LMG01_Fire_04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Fire/LMG01_Fire_05"));

        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Reload/LMG01_Reload_01"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Reload/LMG01_Reload_02"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Reload/LMG01_Reload_03"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Reload/LMG01_Reload_04"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/LMG/Reload/LMG01_Reload_05"));

        if (_empty == null)
        {
            _empty = Resources.Load<AudioClip>("Sound/Weapon/Fire_Empty");
        }

        _infomation =
        $"공격력 : {_damage}\n" +
        $"연사력 : {_fireDelay}\n" +
        $"장탄수 : {_magazine}발\n" +
        "사용 탄환 : 지원화기탄";

        #region Null Check

        if (_fireClips.Count < 5 || _reloadClips.Count < 5 || _empty == null)
        {
            CPrint.Error($"{_name}.cs Sound Load Fail.");
            return;
        }
        #endregion
    }

    private void Update()
    {

        if (_isFire)
        {
            if (!_completeFire)
            {
                if (_fireDelayTimer.AddTimer())
                {
                    _isFire = false;
                    _completeFire = true;
                }
            }
        }

        if (_isReload)
        {
            if (!_completeReload)
            {
                if (_reloadDelayTimer.AddTimer())
                {
                    switch (_reloadState)
                    {
                        case ReloadState.BoxOpen:
                            BeltUnload();
                            break;
                        case ReloadState.BeltUnload:
                            BeltLoad();
                            break;
                        case ReloadState.BeltLoad:
                            BoxClose();
                            break;
                        case ReloadState.BoxClose:
                            Bolt();
                            break;
                        case ReloadState.Bolt:
                            Done();
                            break;
                    }
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

        if (_ammo == 0)
        {
            _fireDelayTimer.SetTimer(1f);
            CallSoundPlay(_empty);
            return;
        }

        // 탄 소모 및 실제 탄 Object 생성
        _ammo -= 1;
        CallBulletSpawn(_damage, curRecoil);
        CallMuzzleFlash(ID, _recoil);
        curRecoil += _recoil;
        Mathf.Clamp(curRecoil, _recoilMin, _recoilMax);

        // 사운드 출력
        int randomSound = Random.Range(0, _fireClips.Count);
        CallSoundPlay(FireClips[randomSound]);
        _fireDelayTimer.SetTimer(_fireDelay);

        CallSetAmmo(_weaponType, _ammo);
    }

    public override void Reload(Inventory inventory)
    {
        if (_isReload)
        {
            return;
        }

        inventory.ReloadAmmo(_weaponType, _ammo, _magazine, out int returnAmmo);
        _returnAmmo = returnAmmo;

        if (_returnAmmo == 0)
        {
            return;
        }

        BoxOpen();
    }

    private void BoxOpen()
    {
        _isReload = true;
        _completeReload = false;
        _reloadState = ReloadState.BoxOpen;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }

    private void BeltUnload()
    {
        _reloadState = ReloadState.BeltUnload;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }
    private void BeltLoad()
    {
        _reloadState = ReloadState.BeltLoad;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }
    private void BoxClose()
    {
        _reloadState = ReloadState.BoxClose;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }
    private void Bolt()
    {
        _reloadState = ReloadState.Bolt;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }

    private void Done()
    {
        _isReload = false;
        _completeReload = true;
        _reloadState = ReloadState.None;
        _ammo += _returnAmmo;
        CallSetAmmo(_weaponType, _ammo);
    }
}
