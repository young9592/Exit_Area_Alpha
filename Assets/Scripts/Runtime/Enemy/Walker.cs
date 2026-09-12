using UnityEngine;

public class Walker : Zombie
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
    [SerializeField] private GameObject _attackHitBox;
    [SerializeField] private CheckForward _forwardDetect;
    [SerializeField] private BoxCollider _sliderBox;



    #region Field
    private State _curState = State.Idle;
    private float _curMoveSpeed = 0f;

    private float _verticalVel = 0f;
    private bool _isFalling = false;

    private string _attackSoundPath = "Sound/Enemy/Attack";
    private string _deadSoundPath = "Sound/Enemy/Dead";

    private int _hashSpeed;
    private int _hashAttack;
    private int _hashFalling;
    private int _hashLand;
    private int _hashJump;
    private int _hashDead;

    // 공격 후 딜레이
    private CTimer _attackDelayTimer = new CTimer();
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
        _moveSpeedMax = Random.Range(_initMoveSpeedMin, _initMoveSpeedMax);
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

        #region NullCheck
        if (_hitBoxHead == null || _hitBoxBody == null ||
            _hitBoxArmL01 == null || _hitBoxArmL02 == null || _hitBoxHandL == null ||
            _hitBoxArmR01 == null || _hitBoxArmR02 == null || _hitBoxHandR == null ||
            _hitBoxLegL == null || _hitBoxLegR == null ||
            _attackHitBox == null || _sliderBox == null
            )
        {
            CPrint.Error("Walker.cs Null Find.");
            enabled = false;
            return;
        }
        #endregion
    }
    protected override void Update()
    {
        #region NullCheck
        base.Update();

        if (_hitBoxHead == null || _hitBoxBody == null ||
            _hitBoxArmL01 == null || _hitBoxArmL02 == null || _hitBoxHandL == null ||
            _hitBoxArmR01 == null || _hitBoxArmR02 == null || _hitBoxHandR == null ||
            _hitBoxLegL == null || _hitBoxLegR == null ||
            _attackHitBox == null || _sliderBox == null
            )
        {
            CPrint.Error("Walker.cs Null Find.");
            enabled = false;
            return;
        }
        #endregion

        #region Timers
        if (_deadTimer.GetCurrentTimerState)
        {
            if (_deadTimer.AddTimer())
            {
                gameObject.SetActive(false);
            }
            return;
        }
        if (_ATKHitTimer.GetCurrentTimerState)
        {
            if (_ATKHitTimer.AddTimer())
            {
                _attackHitBox.SetActive(false);
            }
        }
        if (_attackDelayTimer.GetCurrentTimerState)
        {
            _attackDelayTimer.AddTimer();
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

        ChangeState();
        UpdateState();
        CheckGrounded();
        CheckBlockingState();
    }
    // 상태 변환
    protected override void ChangeState()
    {
        if (_isDead)
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

                if (!_attackDelayTimer.GetCurrentTimerState)
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
                if (!_attackDelayTimer.GetCurrentTimerState)
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
        if (_isDead)
        {
            return;
        }

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
        if (_isDead)
        {
            return;
        }

        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, 0, 1f - Mathf.Exp(-20 * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);
    }
    private void Trace()
    {
        if (_isDead)
        {
            return;
        }

        Vector3 moveDir = (_playerTr.position - transform.position).normalized;

        TargetMove(moveDir);
        TargetRotate(moveDir, true);
    }
    private void Attack()
    {
        if (_isDead)
        {
            return;
        }

        if (_doJump || _isFalling || _landingTimer.GetCurrentTimerState)
        {
            return;
        }

        Vector3 moveDir = (_playerTr.position - transform.position).normalized;
        TargetRotate(moveDir, false);

        Hit hitScript = _attackHitBox.GetComponent<Hit>();
        hitScript.Initialize(_damage);

        _attackHitBox.SetActive(true);
        _attackDelayTimer.SetTimer(_ATKDelay);
        _ATKHitTimer.SetTimer(_ATKHitDuration);
        _animator.SetTrigger(_hashAttack);
        _audio.PlayOneShot(Resources.Load<AudioClip>(_attackSoundPath));
    }
    private void TargetMove(Vector3 moveDir)
    {
        if (_isDead)
        {
            return;
        }

        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, _moveSpeedMax, 1f - Mathf.Exp(-2 * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);

        Vector3 velocity = moveDir * _curMoveSpeed;
        velocity.y = _verticalVel;
        _controller.Move(velocity * Time.deltaTime);
    }
    private void TargetRotate(Vector3 moveDir, bool isLerp)
    {
        if (_isDead)
        {
            return;
        }

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
        if (_isDead)
        {
            return;
        }

        if (_controller.isGrounded)
        {
            if (_isFalling || _doJump)
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
            if (!_isFalling)
            {
                if(_controller.velocity.y <= _fallingCheckSpeed)
                {
                    _isFalling = true;
                    _animator.SetTrigger(_hashFalling);
                }
            }

        }
        // 중력 적용
        _verticalVel += _gravity * Time.deltaTime;
    }
    // 점프를 해서 걸림돌을 넘어가도록 유도
    private void CheckBlockingState()
    {
        if (_isDead)
        {
            return;
        }

        if (_controller.velocity.sqrMagnitude >= _moveSpeedMax * _moveSpeedMax * 0.2f)
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

        if (_attackDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (!_forwardDetect.HitCheck)
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
            _audio.PlayOneShot(Resources.Load<AudioClip>(_deadSoundPath));
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
        _sliderBox.enabled = toggle;
    }
    private void OnDrawGizmos()
    {
        if (_isDead)
        {
            return;
        }

        // 좀비의 감지 범위
        Gizmos.color = Color.green;
        float halfAngle = 45f;
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * 3f);
        Gizmos.DrawRay(transform.position, rightDir * 3f);
    }
}
