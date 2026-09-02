using UnityEngine;

public class M4A1 : Weapon
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
        _id = 1;
        _name = "M4A1";
        _damage = 5f;
        _fireDelay = 0.075f;
        _recoil = 0.75f;
        _recoilMin = 0f;
        _recoilMax = 15f;
        _ammo = 30;
        _magazine = 30;
        _returnAmmo = 0;

        _pelletCount = 1;
        _handType = HandType.TwoHand;
        _weaponType = WeaponType.Rifle;

        _audioSource = GetComponentInParent<AudioSource>();

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Fire/AR01_Fire01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Fire/AR01_Fire02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Fire/AR01_Fire03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Fire/AR01_Fire04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Fire/AR01_Fire05"));

        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Reload/AR01_Reload01"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Reload/AR01_Reload02"));
        _reloadDelays.Add(1.08f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/M4A1/Reload/AR01_Reload03"));

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
            _audioSource.clip = _empty;
            _audioSource.Play();
            return;
        }

        // 탄 소모 및 실제 탄 Object 생성
        _ammo -= 1;
        CallBulletSpawn(_damage, curRecoil);
        CallMuzzleFlash(ID, _recoil);
        curRecoil += _recoil;
        Mathf.Clamp(curRecoil, _recoilMin, _recoilMax);

        // 사운드 출력
        int randomSound = UnityEngine.Random.Range(0, _fireClips.Count);
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

        // 인벤토리 탄약 없음
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
