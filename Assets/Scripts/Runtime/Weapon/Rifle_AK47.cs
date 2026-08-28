using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AK47 : Weapon, IPoolUse
{
    private enum ReloadState
    {
        MagazineDrop,
        MagazineInsert,
        Bolt,
        None
    }

    private CTimer _fireDelayTimer = new CTimer();
    private CTimer _reloadDelayTimer = new CTimer();
    private ReloadState _reloadState = ReloadState.None;
    private AudioClip _empty = null;
    private ObjectPool _objectPool = null;


    private void Awake()
    {
        _id = 2;
        _name = "AK47";
        _damage = 7f;
        _fireDelay = 0.1f;
        _recoil = 1.25f;
        _recoilMin = 0.5f;
        _recoilMax = 15f;
        _ammo = 30;
        _magazine = 30;
        _returnAmmo = 0;

        _pelletCount = 1;
        _handType = HandType.TwoHand;
        _weaponType = WeaponType.Rifle;

        _audioSource = GetComponentInParent<AudioSource>();

        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Fire/AR02_Fire01"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Fire/AR02_Fire02"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Fire/AR02_Fire03"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Fire/AR02_Fire04"));
        _fireClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Fire/AR02_Fire05"));

        _reloadDelays.Add(1f);
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Reload/AR02_Reload01"));
        _reloadDelays.Add(1f);                                   
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Reload/AR02_Reload02"));
        _reloadDelays.Add(1.08f);                                
        _reloadClips.Add(Resources.Load<AudioClip>("Sound/Weapon/AK47/Reload/AR02_Reload03"));

        _empty = Resources.Load<AudioClip>("Sound/Weapon/Fire_Empty");

        #region Null Check
        if (_audioSource == null)
        {
            CPrint.Error($"{_name}.cs AudioSource Connect Fail.");
            return;
        }

        if (_fireClips.Count < 5 || _reloadClips.Count < 3 || _empty == null)
        {
            CPrint.Error($"{_name}.cs Sound Load Fail.");
            return;
        }
        #endregion
    }

    private void Update()
    {
        #region Null Check
        if (_audioSource == null)
        {
            CPrint.Error($"{_name}.cs AudioSource Connect Fail.");
            return;
        }
        if(_objectPool == null)
        {
            CPrint.Error($"{_name}.cs ObjectPool Connect Fail.");
            return;

        }
        #endregion

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
    public void SetObjectPool(ObjectPool objectPool)
    {
        _objectPool = objectPool;

        if (_objectPool != null)
        {
            CPrint.KV($"{_name}", "ObjectPool Connect.");
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
        _objectPool.SpawnBullet(_damage, curRecoil);
        curRecoil += _recoil;
        Mathf.Clamp(curRecoil, _recoilMin, _recoilMax);

        // 사운드 출력
        int randomSound = Random.Range(0, _fireClips.Count);
        _audioSource.PlayOneShot(_fireClips[randomSound]);
        _fireDelayTimer.SetTimer(_fireDelay);
    }

    public override void Reload(Inventory inventory)
    {
        if (_isReload)
        {
            return;
        }

        inventory.ReloaingAmmo(_weaponType, _magazine, out int returnAmmo);
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
        _audioSource.clip = _reloadClips[(int)_reloadState];
        _audioSource.Play();
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);

        CPrint.Log("탄창 드롭");
    }

    private void Insert()
    {
        _reloadState = ReloadState.MagazineInsert;
        _audioSource.clip = _reloadClips[(int)_reloadState];
        _audioSource.Play();
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);

        CPrint.Log("탄창 삽입");
    }

    private void Bolt()
    {
        _reloadState = ReloadState.Bolt;
        _audioSource.clip = _reloadClips[(int)_reloadState];
        _audioSource.Play();
        _reloadDelayTimer.SetTimer(_reloadDelays[(int)_reloadState]);

        CPrint.Log("볼트 당기기");
    }

    private void Done()
    {
        _isReload = false;
        _completeReload = true;
        _reloadState = ReloadState.None;
        _ammo = _returnAmmo;

        CPrint.Log($"재장전 완료 [현재 탄약] : {_ammo}");
    }
}
