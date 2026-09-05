using UnityEngine;

public class Scout : Weapon
{
    private enum FireState
    {
        Fire,
        Reload,
        None
    }
    private enum ReloadState
    {
        MagazineDrop,
        MagazineInsert,
        Bolt,
        None
    }

    private ReloadState _reloadState = ReloadState.None;
    private FireState _fireState = FireState.Fire;

    private void Awake()
    {
        _id = 5;
        _name = "Scout";
        _damage = 70f;
        _fireDelay = 1f;
        _recoil = 10f;
        _recoilMin = 0f;
        _recoilMax = 15f;
        _ammo = 5;
        _magazine = 5;
        _returnAmmo = 0;

        _pelletCount = 1;
        _handType = HandType.Rifle;
        _weaponType = WeaponType.Rifle;

        _audioSource = GetComponentInParent<AudioSource>();

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Fire/SR01_Fire_01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Fire/SR01_Fire_02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Fire/SR01_Fire_03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Fire/SR01_Fire_04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Fire/SR01_Fire_05"));

        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Reload/SR01_Reload_01"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Reload/SR01_Reload_02"));
        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/Scout/Reload/SR01_Reload_03"));

        _empty = Resources.Load<AudioClip>("Sound/Weapon/Fire_Empty");

        _canZoom = true;
        
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
                    switch (_fireState)
                    {
                        case FireState.Fire:
                            FireReload();
                            break;
                        case FireState.Reload:
                            FireDone();
                            break;
                    }
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
            _fireState = FireState.Reload;
            return;
        }

        FireBullet(ref curRecoil);
    }

    private void FireBullet(ref float curRecoil)
    {
        _ammo -= 1;
        CallBulletSpawn(_damage, curRecoil);
        CallMuzzleFlash(ID, _recoil);
        curRecoil += _recoil;
        Mathf.Clamp(curRecoil, _recoilMin, _recoilMax);

        int randomSound = Random.Range(0, _fireClips.Count);
        CallSoundPlay(FireClips[randomSound]);
        _fireDelayTimer.SetTimer(_fireDelay);

        CallSetAmmo(_weaponType, _ammo);

        _fireState = FireState.Fire;
    }
    private void FireReload()
    {
        CallSoundPlay(ReloadClips[(int)ReloadState.Bolt]);
        _fireDelayTimer.SetTimer(_reloadDelays[(int)ReloadState.Bolt]);

        _fireState = FireState.Reload;
    }
    private void FireDone()
    {
        _isFire = false;
        _completeFire = true;

        _fireState = FireState.None;
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
