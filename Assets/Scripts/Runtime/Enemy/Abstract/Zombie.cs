using UnityEngine;

public abstract class Zombie : MonoBehaviour
{
    #region Inspector
    [Header("필수 참조")]
    [SerializeField] protected Animator _animator;
    [SerializeField] protected CharacterController _controller;
    [SerializeField] protected static Transform _playerTr;

    [Header("Zombie Status")]
    [SerializeField] protected float _health = 100f;
    [SerializeField] protected float _healthMax = 100f;
    [SerializeField] protected float _damage = 5f;
    [SerializeField] protected float _ATKDelay = 3f;
    [SerializeField] protected float _ATKHitDuration = 0.5f;
    [SerializeField] protected float _attackDistance = 2f;
    [SerializeField] protected float _moveSpeedMax = 3f;
    [SerializeField] protected float _jumpHeight = 3.5f;
    [SerializeField] protected float _jumpDelay = 4f;
    [SerializeField] protected float _detectDistance = 10f;
    // 지금 게임 상황에 따라 빠지게 되었습니다.
    //[SerializeField] protected float _detectDuration = 10f;
    [SerializeField] protected float _landingDelay = 1.5f;

    [Header("RigidBody")]
    [SerializeField] protected float _groundStick = -2.0f;
    [SerializeField] protected float _gravity = -9.8f;
    [SerializeField] protected float _fallResetVelocity = -18f;
    #endregion

    #region Field
    protected static int _count = 0;
    protected static bool _isAlertMode = false;
    protected bool _doJump = false;
    protected bool _isDead = false;
    #endregion

    #region Property
    public static int GetSpawnCount => _count;
    #endregion

    protected abstract void ChangeState();
    protected abstract void UpdateState();
    public abstract void TakeDamage(float damage);

    protected void Start()
    {
        if(_playerTr == null)
        {
            _playerTr = Player.PlayerTr;
        }
    }

    protected virtual void OnEnable()
    {
        // 현재 생성된 좀비 수
        _count++;

        CPrint.Log($"현재 생존한 좀비의 수 : {_count}마리");

        _isDead = false;
    }

    protected virtual void OnDisable()
    {
        _count--;

        CPrint.Log($"현재 생존한 좀비의 수 : {_count}마리");

        // 모두 죽으면 경계모드 해제
        if (_isAlertMode && _count == 0)
        {
            _isAlertMode = false;
            CPrint.Log($"경계모드 : {_isAlertMode}");
        }
    }
}
