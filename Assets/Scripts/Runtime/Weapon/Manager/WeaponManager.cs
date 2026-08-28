using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

public class WeaponManager : MonoBehaviour
{

    #region Inspector
    [Header("참조")]
    [SerializeField] private Player _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private BasicCamera _cameraManager;
    [SerializeField] private ObjectPool _objectPool;
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

    // 플레이어 반동 시각적 효과
    #region Property
    public float GetCurentFireArmRecoil => _slots[_curSlotIdx].Recoil;
    #endregion

    private void Awake()
    {
        // 추후 무기 갯수에 따라서 count가 list랑 동일한지 체크해야합니다.
        #region Null Check
        if (_inventory == null || _player == null || _cameraManager == null || _objectPool == null)
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

            if (_slots[i] is IPoolUse poolUse)
            {
                poolUse.SetObjectPool(_objectPool);
            }
        }
    }

    private void Update()
    {
        #region Null Check
        if (_inventory == null || _player == null || _cameraManager == null || _objectPool == null)
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
                _player.SuccessFireDelay();
                _isFire = false;
            }
        }
        // 재장전중일때 완료 되었는지 체크
        if (_isReloading)
        {
            if (_slots[_curSlotIdx].CompliteReload)
            {
                _player.SuccessReload();
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


        if (_muzzleFlashs[_slots[_curSlotIdx].ID] != null && _slots[_curSlotIdx].Ammo != 0)
        {
            _muzzleFlashs[_slots[_curSlotIdx].ID].Play();
            CPrint.KV("총 발사", $"{_slots[_curSlotIdx].Ammo - 1}");
            _cameraManager.AddRecoil(_slots[_curSlotIdx].Recoil);
        }

        _isFire = true;
        _slots[_curSlotIdx].Fire(ref _curRecoil);

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

        _curSlotRecoilMin = _slots[index].RecoilMin;
        _curSlotRecoilMax = _slots[index].RecoilMax;
        _curRecoil = _curSlotRecoilMin;

        _curSlotIdx = index;
        return true;
    }

    private void OnGUI()
    {
        // 크로스헤어
        float crossHairWidth = 50f;
        float crossHairHeight = 5f;
        float subPush = 25;
        float recoilOffset = 10f;

        Texture2D greenTexture;
        Color color = new Color(0, 1f, 0, 0.2f);
        greenTexture = new Texture2D(1, 1);
        greenTexture.SetPixel(0, 0, color);
        greenTexture.Apply();

        GUIStyle box = new GUIStyle(GUI.skin.box);
        box.normal.background = greenTexture;

        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height - crossHairHeight) * 0.5f, crossHairHeight, crossHairHeight), "", box);

        GUI.Box(new Rect((Screen.width * 0.5f - crossHairWidth - subPush) - _curRecoil * recoilOffset, (Screen.height - crossHairHeight) * 0.5f, crossHairWidth, crossHairHeight), "", box);
        GUI.Box(new Rect((Screen.width * 0.5f + subPush) + _curRecoil * recoilOffset, (Screen.height - crossHairHeight) * 0.5f, crossHairWidth, crossHairHeight), "", box);

        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height * 0.5f - crossHairWidth - subPush) - _curRecoil * recoilOffset, crossHairHeight, crossHairWidth), "", box);
        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height * 0.5f + subPush) + _curRecoil * recoilOffset, crossHairHeight, crossHairWidth), "", box);
    }
}
