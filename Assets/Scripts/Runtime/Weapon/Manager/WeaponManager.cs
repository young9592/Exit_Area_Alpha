using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine.Animations.Rigging;

public class WeaponManager : MonoBehaviour
{

    #region Inspector
    [Header("참조")]
    [SerializeField] private Player _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private BasicCamera _cameraManager;
    // 총기의 오브젝트
    [SerializeField] private List<GameObject> _weaponGo;
    // 왼손 총기 핸드가드 매칭 인스펙터
    [SerializeField] private TwoBoneIKConstraint _traker;
    [SerializeField] private List<Transform> _leftHandGo;

    // 총기 머즐플래시
    [SerializeField] private List<ParticleSystem> _muzzleFlashs;

    [Header("Slot")]
    [SerializeField] private Weapon[] _slots = new Weapon[3];
    #endregion

    #region Field
    private bool _isFire = false;
    private bool _isReloading = false;
    private int _curSlotIdx = 0;
    #endregion

    // 플레이어 반동 시각적 효과
    #region Property
    public float GetCurentFireArmRecoil => _slots[_curSlotIdx].Recoil;
    #endregion

    private void Awake()
    {
        // 추후 무기 갯수에 따라서 count가 list랑 동일한지 체크해야합니다.
        #region Null Check
        if (_inventory == null || _player == null || _cameraManager == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            enabled = false;
            return;
        }
        #endregion

    }

    private void Update()
    {
        #region Null Check
        if (_inventory == null || _player == null || _cameraManager == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            return;
        }
        #endregion

        if (_isFire)
        {
            if (_slots[_curSlotIdx].CompliteFire)
            {
                _player.SuccessFireDelay();
                _isFire = false;
            }
        }

        if (_isReloading)
        {
            if (_slots[_curSlotIdx].CompliteReload)
            {
                _player.SuccessReload();
                _isReloading = false;
            }
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
        if(_slots[_curSlotIdx].GetWeaponType == Weapon.WeaponType.None)
        {
            return false;
        }


        if (_muzzleFlashs[_slots[_curSlotIdx].ID] != null && _slots[_curSlotIdx].Ammo != 0)
        {
            _muzzleFlashs[_slots[_curSlotIdx].ID].Play();
            CPrint.KV("총 발사", $"{_slots[_curSlotIdx].Ammo}");
            _cameraManager.AddRecoil(_slots[_curSlotIdx].Recoil);
        }

        _isFire = true;
        _slots[_curSlotIdx].Fire();

        return true;
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

        _curSlotIdx = index;
        return true;
    }
}
