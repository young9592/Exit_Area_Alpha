using UnityEngine;
using UnityEngine.Animations.Rigging;


public class WeaponManager : MonoBehaviour
{ 
    public enum HandState
    {
        None,
        Pistol,
        Rifle
    }

    #region Inspector
    [SerializeField] private Animator _animator;

    [SerializeField] private string _paramHandState = "nHandState";

    [SerializeField] private GameObject _rootGO;
    [SerializeField] private RigBuilder _rig;
    #endregion

    #region Field
    private HandState _hand = HandState.None;

    private int _hashHandState;
    #endregion

    #region Property
    public HandState GetHandState
    {
        get { return _hand; }
    }
    #endregion

    private void Awake()
    {
        if (_animator == null)
        {
            enabled = false;
            return;
        }

        

        _hashHandState = Animator.StringToHash(_paramHandState);
        _rig.enabled = false;
        _rootGO.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _hand = HandState.None;
            _animator.SetInteger(_hashHandState, (int)_hand);
            _rig.enabled = false;
            _rootGO.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // current not use;
            return;
            _hand = HandState.Pistol;
            _animator.SetInteger(_hashHandState, (int)_hand);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _hand = HandState.Rifle;
            _animator.SetInteger(_hashHandState, (int)_hand);
            _rig.enabled = true;
            _rootGO.SetActive(true);
        }

        else if (Input.GetKey(KeyCode.Mouse0))
        {
            _animator.SetTrigger("tFire");
        }
    }
}
