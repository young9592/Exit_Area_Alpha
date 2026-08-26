using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    private class Slot
    {
        [SerializeField] private HandType handType;
        [SerializeField] private AudioClip clip;
        [SerializeField] private float fireDelay;
        [SerializeField] private float reloadDelay;

        public Slot(HandType handType, AudioClip clip, float fireDelay, float reloadDelay)
        {
            this.handType = handType;
            this.clip = clip;
            this.fireDelay = fireDelay;
            this.reloadDelay = reloadDelay;
        }
        public HandType Hand
        {
            get { return handType; }
            set { handType = value; }
        }

        public AudioClip GetAudioClip
        {
            get { return clip; }
        }

        public float GetFireDelay
        {
            get { return fireDelay; }
        }

        public float GetReloadDelay
        {
            get { return reloadDelay; }
        }
    }

    public enum HandType
    {
        None,
        Pistol,
        Rifle
    }

    #region Inspector
    [Header("ÂüÁ¶")]
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _rootWeaponGo;
    [SerializeField] private AudioSource _audioSource;

    [Header("Slot")]
    [SerializeField] private Slot[] _slots = new Slot[3];
    #endregion

    #region Field
    private int _curSlotIdx = 0;

    private bool _isFire = false;
    private bool _isReloading = false;

    private CTimer _fireDelayTimer = new CTimer();
    private CTimer _reloadDelayTimer = new CTimer();
    #endregion

    #region Property

    #endregion

    private void Awake()
    {
        AudioClip clip = Resources.Load<AudioClip>("Sound/AR_01_Fire");

        _slots[0] = new Slot(HandType.Rifle, clip, 0.1f, 3.5f);
        _slots[1] = new Slot(HandType.None, null, 0f, 0f);
        _slots[2] = new Slot(HandType.None, null, 0f, 0f);

        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_isFire)
        {
            if (_fireDelayTimer.AddTimer())
            {
                _player.SuccessFireDelay();
                _isFire = false;
            }
        }

        if (_isReloading)
        {
            if (_reloadDelayTimer.AddTimer())
            {
                _player.SuccessReload();
                _isReloading = false;
            }
        }
    }

    private void CheckSwap(int index, out HandType type)
    {
        if (_slots[index].Hand != HandType.None)
        {
            _rootWeaponGo.SetActive(true);
        }
        else
        {
            _rootWeaponGo.SetActive(false);
        }

        type = _slots[index].Hand;
    }
    public bool Fire()
    {
        if (_isFire || _isReloading)
        {
            return false;
        }

        _fireDelayTimer.SetTimer(_slots[_curSlotIdx].GetFireDelay);
        _isFire = true;
        _audioSource.clip = _slots[_curSlotIdx].GetAudioClip;
        _audioSource.Play();
        return true;
    }
    public bool Reload()
    {
        if (_isFire || _isReloading)
        {
            return false;
        }

        _reloadDelayTimer.SetTimer(_slots[_curSlotIdx].GetReloadDelay);
        _isReloading = true;
        return true;
    }
    public bool SelectSlot(int index, out HandType type)
    {
        if (_isFire || _isReloading)
        {
            type = HandType.None;
            return false;
        }

        CheckSwap(index, out type);
        _curSlotIdx = index;
        return true;
    }


}
