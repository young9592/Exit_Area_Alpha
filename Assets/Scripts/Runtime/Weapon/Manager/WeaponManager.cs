using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;
using System;

public class WeaponManager : MonoBehaviour
{
    // 무기 변경 이벤트 핸들러
    public event Action<int, Weapon> OnSetWeapon;

    // 무기 발사 및 재장전 핸들러
    public event Action OnFire;
    public event Action OnReload;

    #region Inspector
    [Header("참조")]

    [SerializeField] private Inventory _inventory;
    [SerializeField] private BasicCamera _cameraManager;
    [SerializeField] private BulletPool _bulletPool;
    // 총기의 오브젝트
    [SerializeField] private List<GameObject> _weaponGo;
    // 왼손 총기 핸드가드 매칭 인스펙터
    [SerializeField] private TwoBoneIKConstraint _traker;
    [SerializeField] private List<Transform> _leftHandGo;

    // 총기 머즐플래시
    [SerializeField] private List<ParticleSystem> _muzzleFlashs;

    [Header("Slot")]
    [SerializeField] private Weapon[] _slots = new Weapon[3];

    [Header("Recoil Recovery Offset")]
    [SerializeField] private float _recoilRecoveryMultiply = 5f;
    #endregion

    #region Field
    private float _curRecoil = 0f;
    private float _curSlotRecoilMin = 0f;
    private float _curSlotRecoilMax = 0f;

    private bool _isFire = false;
    private bool _isReloading = false;
    private int _curSlotIdx = 0;
    #endregion

    #region Property
    public Weapon CurrentSlot => _slots[_curSlotIdx];
    public int CurrentSlotID => _slots[_curSlotIdx].ID;
    public int CurrentSlotAmmo => _slots[_curSlotIdx].Ammo;
    public float CurRecoil => _curRecoil;
    #endregion

    private void Awake()
    {
        // 추후 무기 갯수에 따라서 count가 list랑 동일한지 체크해야합니다.
        #region Null Check
        if (_inventory == null || _cameraManager == null || _bulletPool == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            enabled = false;
            return;
        }
        #endregion
    }

    private void Start()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                CPrint.Warn($"Slot[{i}] is Null");
                continue;
            }

            // 무기 스크립트가 해당 기능을 필요로 하는지 체크
            if (_slots[i] is IUseBulletPool usePoolManager)
            {
                usePoolManager.SetBulletObjectPool(_bulletPool);
            }
            if (_slots[i] is M4A1 m4a1)
            {
                m4a1.OnMuzzleFlash += OnMuzzleFlash;
            }

            if (_slots[i] is AK47 ak47)
            {
                ak47.OnMuzzleFlash += OnMuzzleFlash;
            }
        }
    }

    private void Update()
    {
        #region Null Check
        if (_inventory == null || _cameraManager == null || _bulletPool == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            return;
        }
        #endregion

        // 발사했을 때 발사 쿨타임이 완료되었을때
        if (_isFire)
        {
            if (_slots[_curSlotIdx].CompliteFire)
            {
                OnFire?.Invoke();
                _isFire = false;
            }
        }
        // 재장전중일때 완료 되었는지 체크
        if (_isReloading)
        {
            if (_slots[_curSlotIdx].CompliteReload)
            {
                OnReload?.Invoke();
                _isReloading = false;
            }
        }
        // 반동 감소
        if (_curRecoil >= _curSlotRecoilMin)
        {
            _curRecoil -= Time.deltaTime * _recoilRecoveryMultiply;
            _curRecoil = Mathf.Clamp(_curRecoil, _curSlotRecoilMin, _curSlotRecoilMax);
        }
    }

    public bool Fire()
    {
        // 발사중 혹은 자전중일때
        if (_isFire || _isReloading)
        {
            return false;
        }
        // 맨손일 경우
        if (_slots[_curSlotIdx].GetWeaponType == Weapon.WeaponType.None)
        {
            return false;
        }

        _isFire = true;
        _slots[_curSlotIdx].Fire(ref _curRecoil);

        return true;
    }

    // 머즐플래시 이벤트 함수
    private void OnMuzzleFlash(int id, float recoil)
    {
        _muzzleFlashs[id].Play();
        _cameraManager.AddRecoil(recoil);
    }

    public bool Reload()
    {
        if (_isFire || _isReloading)
        {
            return false;
        }

        if (_slots[_curSlotIdx].GetWeaponType == Weapon.WeaponType.None)
        {
            return false;
        }


        _isReloading = true;
        _slots[_curSlotIdx].Reload(_inventory);

        // 인벤토리 남은 탄환 0발
        if (_slots[_curSlotIdx].ReturnAmmo == 0)
        {
            return false;
        }

        return true;
    }
    public bool SelectSlot(int index, out Weapon.HandType type)
    {
        if (_isFire || _isReloading)
        {
            type = Weapon.HandType.None;
            return false;
        }

        var data = _traker.data;
        data.target = _leftHandGo[_slots[index].ID];
        _traker.data = data;

        _weaponGo[_slots[_curSlotIdx].ID].SetActive(false);
        _weaponGo[_slots[index].ID].SetActive(true);

        type = _slots[index].GetHandType;

        _curSlotRecoilMin = _slots[index].RecoilMin;
        _curSlotRecoilMax = _slots[index].RecoilMax;
        _curRecoil = _curSlotRecoilMin;

        OnSetWeapon?.Invoke(index, _slots[index]);
        
        _curSlotIdx = index;

        return true;
    }
}
