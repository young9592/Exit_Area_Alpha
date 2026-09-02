using UnityEngine;

public class Glock17 : Weapon
{
    private enum ReloadState
    {
        MagazineDrop,
        MagazineInsert,
        Release,
        None
    }

    private ReloadState _reloadState = ReloadState.None;

    private void Awake()
    {
        _id = 3;
        _name = "Glock17";
        _damage = 10f;
        _fireDelay = 0.17f;
        _recoil = 1.2f;
        _recoilMin = 1.7f;
        _recoilMax = 15f;
        _ammo = 20;
        _magazine = 20;
        _returnAmmo = 0;

        _pelletCount = 1;
        _handType = HandType.OneHand;
        _weaponType = WeaponType.HandGun;

        _audioSource = GetComponentInParent<AudioSource>();

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Fire/HG_Fire_01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Fire/HG_Fire_02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Fire/HG_Fire_03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Fire/HG_Fire_04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Fire/HG_Fire_05"));

        _reloadDelays.Add(0.4f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Reload/HG_Reload_01"));
        _reloadDelays.Add(0.4f);                                                  
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Reload/HG_Reload_02"));
        _reloadDelays.Add(0.5f);                                                  
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Glock17/Reload/HG_Reload_03"));

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
                            Release();
                            break;
                        case ReloadState.Release:
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

        _ammo -= 1;
        CallBulletSpawn(_damage, curRecoil);
        CallMuzzleFlash(ID, _recoil);
        curRecoil += _recoil;
        Mathf.Clamp(curRecoil, _recoilMin, _recoilMax);

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

    private void Release()
    {
        _reloadState = ReloadState.Release;
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
