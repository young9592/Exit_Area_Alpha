using System.Collections.Generic;
using UnityEngine;


public abstract class Weapon : MonoBehaviour
{
    public enum HandType
    {
        None,
        OneHand,
        TwoHand
    }

    public enum WeaponType
    {
        None,
        HandGun,
        Rifle,
        Shotgun,
        Melee,
        Throw
    }

    #region Field
    protected int _id;
    protected string _name;
    protected float _damage;
    protected float _fireDelay;
    protected List<AudioClip> _fireClips = new List<AudioClip>();
    protected float _recoil;
    protected float _recoilMin;
    protected float _recoilMax;
    protected List<float> _reloadDelays = new List<float>();
    protected List<AudioClip> _reloadClips = new List<AudioClip>();
    protected int _ammo;
    protected int _magazine;
    protected int _pelletCount;
    protected HandType _handType;
    protected WeaponType _weaponType;

    protected bool _isFire;
    protected bool _completeFire;
    protected bool _isReload;
    protected bool _completeReload;
    protected int _returnAmmo;

    protected AudioSource _audioSource;
    #endregion

    #region Property
    public int ID => _id;
    public string Name => _name;
    public float Damage => _damage;
    public float FireDelay => _fireDelay;
    public float Recoil => _recoil;
    public float RecoilMin => _recoilMin;
    public float RecoilMax => _recoilMax;
    public List<float> ReloadDelay => _reloadDelays;
    public int Ammo => _ammo;
    public int Magazine => _magazine;
    public int PelletCount => _pelletCount;
    public HandType GetHandType => _handType;
    public WeaponType GetWeaponType => _weaponType;
    public List<AudioClip> FireClips => _fireClips;
    public List<AudioClip> ReloadClips => _reloadClips;
    public bool IsFire => _isFire;
    public bool IsReload => _isReload;
    public bool CompliteFire => _completeFire;
    public bool CompliteReload => _completeReload;
    public int ReturnAmmo => _returnAmmo;
    #endregion



    public abstract void Fire(ref float _recoil);
    public abstract void Reload(Inventory inventory);
}
