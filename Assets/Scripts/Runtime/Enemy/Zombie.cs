using System;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Flags]
    public enum State
    {
        Idle,
        Trace,
        Attack,
        Dead
    }

    #region Inspector
    [Header("필수 참조")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _playerTr;

    [Header("Zombie Status")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private float _healthMax = 100f;
    [SerializeField] private float _damage = 5f;
    [SerializeField] private float _ATKDelay = 3f;
    [SerializeField] private float _ATKHitDelay = 0.5f;
    [SerializeField] private float _moveTickSpeed = 0.3f;
    [SerializeField] private float _moveSpeedMax = 3f;
    [SerializeField] private float _detectDistance = 10f;
    [SerializeField] private float _attackDistance = 2f;

    [Header("Animation Parameter")]
    [SerializeField] private string _paramAttack = "tAttack";
    [SerializeField] private string _paramSpeed = "fSpeed";
    [SerializeField] private string _paramDead = "tDead";

    [Header("HitBox")]
    [SerializeField] private BoxCollider _hitBoxHead;
    [SerializeField] private BoxCollider _hitBoxBody;
    [SerializeField] private BoxCollider _hitBoxArmL01;
    [SerializeField] private BoxCollider _hitBoxArmL02;
    [SerializeField] private BoxCollider _hitBoxHandL;
    [SerializeField] private BoxCollider _hitBoxArmR01;
    [SerializeField] private BoxCollider _hitBoxArmR02;
    [SerializeField] private BoxCollider _hitBoxHandR;
    [SerializeField] private BoxCollider _hitBoxLegL;
    [SerializeField] private BoxCollider _hitBoxLegR;

    [SerializeField] private GameObject _ATKHitBox;
    #endregion

    #region Field
    private State _curState = State.Idle;
    private float _curMoveSpeed = 0f;

    private int _hashAttack;
    private int _hashSpeed;
    private int _hashDead;
    private CTimer _ATKDelayTimer = new CTimer();
    private CTimer _ATKHitTimer = new CTimer();
    private CTimer _deadTimer = new CTimer();
    #endregion

    private void OnEnable()
    {
        _health = _healthMax;
        _curState = State.Idle;
        ToggleCollider(true);
    }
    private void Awake()
    {
        _hashAttack = Animator.StringToHash(_paramAttack);
        _hashSpeed = Animator.StringToHash(_paramSpeed);
        _hashDead = Animator.StringToHash(_paramDead);
    }

    private void Update()
    {
        if (_deadTimer.GetCurrentTimerState)
        {
            if (_deadTimer.AddTimer())
            {
                this.gameObject.SetActive(false);
            }
        }

        if (_ATKHitTimer.GetCurrentTimerState)
        {
            if (_ATKHitTimer.AddTimer())
            {
                _ATKHitBox.SetActive(false);
            }
        }

        if (_ATKDelayTimer.GetCurrentTimerState)
        {
            _ATKDelayTimer.AddTimer();
        }


        ChangeState();
        UpdateState();
    }
    private void ChangeState()
    {
        if (_curState == State.Dead)
        {
            return;
        }


        float distance = (transform.position - _playerTr.position).sqrMagnitude;
        float dot = Vector3.Dot(transform.position, _playerTr.position);

        if (distance > _detectDistance * _detectDistance)
        {
            _curState = State.Idle;

        }
        else if (distance <= _detectDistance * _detectDistance && distance > _attackDistance * _attackDistance)
        {
            if (!_ATKDelayTimer.GetCurrentTimerState)
            {
                _curState = State.Trace;

            }
            else
            {
                _curState = State.Idle;
            }
        }
        else if (distance <= _attackDistance * _attackDistance)
        {
            if (!_ATKDelayTimer.GetCurrentTimerState)
            {
                _curState = State.Attack;

            }
            else
            {
                _curState = State.Idle;
            }

        }
    }

    private void UpdateState()
    {
        switch (_curState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Trace:
                Trace();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Dead:
                break;
        }
        float clamp01 = _curMoveSpeed / _moveSpeedMax;

        if (clamp01 < 0.001)
        {
            clamp01 = 0;
        }

        _animator.SetFloat(_hashSpeed, clamp01, 0.12f, Time.deltaTime);
    }
    private void Idle()
    {
        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, 0, 1f - Mathf.Exp(-15 * Time.deltaTime));


        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);
    }

    private void Trace()
    {
        Vector3 moveDir = (_playerTr.position - transform.position).normalized;

        TargetMove(moveDir);
        TargetRotate(moveDir);
    }

    private void Attack()
    {
        Hit hitScript = _ATKHitBox.GetComponent<Hit>();
        hitScript.Initialize(_damage);

        _ATKHitBox.SetActive(true);
        _ATKHitTimer.SetTimer(_ATKHitDelay);
        _ATKDelayTimer.SetTimer(_ATKDelay);
        _animator.SetTrigger(_hashAttack);
    }

    private void TargetMove(Vector3 moveDir)
    {
        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, _moveSpeedMax, 1f - Mathf.Exp(-5 * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);

        Vector3 velocity = moveDir * _curMoveSpeed;
        velocity.y = 0f;
        _controller.Move(velocity * Time.deltaTime);
    }
    private void TargetRotate(Vector3 moveDir)
    {
        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-5 * Time.deltaTime));
    }

    public void TakeDamage(float damage)
    {
        CPrint.Log($"좀비는 {damage}데미지를 입었습니다.");
        _health -= damage;

        Mathf.Clamp(_health, 0, _healthMax);

        if (_health <= 0)
        {
            CPrint.Log("좀비는 죽었습니다.");
            _animator.SetTrigger(_hashDead);
            _curState = State.Dead;
            ToggleCollider(false);

            _deadTimer.SetTimer(3f);
        }
    }

    private void ToggleCollider(bool toggle)
    {
        _controller.enabled = toggle;
        _hitBoxHead.enabled = toggle;
        _hitBoxBody.enabled = toggle;
        _hitBoxArmL01.enabled = toggle;
        _hitBoxArmL02.enabled = toggle;
        _hitBoxHandL.enabled = toggle;
        _hitBoxArmR01.enabled = toggle;
        _hitBoxArmR02.enabled = toggle;
        _hitBoxHandR.enabled = toggle;
        _hitBoxLegL.enabled = toggle;
        _hitBoxLegR.enabled = toggle;
    }
}
