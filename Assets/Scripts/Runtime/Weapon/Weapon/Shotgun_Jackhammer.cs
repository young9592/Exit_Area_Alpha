using UnityEngine;

public class Jackhammer : Weapon
{
    private enum ReloadState
    {
        MagazineDrop,
        MagazineInsert,
        Bolt,
        None
    }

    private ReloadState _reloadState = ReloadState.None;

    private void Awake()
    {
        _id = 4;
        _name = "JackHammer";
        _damage = 10f;
        _fireDelay = 0.5f;
        _recoil = 5f;
        _recoilMin = 1f;
        _recoilMax = 15f;
        _ammo = 10;
        _magazine = 10;
        _returnAmmo = 0;

        _pelletCount = 8;
        _handType = HandType.Rifle;
        _weaponType = WeaponType.Shotgun;

        _audioSource = GetComponentInParent<AudioSource>();

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Fire/SG01_Fire_01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Fire/SG01_Fire_02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Fire/SG01_Fire_03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Fire/SG01_Fire_04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Fire/SG01_Fire_05"));

        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Reload/SG01_Reload_01"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Reload/SG01_Reload_02"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Jackhammer/Reload/SG01_Reload_03"));

        _empty = Resources.Load<AudioClip>("Sound/Weapon/Fire_Empty");

        #region Null Check

        if (_fireClips.Count < 5 || _reloadClips.Count < 3 || _empty == null)
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
                        case ReloadState.MagazineDrop:
                            Insert();
                            break;
                        case ReloadState.MagazineInsert:
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

        for (int i = 0; i < _pelletCount; i++)
        {
            CallBulletSpawn(_damage, curRecoil + Random.Range(0f, (i / 4) * 0.2f));
        }

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

        Drop();
    }

    private void Drop()
    {
        _isReload = true;
        _completeReload = false;
        _reloadState = ReloadState.MagazineDrop;
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);
        CallSoundPlay(ReloadClips[(int)_reloadState]);
    }

    private void Insert()
    {
        _reloadState = ReloadState.MagazineInsert;
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
