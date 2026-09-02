using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("필수 연결 목록")]
    [SerializeField] private Player _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private BasicCamera _cameraManager;

    [Header("UI Interact View")]
    [SerializeField] private GameObject _interactInfo;
    [SerializeField] private GameObject _interactLine;
    [SerializeField] private Image _lineImage;
    [SerializeField] private Image _targetInfoBackgroundImage;
    [SerializeField] private TextMeshProUGUI _targetNameText;
    [SerializeField] private TextMeshProUGUI _targetInfoText;
    [SerializeField] private string _interactTagName01 = "Weapon";

    [Header("UI Weapon Slot")]
    [SerializeField] private Image _equipWeaponImage;
    [SerializeField] private Image _equipWeaponBulletImage;
    [SerializeField] private Image _slot01Image;
    [SerializeField] private Image _slot02Image;
    [SerializeField] private Image _slot03Image;
    [SerializeField] private TextMeshProUGUI _curWeaponAmmoText;
    [SerializeField] private TextMeshProUGUI _curWeaponInventoryAmmoText;

    [Header("UI Health / Stemina")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private TextMeshProUGUI _curHealthText;
    [SerializeField] private TextMeshProUGUI _healthMaxText;

    [SerializeField] private Image _steminaBar;
    [SerializeField] private TextMeshProUGUI _curSteminaText;
    [SerializeField] private TextMeshProUGUI _steminaMaxText;

    [Header("Sprite")]
    [SerializeField] private Sprite _noneSp;
    [SerializeField] private Sprite _weaponSp01;
    [SerializeField] private Sprite _weaponSp02;
    [SerializeField] private Sprite _weaponSp03;

    [SerializeField] private Sprite _arBulletSp;
    [SerializeField] private Sprite _hgBulletSp;

    #region Field
    private string _targetName;
    private StringBuilder _info = new StringBuilder(100);
    // 둘이 세트
    private GameObject _curInteractWeapon;
    private Weapon _curWeaponScript;

    // Player Status
    private float _health;
    private float _healthMax;
    private float _stemina;
    private float _steminaMax;

    // CrossHair
    private float _crossHairWidth = 30f;
    private float _crossHairHeight = 4f;
    private float _subPush = 8f;
    private float _recoilOffset = 10f;
    float _curRecoil = 0f;
    private Texture2D _greenTexture;
    private Color _color = new Color(0, 1f, 0, 0.2f);
    private GUIStyle _box;

    private Rect _centerHair = new Rect();
    private Rect _leftHair = new Rect();
    private Rect _rightHair = new Rect();
    private Rect _topHair = new Rect();
    private Rect _buttomHair = new Rect();
    #endregion

    #region Property
    public GameObject InteractWeapon => _curInteractWeapon;
    public Weapon InteractWeaponScript => _curWeaponScript;
    #endregion

    private void Awake()
    {
        Cursor.visible = false; // 마우스 커서 없애기
        Cursor.lockState = CursorLockMode.Locked; // 마우스 잠그기

        // 플레이어 상태 초기화
        _health = _player.Health;
        _healthMax = _player.HealthMax;
        _stemina = _player.Stemina;
        _steminaMax = _player.SteminaMax;

        // 크로스 헤어 세팅
        _greenTexture = new Texture2D(1, 1);
        _greenTexture.SetPixel(0, 0, _color);
        _greenTexture.Apply();

        // 이벤트 등록
        _cameraManager.OnInteract += GetInteractInfo;
        _weaponManager.OnSetWeapon += SetWeapon;
        _weaponManager.CurrentSlot.OnSetAmmo += SetAmmo;
        _player.OnSetHealth += SetHealth;
        _player.OnSetStemina += SetStemina;
    }
    public void SetAmmo(Weapon.WeaponType weaponType, int ammo)
    {

        // 추후 무기 변경에 따른 총알변경 필요 ID로 구분할 예정..
        int inventoryAmmo = 0;

        switch (weaponType)
        {
            case Weapon.WeaponType.None:
                inventoryAmmo = _inventory.NoneType;
                break;
            case Weapon.WeaponType.HandGun:
                inventoryAmmo = _inventory.AmmoHG;
                break;
            case Weapon.WeaponType.Rifle:
                inventoryAmmo = _inventory.AmmoAR;
                break;
            case Weapon.WeaponType.Shotgun:
                break;
            case Weapon.WeaponType.Melee:
                break;
            case Weapon.WeaponType.Throw:
                break;
            default:
                break;
        }

        _curWeaponAmmoText.text = ammo.ToString();
        _curWeaponInventoryAmmoText.text = inventoryAmmo.ToString();

    }
    public void GetInteractInfo(GameObject hitObject)
    {
        if(hitObject != _curInteractWeapon)
        {
            _interactInfo.SetActive(false);
            _interactLine.SetActive(false);
            _info.Clear();
        }

        if (hitObject == null)
        {
            _interactInfo.SetActive(false);
            _interactLine.SetActive(false);
            _curInteractWeapon = null;
            _curWeaponScript = null;
            _info.Clear();
        }
        else
        {
            if (hitObject.tag == _interactTagName01)
            {
                if (_curInteractWeapon == null)
                {
                    _curInteractWeapon = hitObject;
                    _curWeaponScript = hitObject.GetComponent<Weapon>();
                    _interactLine.SetActive(true);
                    _lineImage.fillAmount = 0;
                    _targetInfoBackgroundImage.fillAmount = 0;
                }
                // 계속 쳐다보면 라인이 늘어나면서 완료되면 info가 보이도록
                else
                {
                    _lineImage.fillAmount = Mathf.Lerp(_lineImage.fillAmount, 1, 1f - Mathf.Exp(-10 * Time.deltaTime));

                    if(_lineImage.fillAmount >= 0.95 && _lineImage.fillAmount <= 1)
                    {
                        _lineImage.fillAmount = 1;

                        _targetInfoBackgroundImage.fillAmount = Mathf.Lerp(_targetInfoBackgroundImage.fillAmount, 1, 1f - Mathf.Exp(-10 * Time.deltaTime));

                        if (_targetInfoBackgroundImage.fillAmount >= 0.95 && _targetInfoBackgroundImage.fillAmount < 1)
                        {
                            _targetInfoBackgroundImage.fillAmount = 1;


                            _targetName = _curWeaponScript.Name;
                            _info.Append($"공격력 : {_curWeaponScript.Damage}\n");
                            _info.Append($"연사력 : {_curWeaponScript.FireDelay}\n");
                            _info.Append($"장탄수 : {_curWeaponScript.Magazine}\n");

                            _targetNameText.text = _targetName;
                            _targetInfoText.text = _info.ToString();
                            _interactInfo.SetActive(true);
                        }
                    }
                }

            }
            else
            {

            }
        }
    }
    public void SetWeapon(int index, Weapon weapon)
    {
        _weaponManager.CurrentSlot.OnSetAmmo -= SetAmmo;

        int id = weapon.ID;
        Sprite selectWeaponSprite = _noneSp;
        Sprite selectBulletSprite = _noneSp;

        switch (id)
        {
            case 0:
                selectWeaponSprite = _noneSp;
                selectBulletSprite = _noneSp;
                break;

            case 1:
                selectWeaponSprite = _weaponSp01;
                selectBulletSprite = _arBulletSp;
                break;

            case 2:
                selectWeaponSprite = _weaponSp02;
                selectBulletSprite = _arBulletSp;
                break;
            case 3:
                selectWeaponSprite = _weaponSp03;
                selectBulletSprite = _hgBulletSp;
                break;
        }

        _equipWeaponImage.sprite = selectWeaponSprite;
        _equipWeaponBulletImage.sprite = selectBulletSprite;


        switch (index)
        {
            case 0:
                _slot01Image.gameObject.SetActive(true);
                _slot02Image.gameObject.SetActive(false);
                _slot03Image.gameObject.SetActive(false);
                break;

            case 1:
                _slot01Image.gameObject.SetActive(false);
                _slot02Image.gameObject.SetActive(true);
                _slot03Image.gameObject.SetActive(false);
                break;

            case 2:
                _slot01Image.gameObject.SetActive(false);
                _slot02Image.gameObject.SetActive(false);
                _slot03Image.gameObject.SetActive(true);
                break;
        }

        weapon.OnSetAmmo += SetAmmo;
        SetAmmo(weapon.GetWeaponType, weapon.Ammo);
    }
    public void SetHealth(float health, float healthMax)
    {
        _health = health;
        _healthMax = healthMax;

        _curHealthText.text = _health.ToString("N0");
        _healthMaxText.text = _healthMax.ToString("N0");

        float clamp01 = Mathf.Clamp01(_health / _healthMax);
        _healthBar.fillAmount = clamp01;
    }
    public void SetStemina(float stemina, float steminaMax)
    {
        _stemina = stemina;
        _steminaMax = steminaMax;

        _curSteminaText.text = _stemina.ToString("N0");
        _steminaMaxText.text = _steminaMax.ToString("N0");

        float clamp01 = Mathf.Clamp01(_stemina / _steminaMax);
        _steminaBar.fillAmount = clamp01;
    }
    private void OnGUI()
    {
        #region CrossHair
        // 크로스헤어

        if (_box == null)
        {
            _box = new GUIStyle(GUI.skin.box);
            _box.normal.background = _greenTexture;
        }

        _curRecoil = _weaponManager.CurRecoil;

        _centerHair.Set((Screen.width - _crossHairHeight) * 0.5f, (Screen.height - _crossHairHeight) * 0.5f, _crossHairHeight, _crossHairHeight);
        GUI.Box(_centerHair, "", _box);

        _leftHair.Set((Screen.width * 0.5f - _crossHairWidth - _subPush) - _curRecoil * _recoilOffset, (Screen.height - _crossHairHeight) * 0.5f, _crossHairWidth, _crossHairHeight);
        GUI.Box(_leftHair, "", _box);

        _rightHair.Set((Screen.width * 0.5f + _subPush) + _curRecoil * _recoilOffset, (Screen.height - _crossHairHeight) * 0.5f, _crossHairWidth, _crossHairHeight);
        GUI.Box(_rightHair, "", _box);

        _topHair.Set((Screen.width - _crossHairHeight) * 0.5f, (Screen.height * 0.5f - _crossHairWidth - _subPush) - _curRecoil * _recoilOffset, _crossHairHeight, _crossHairWidth);
        GUI.Box(_topHair, "", _box);

        _buttomHair.Set((Screen.width - _crossHairHeight) * 0.5f, (Screen.height * 0.5f + _subPush) + _curRecoil * _recoilOffset, _crossHairHeight, _crossHairWidth);
        GUI.Box(_buttomHair, "", _box);
        #endregion
    }
}
