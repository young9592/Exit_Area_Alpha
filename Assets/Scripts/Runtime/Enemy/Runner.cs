using UnityEngine;

public class Runner : Zombie
{

    public enum State
    {
        Idle,
        Trace,
        Attack,
        Dead
    }

    [Header("Animation Parameter")]
    [SerializeField] private string _paramSpeed = "fSpeed";
    [SerializeField] private string _paramAttack = "tAttack";
    [SerializeField] private string _paramFalling = "tFalling";
    [SerializeField] private string _paramLand = "tLand";
    [SerializeField] private string _paramJump = "tJump";
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
    [SerializeField] private BoxCollider _SliderBox;

    #region Field
    private State _curState = State.Idle;
    private float _curMoveSpeed = 0f;

    private float _verticalVel = 0f;
    private bool _isFalling = false;

    private int _hashSpeed;
    private int _hashAttack;
    private int _hashFalling;
    private int _hashLand;
    private int _hashJump;
    private int _hashDead;

    // 공격 후 딜레이
    private CTimer _ATKDelayTimer = new CTimer();
    // 공격 판정 충돌박스 삭제 시간
    private CTimer _ATKHitTimer = new CTimer();
    // 랜딩 모션 타이머
    private CTimer _landingTimer = new CTimer();
    // 점프 모션 타이머
    private CTimer _jumpDelayTimer = new CTimer();
    // 사망 시 오브젝트 삭제 시간
    private CTimer _deadTimer = new CTimer();
    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();

        // 상태 초기화
        _health = _healthMax;
        _curState = State.Idle;
        ToggleCollider(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Awake()
    {
        #region StringToHash
        _hashSpeed = Animator.StringToHash(_paramSpeed);
        _hashAttack = Animator.StringToHash(_paramAttack);
        _hashFalling = Animator.StringToHash(_paramFalling);
        _hashLand = Animator.StringToHash(_paramLand);
        _hashJump = Animator.StringToHash(_paramJump);
        _hashDead = Animator.StringToHash(_paramDead);
        #endregion
    }
    private void Update()
    {
        #region Timers
        if (_deadTimer.GetCurrentTimerState)
        {
            if (_deadTimer.AddTimer())
            {
                gameObject.SetActive(false);
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
        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            _jumpDelayTimer.AddTimer();
        }
        if (_landingTimer.GetCurrentTimerState)
        {
            if (!_landingTimer.AddTimer())
            {
                return;
            }
        }
        #endregion

        if (_isDead)
        {
            return;
        }

        CheckBlockingState();
        CheckGrounded();
        ChangeState();
        UpdateState();
    }
    // 상태 변환
    protected override void ChangeState()
    {
        if (_curState == State.Dead)
        {
            return;
        }

        Vector3 distanceVec = _playerTr.position - transform.position;
        float distance = distanceVec.sqrMagnitude;
        // 내적 : 정면 90도 체크
        float dot = Vector3.Dot(transform.forward, distanceVec.normalized);

        float detectDistanceSqr = _detectDistance * _detectDistance;
        float attackDistanceSqr = _attackDistance * _attackDistance;

        if (!_isAlertMode)
        {
            if (distance > detectDistanceSqr)
            {
                _curState = State.Idle;

            }
            // 발각 거리에 들어왔을 때
            else if (distance <= detectDistanceSqr)
            {
                if (dot < Mathf.Cos(45f * Mathf.Deg2Rad))
                {
                    return;
                }
                _isAlertMode = true;
            }
        }
        else
        {
            if (distance > attackDistanceSqr)
            {

                if (!_ATKDelayTimer.GetCurrentTimerState)
                {
                    _curState = State.Trace;
                }
                else
                {
                    _curState = State.Idle;
                    TargetRotate(distanceVec.normalized, true);

                }
            }
            else if (distance <= attackDistanceSqr)
            {
                if (!_ATKDelayTimer.GetCurrentTimerState)
                {
                    _curState = State.Attack;
                }
                else
                {
                    _curState = State.Idle;
                    TargetRotate(distanceVec.normalized, true);
                }
            }
        }
    }
    // 행동 수행
    protected override void UpdateState()
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

        if (clamp01 < 0.01)
        {
            clamp01 = 0;
        }

        _animator.SetFloat(_hashSpeed, clamp01, 0.12f, Time.deltaTime);
    }
    private void Idle()
    {
        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, 0, 1f - Mathf.Exp(-20 * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);
    }
    private void Trace()
    {
        Vector3 moveDir = (_playerTr.position - transform.position).normalized;

        TargetMove(moveDir);
        TargetRotate(moveDir, true);
    }
    private void Attack()
    {
        Vector3 moveDir = (_playerTr.position - transform.position).normalized;
        TargetRotate(moveDir, false);

        Hit hitScript = _ATKHitBox.GetComponent<Hit>();
        hitScript.Initialize(_damage);

        _ATKHitBox.SetActive(true);
        _ATKDelayTimer.SetTimer(_ATKDelay);
        _ATKHitTimer.SetTimer(_ATKHitDuration);
        _animator.SetTrigger(_hashAttack);
    }
    private void TargetMove(Vector3 moveDir)
    {
        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, _moveSpeedMax, 1f - Mathf.Exp(-2 * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);

        Vector3 velocity = moveDir * _curMoveSpeed;
        velocity.y = _verticalVel;
        _controller.Move(velocity * Time.deltaTime);
    }
    private void TargetRotate(Vector3 moveDir, bool isLerp)
    {
        // y축이 차이닐 시 기울어지니 보정
        moveDir.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

        if (isLerp)
        {
            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-5 * Time.deltaTime));
        }
        else
        {
            // 즉시 해당 방향으로 회전
            transform.rotation = targetRot;
        }
    }
    // 떨어지는 중인지 체크
    private void CheckGrounded()
    {
        if (_controller.isGrounded)
        {
            if (_isFalling)
            {
                _animator.SetTrigger(_hashLand);
                _landingTimer.SetTimer(_landingDelay);
                _curMoveSpeed = 0f;
                _animator.SetFloat(_hashSpeed, _curMoveSpeed);
                _isFalling = false;
                _doJump = false;
            }

            if (_verticalVel < 0.0f)
            {
                _verticalVel = _groundStick;
            }
        }
        else
        {
            if (!_isFalling && _controller.velocity.y <= -3f)
            {
                _isFalling = true;
                _animator.SetTrigger(_hashFalling);
            }

            // 중력 적용
            _verticalVel += _gravity * Time.deltaTime;

            // isGrounded이 인식 못할때 방지책
            if (!_doJump && _controller.velocity.y <= 0.1f && _verticalVel <= _fallResetVelocity)
            {
                _verticalVel = 0f;
            }

        }
    }
    // 점프를 해서 걸림돌을 넘어가도록 유도
    private void CheckBlockingState()
    {
        if (_controller.velocity.sqrMagnitude >= 0.001f)
        {
            return;
        }

        // 걸리는 경우가 추적 상태일때만이기 때문에 다른 상태일땐 return
        if (_curState == State.Idle || _curState == State.Attack)
        {
            return;
        }

        // 점프 쿨타임
        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        int layerMask = LayerMask.GetMask("Block");
        Physics.Raycast(transform.position + transform.up * 0.1f, transform.forward, out RaycastHit hit, 1f, layerMask);

        if (hit.collider == null)
        {
            return;
        }

        _animator.SetTrigger(_hashJump);
        _verticalVel = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);
        _jumpDelayTimer.SetTimer(_jumpDelay);
        _doJump = true;
    }
    public override void TakeDamage(float damage)
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
            _isDead = true;
        }
        else
        {
            _isAlertMode = true;
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
        _SliderBox.enabled = toggle;
    }
    private void OnDrawGizmos()
    {
        // 좀비의 블록 감지범위
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + transform.up * 0.1f, transform.forward * 1f);

        // 좀비의 감지 범위
        Gizmos.color = Color.green;
        float halfAngle = 45f;
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * 3f);
        Gizmos.DrawRay(transform.position, rightDir * 3f);
    }
}

