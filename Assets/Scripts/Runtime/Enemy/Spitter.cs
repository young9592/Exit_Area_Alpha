using UnityEngine;

public class Spitter : Zombie
{
    public enum State : byte
    {
        Idle,
        Trace,
        Move,
        Spit,
        Dead
    }

    [Header("Projectile Pool")]
    [SerializeField] private BulletPool _spitPool;

    [Header("Animation Parameter")]
    [SerializeField] private string _paramSpeed = "fSpeed";
    [SerializeField] private string _paramSpit = "tSpit";
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
    [SerializeField] private BoxCollider _sliderBox;
    [SerializeField] private Transform _launchPoint;
    [SerializeField] private GameObject _forwardDetectGO;

    [Header("Spit Inspector")]
    [SerializeField] private float _force = 10f;
    [SerializeField] private string _targetLayerName = "Player";
    [SerializeField] private string _blockLayerName = "Block";

    #region Field
    private State _curState = State.Idle;
    private float _curMoveSpeed = 0f;

    private float _verticalVel = 0f;
    private bool _isFalling = false;

    private int _spitCount = 0;
    private int _randDir = 0;
    private Vector3 _positionMoveDir = Vector3.zero;

    private string _attackSoundPath = "Sound/Enemy/Attack";
    private string _deadSoundPath = "Sound/Enemy/Dead";

    private int _hashSpeed;
    private int _hashSpit;
    private int _hashFalling;
    private int _hashLand;
    private int _hashJump;
    private int _hashDead;

    private CheckForward _forwardDetectScript;

    // 공격 후 딜레이
    private CTimer _attackDelayTimer = new CTimer();
    // 바로 발사하는게 아닌 잠시 대기 후 공격
    private CTimer _shootDelayTimer = new CTimer();
    // 일정횟수 공격 후 이동시간
    private CTimer _moveTimer = new CTimer();
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
        _forwardDetectScript = _forwardDetectGO.GetComponent<CheckForward>();
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
        _hashSpit = Animator.StringToHash(_paramSpit);
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
            _launchPoint == null || _sliderBox == null
            )
        {
            CPrint.Error("Spitter.cs Null Find.");
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
            _launchPoint == null || _sliderBox == null
            )
        {
            CPrint.Error("Spitter.cs Null Find.");
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
        if (_attackDelayTimer.GetCurrentTimerState)
        {
            _attackDelayTimer.AddTimer();
        }
        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            _jumpDelayTimer.AddTimer();
        }
        if (_moveTimer.GetCurrentTimerState)
        {
            if (_moveTimer.AddTimer())
            {
                _curState = State.Idle;
            }
        }
        if (_shootDelayTimer.GetCurrentTimerState)
        {
            if (_shootDelayTimer.AddTimer())
            {
                _launchPoint.rotation = Quaternion.LookRotation(_playerTr.position - transform.position, Vector3.up);
                _spitPool.SpawnBulletRb(_damage, 0f, 0.2f, _force, 4f, _launchPoint);
                _spitPool.SpawnBulletRb(_damage, 3f, 0.2f, _force, 4f, _launchPoint);
                _spitPool.SpawnBulletRb(_damage, 3f, 0.2f, _force, 4f, _launchPoint);
            }
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

    // 초기 생성 시 연결
    public void Initialize(BulletPool bulletPool)
    {
        _spitPool = bulletPool;
    }

    // 상태 변환
    protected override void ChangeState()
    {
        if (_isDead)
        {
            return;
        }

        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        Vector3 distanceVec = _playerTr.position - transform.position;
        float distance = distanceVec.sqrMagnitude;
        float dot = Vector3.Dot(transform.forward, distanceVec.normalized);

        float detectDistanceSqr = _detectDistance * _detectDistance;
        float attackDistanceSqr = _attackDistance * _attackDistance;

        int layerMask = LayerMask.GetMask(_targetLayerName, _blockLayerName);

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
            if (_spitCount >= 3 && !_attackDelayTimer.GetCurrentTimerState)
            {
                _randDir = Random.Range(0, 2);
                _positionMoveDir = (transform.right * (_randDir == 0 ? 1 : -1)).normalized;
                _curState = State.Move;
                _moveTimer.SetTimer(Random.Range(0.5f, 1f));
                _spitCount = 0;
                return;
            }

            if (_moveTimer.GetCurrentTimerState)
            {
                _curState = State.Move;
                return;
            }

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

                    Vector3 targetPos = _playerTr.position + new Vector3(0, 1.7f, 0);
                    Vector3 shootDirectionVec = (targetPos - _launchPoint.position).normalized;
                    bool checkLaunchPointForward = Physics.Raycast(_launchPoint.position, shootDirectionVec, out RaycastHit hit, _attackDistance, layerMask);

                    if (checkLaunchPointForward)
                    {
                        int targetLayerMask = layerMask & ~LayerMask.GetMask(_blockLayerName);

                        if (targetLayerMask == 1 << hit.collider.gameObject.layer)
                        {
                            _curState = State.Spit;
                        }
                        else
                        {
                            _curState = State.Trace;
                        }
                    }
                    else
                    {
                        _curState = State.Trace;
                    }
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
            case State.Move:
                Move();
                break;
            case State.Spit:
                Spit();
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
    private void Move()
    {
        if (_isDead)
        {
            return;
        }

        TargetMove(ref _positionMoveDir);
        TargetRotate(_positionMoveDir, false);
    }
    private void Spit()
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
        _shootDelayTimer.SetTimer(0.25f);
        _attackDelayTimer.SetTimer(_ATKDelay);
        _animator.SetTrigger(_hashSpit);
        _audio.PlayOneShot(Resources.Load<AudioClip>(_attackSoundPath));
        _spitCount++;
    }
    private void TargetMove(Vector3 moveDir)
    {
        if (_isDead)
        {
            return;
        }

        _curMoveSpeed = Mathf.Lerp(_curMoveSpeed, _moveSpeedMax, 1f - Mathf.Exp(-2f * Time.deltaTime));
        _curMoveSpeed = Mathf.Clamp(_curMoveSpeed, 0, _moveSpeedMax);

        Vector3 velocity = moveDir * _curMoveSpeed;
        velocity.y = _verticalVel;
        _controller.Move(velocity * Time.deltaTime);
    }
    private void TargetMove(ref Vector3 moveDir)
    {
        if (_isDead)
        {
            return;
        }

        bool fallCheck = Physics.Raycast(_forwardDetectGO.transform.position, Vector3.down, 1f);

        if (!fallCheck)
        {
            moveDir *= -1f;
        }

        _curMoveSpeed = _moveSpeedMax;

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
                if (_controller.velocity.y <= _fallingCheckSpeed)
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

        if (_curState == State.Idle || _curState == State.Spit)
        {
            return;
        }

        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (_attackDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (!_forwardDetectScript.HitCheck)
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
