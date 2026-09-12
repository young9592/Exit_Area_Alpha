using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponManager : MonoBehaviour
{
    // UI 무기변경
    public event Action<int, Weapon> OnSetWeapon;
    // 무기 줍거나 버릴 시 갱신 이벤트 핸들러
    public event Action<int> OnSwap;

    // 무기 발사 및 재장전 핸들러
    public event Action OnFire;
    public event Action OnReload;

    #region Inspector
    [Header("참조")]
    [SerializeField] private Player _player;
    [SerializeField] private UI _uiManager;
    [SerializeField] private Inventory _inventoryManager;
    [SerializeField] private BasicCamera _cameraManager;
    [SerializeField] private BulletPool _bulletPool;
    [SerializeField] private AudioSource _audioSource;
    // 손에 들려있는 총기의 오브젝트
    [SerializeField] private List<GameObject> _handWeaponGO;
    // 왼손 총기 핸드가드 매칭 인스펙터
    [SerializeField] private TwoBoneIKConstraint _traker;
    [SerializeField] private List<Transform> _leftHandGo;
    // 총기 머즐플래시
    [SerializeField] private List<ParticleSystem> _muzzleFlashs;

    [Header("Slot")]
    [SerializeField] private GameObject[] _slotGO;
    [SerializeField] private Weapon[] _slots;

    [Header("총기 프리펩[버릴 때 생성]")]
    [SerializeField] private List<GameObject> _weaponPrefabs;

    [Header("Recoil Recovery Offset")]
    [SerializeField] private float _recoilRecoveryMultiply = 5f;
    #endregion

    #region Field
    private float _curRecoil = 0f;
    private float _curSlotRecoilMin = 0f;
    private float _curSlotRecoilMax = 0f;

    private bool _isFire = false;
    private bool _isReloading = false;
    private int _curSlotIdx = 5;
    // 치료나 주사기 사용후 복귀하기 위한 인덱스
    private int _returnSlotIdx = 0;
    #endregion

    #region Property
    public GameObject CurentSlotObject => _slotGO[_curSlotIdx];
    public Weapon CurrentSlot => _slots[_curSlotIdx];
    public bool CurrentCanZoom => _slots[_curSlotIdx].CanZoom;
    public int CurrentSlotID => _slots[_curSlotIdx].ID;
    public int CurrentSlotAmmo => _slots[_curSlotIdx].Ammo;
    public float CurRecoil => _curRecoil;
    public int CurrentIndex => _curSlotIdx;
    public int HasMedikit => _slots[3].ID;
    public int HasInjector => _slots[4].ID;
    #endregion
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // 추후 무기 갯수에 따라서 count가 list랑 동일한지 체크해야합니다.
        #region Null Check
        if (_player == null || _inventoryManager == null || _cameraManager == null || _bulletPool == null || _audioSource == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            enabled = false;
            return;
        }
        #endregion

        _player.OnInteract += Interact;
        _player.OnDrop += Drop;
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

            _slots[i].OnMuzzleFlash += MuzzleFlashPlay;
            _slots[i].OnBulletSpawn += SpawnBullet;
            _slots[i].OnSoundPlay += SoundPlay;
        }
    }
    private void Update()
    {
        #region Null Check
        if (_player == null || _inventoryManager == null || _cameraManager == null || _bulletPool == null || _audioSource == null)
        {
            CPrint.Error("WeaponManager.cs Null find.");
            enabled = false;
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

                // 메디킷 사용 후 복귀 인덱스 반환
                if (_curSlotIdx == 3 || _curSlotIdx == 4)
                {
                    // 이벤트 해제
                    _slots[_curSlotIdx].OnSoundPlay -= SoundPlay;

                    Destroy(_slotGO[_curSlotIdx].GetComponent<Weapon>());
                    _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<None>();

                    _curSlotIdx = _returnSlotIdx;
                    OnSwap?.Invoke(_curSlotIdx);
                }
            }

            // 무결성 체크
            if (!_slots[_curSlotIdx].IsFire)
            {
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
            // 무결성 체크
            if (!_slots[_curSlotIdx].IsReload)
            {
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
        // 발사중 혹은 장전중일때
        if (_isFire || _isReloading)
        {
            return false;
        }
        // 맨손일 경우
        if (_slots[_curSlotIdx].ID == 0)
        {
            return false;
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
        _slots[_curSlotIdx].Reload(_inventoryManager);

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

        if (_curSlotIdx == index)
        {
            type = Weapon.HandType.None;
            return false;
        }

        // 메디킷이나 주사기 사용 시 복귀할 인덱스 저장
        if (index == 3 || index == 4)
        {
            if (_slots[index].ID == 0)
            {
                type = _slots[_curSlotIdx].GetHandType;
                return false;
            }
            if (_curSlotIdx != 3 && _curSlotIdx != 4)
            {
                _returnSlotIdx = _curSlotIdx;
            }
        }

        // 핸드 트래커 위치 변경
        var data = _traker.data;
        data.target = _leftHandGo[_slots[index].ID];
        _traker.data = data;

        // 현재 손에 들려있는 무기 활성화하여 보여주기
        for (int i = 0; i < _handWeaponGO.Count; i++)
        {
            _handWeaponGO[i].SetActive(false);
        }
        _handWeaponGO[_slots[index].ID].SetActive(true);

        type = _slots[index].GetHandType;
        _cameraManager.SetHandState((BasicCamera.HandState)type);

        _curSlotRecoilMin = _slots[index].RecoilMin;
        _curSlotRecoilMax = _slots[index].RecoilMax;
        _curRecoil = _curSlotRecoilMin;

        OnSetWeapon?.Invoke(index, _slots[index]);

        if (_slots[index].ID != 0)
        {
            SoundPlay(Resources.Load<AudioClip>("Sound/Other/WeaponSwap"));
        }

        _curSlotIdx = index;

        return true;
    }
    // 머즐 플래시 발생
    private void MuzzleFlashPlay(int id, float recoil)
    {
        _muzzleFlashs[id].Play();
        _cameraManager.AddRecoil(recoil);
    }
    // 사운드 플레이
    private void SoundPlay(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
    // 총알 오브젝트 풀 생성
    private void SpawnBullet(float damage, float curRecoil)
    {
        _bulletPool.SpawnBullet(damage, curRecoil);
    }
    private void Interact()
    {
        if (_isFire || _isReloading)
        {
            return;
        }

        GameObject interactObject = _uiManager.InteractObject;

        if (interactObject == null)
        {
            return;
        }

        // 줍기인 경우
        if (interactObject.CompareTag(_uiManager.InteractTagName01))
        {
            Weapon interactWeaponScript = _uiManager.InteractWeaponScript;

            if (interactWeaponScript.ID == 6)
            {
                if (_slots[3].ID == 0)
                {
                    Destroy(_slotGO[3].GetComponent<Weapon>());
                    _slots[3] = _slotGO[3].AddComponent<Medikit>();
                    _slots[3].OnSoundPlay += SoundPlay;
                    Destroy(interactObject);
                }
            }
            else if (interactWeaponScript.ID == 7)
            {
                if (_slots[4].ID == 0)
                {
                    Destroy(_slotGO[4].GetComponent<Weapon>());
                    _slots[4] = _slotGO[4].AddComponent<Injector>();
                    _slots[4].OnSoundPlay += SoundPlay;
                    Destroy(interactObject);
                }
            }
            else
            {
                // 메디킷 슬롯이나 주사기 슬롯일땐 줍기 안되도록
                if (_curSlotIdx == 3 || _curSlotIdx == 4)
                {
                    return;
                }

                // 이벤트 해제
                _slots[_curSlotIdx].OnMuzzleFlash -= MuzzleFlashPlay;
                _slots[_curSlotIdx].OnBulletSpawn -= SpawnBullet;
                _slots[_curSlotIdx].OnSoundPlay -= SoundPlay;

                // None
                if (_slots[_curSlotIdx].ID != 0)
                {
                    GameObject dropItem = Instantiate(_weaponPrefabs[_slots[_curSlotIdx].ID - 1], Camera.main.transform.position + Camera.main.transform.forward * 3, Quaternion.Euler(0, 0, 90));
                    Weapon dropWeaponScript = dropItem.GetComponent<Weapon>();
                    Rigidbody rb = dropItem.GetComponent<Rigidbody>();
                    rb.AddForce(Camera.main.transform.forward * 5f, ForceMode.Impulse);
                    dropWeaponScript.Initialize(_slots[_curSlotIdx].Ammo);
                }

                Destroy(_slotGO[_curSlotIdx].GetComponent<Weapon>());

                switch (interactWeaponScript.ID)
                {
                    case 1:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<M4A1>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 2:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<AK47>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 3:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<Glock17>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 4:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<Jackhammer>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 5:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<Scout>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 8:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<DesertEagle>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    case 9:
                        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<LightMachineGun>();
                        _slots[_curSlotIdx].Initialize(interactWeaponScript.Ammo);
                        break;
                    default:
                        CPrint.Log("WeaponManager.cs 새로운 무기 추가된 상태");
                        break;
                }
                // 이벤트 등록
                _slots[_curSlotIdx].OnMuzzleFlash += MuzzleFlashPlay;
                _slots[_curSlotIdx].OnBulletSpawn += SpawnBullet;
                _slots[_curSlotIdx].OnSoundPlay += SoundPlay;

                OnSwap?.Invoke(_curSlotIdx);

                Destroy(interactObject);
            }
        }
        else if (interactObject.CompareTag(_uiManager.InteractTagName02))
        {
            ItemBox weaponCaseScript = interactObject.GetComponent<ItemBox>();
            weaponCaseScript.Open();

            int layerMask = LayerMask.NameToLayer("OnlyPlayerBlock");
            interactObject.layer = layerMask;
            SoundPlay(Resources.Load<AudioClip>("Sound/Other/ItemBoxOpen"));
        }
        else if (interactObject.CompareTag(_uiManager.InteractTagName03))
        {
            AmmoBox ammoBoxScript = interactObject.GetComponent<AmmoBox>();
            ammoBoxScript.Open();

            _inventoryManager.GetAmmoBox(_slots[0].GetWeaponType, _slots[1].GetWeaponType, _slots[2].GetWeaponType);
            OnSwap?.Invoke(_curSlotIdx);

            int layerMask = LayerMask.NameToLayer("OnlyPlayerBlock");
            interactObject.layer = layerMask;
            SoundPlay(Resources.Load<AudioClip>("Sound/Other/AmmoBoxOpen"));
        }
        else if (interactObject.CompareTag(_uiManager.InteractTagName04))
        {
            SoundPlay(Resources.Load<AudioClip>("Sound/Other/BarricadeBreak"));
            interactObject.SetActive(false);
        }
        else if (interactObject.CompareTag(_uiManager.InteractTagName05))
        {
            CPrint.Log("엔딩씬 전환. 탈출에 성공하였습니다.");
        }
    }
    private void Drop()
    {
        if (_isFire || _isReloading)
        {
            return;
        }

        // 메디킷이나 주사기는 버리지 못하게
        if (_curSlotIdx == 3 || _curSlotIdx == 4)
        {
            return;
        }

        // 이벤트 해제
        _slots[_curSlotIdx].OnMuzzleFlash -= MuzzleFlashPlay;
        _slots[_curSlotIdx].OnBulletSpawn -= SpawnBullet;
        _slots[_curSlotIdx].OnSoundPlay -= SoundPlay;

        // None
        if (_slots[_curSlotIdx].ID != 0)
        {
            GameObject dropItem = Instantiate(_weaponPrefabs[_slots[_curSlotIdx].ID - 1], Camera.main.transform.position + Camera.main.transform.forward * 3, Quaternion.Euler(0, 0, 90));
            Weapon dropWeaponScript = dropItem.GetComponent<Weapon>();
            Rigidbody rb = dropItem.GetComponent<Rigidbody>();
            rb.AddForce(Camera.main.transform.forward * 5f, ForceMode.Impulse);
            dropWeaponScript.Initialize(_slots[_curSlotIdx].Ammo);
        }

        Destroy(_slotGO[_curSlotIdx].GetComponent<Weapon>());

        _slots[_curSlotIdx] = _slotGO[_curSlotIdx].AddComponent<None>();

        OnSwap?.Invoke(_curSlotIdx);

    }
}
