using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public partial class Player : MonoBehaviour
{
    // 체력과 스테미너 갱신 이벤트 핸들러
    public event Action<float, float> OnSetHealth;
    public event Action<float, float> OnSetStemina;

    // 무기 줍기 및 버리기
    public event Action OnPickup;
    public event Action OnDrop;

    #region Inspector
    [Header("Manager")]
    [SerializeField] private WeaponManager _weaponManager;

    [Header("Animation and Controller")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private RigBuilder _rig;

    [Header("Animation Parameter")]
    [SerializeField] private string _paramAnimationMoveSpeed = "fAniMoveSpeed";
    [SerializeField] private string _paramInputX = "fInputX";
    [SerializeField] private string _paramInputY = "fInputY";

    [SerializeField] private string _paramJump = "tJump";
    [SerializeField] private string _paramLand = "tLand";

    [SerializeField] private string _paramHandState = "nHandState";
    [SerializeField] private string _paramFire = "tFire";
    [SerializeField] private string _paramFireDelay = "bFireDelay";
    [SerializeField] private string _paramReload = "tReload";

    [SerializeField] private string _paramDead = "tDead";

    [Header("Mouse")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _playerPivot;

    [SerializeField] private float _mouseSensitivity = 3.0f;
    [SerializeField] private float _mousePitchMin = -50.0f;
    [SerializeField] private float _mousePitchMax = 20.0f;

    [Header("Key")]
    [SerializeField] private KeyCode _keySlot01 = KeyCode.Alpha1;
    [SerializeField] private KeyCode _keySlot02 = KeyCode.Alpha2;
    [SerializeField] private KeyCode _keySlot03 = KeyCode.Alpha3;

    [SerializeField] private string _keyHor = "Horizontal";
    [SerializeField] private string _keyVer = "Vertical";

    [SerializeField] private string _keyMouseX = "Mouse X";
    [SerializeField] private string _keyMouseY = "Mouse Y";

    [SerializeField] private KeyCode _keyFire = KeyCode.Mouse0;
    [SerializeField] private KeyCode _keyReload = KeyCode.R;
    [SerializeField] private KeyCode _keyJump = KeyCode.Space;
    [SerializeField] private KeyCode _keySprint = KeyCode.LeftShift;

    [SerializeField] private KeyCode _keyPickup = KeyCode.F;
    [SerializeField] private KeyCode _keyDrop = KeyCode.G;
    #endregion

    #region Field
    // 수직 속도
    private float _verticalVel;

    // 현재 점프중인가?
    private bool _doJump;

    // 최근 달렸나?
    private bool _curentRun;
    private CTimer _steminaRecoverTimer = new CTimer();

    // 현재 사망 상태인가?
    private bool _isDead;

    // 랜딩시 점프 쿨타임
    private CTimer _jumpDelayTimer = new CTimer();
    // 마우스 이동
    private float _mouseX;
    private float _mouseY;

    // 해시변환
    private int _hashAnimationMoveSpeed;
    private int _hashJump;
    private int _hashLand;
    private int _hashInputX;
    private int _hashInputY;
    private int _hashHandState;
    private int _hashFire;
    private int _hashFireDelay;
    private int _hashReload;
    private int _hashDead;
    #endregion

    // 커스텀 함수 아님
    private void Reset()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _rig = GetComponent<RigBuilder>();
    }

    private void Awake()
    {
        #region Null Check
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_rig == null)
        {
            _rig = GetComponent<RigBuilder>();
        }

        if (_weaponManager == null || _cameraPivot == null || _playerPivot == null)
        {
            CPrint.Error("Player.cs Null find.");
            enabled = false;
            return;
        }
        #endregion

        #region StringToHash
        // 해시 준비
        _hashAnimationMoveSpeed = Animator.StringToHash(_paramAnimationMoveSpeed);
        _hashInputX = Animator.StringToHash(_paramInputX);
        _hashInputY = Animator.StringToHash(_paramInputY);
        _hashHandState = Animator.StringToHash(_paramHandState);
        _hashFire = Animator.StringToHash(_paramFire);
        _hashFireDelay = Animator.StringToHash(_paramFireDelay);
        _hashReload = Animator.StringToHash(_paramReload);
        _hashJump = Animator.StringToHash(_paramJump);
        _hashLand = Animator.StringToHash(_paramLand);
        _hashDead = Animator.StringToHash(_paramDead);
        #endregion

        _rig.enabled = false;

        _weaponManager.OnFire += SuccessFireDelay;
        _weaponManager.OnReload += SuccessReload;
        _weaponManager.OnPickup += InitSwap;
    }

    private void Start()
    {
        InitSwap(0);
    }
    private void Update()
    {
        #region Null Check
        if (
            _controller == null || _animator == null || _rig == null ||
            _weaponManager == null || _cameraPivot == null || _playerPivot == null
            )
        {
            CPrint.Error("Player.cs Null find.");
            return;
        }
        #endregion

        // 사망
        if (_isDead)
        {
            return;
        }

        // Timer
        if (_jumpDelayTimer.GetCurrentTimerState)
        {
            _jumpDelayTimer.AddTimer();
        }
        if (_steminaRecoverTimer.GetCurrentTimerState)
        {
            if (_steminaRecoverTimer.AddTimer())
            {
                _curentRun = false;
            }
        }


        Move();
        Fire();
        Reload();
        Swap();
        PickUp();
        Drop();
        RecoverStemina();
    }
    // 이동방향 설계
    private Vector3 BuildMoveDirection(Vector3 input)
    {
        // 플레이어 forward / right를 바닥 평면으로 투영
        Vector3 camF = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 dir = camF * input.z + camR * input.x;

        // 상하좌우 → 합성 → 대각선 → 속도 2배를 방지
        return dir.normalized;
    }
    // 점프 설계
    private bool TickJumpAndGravity(bool jumpKeyDown)
    {
        bool jumpd = false;

        // isCrounded : 바닥에 닿아있다고 판단하는 상태
        if (_controller.isGrounded)
        {
            // 점프중 상태일때 바닥에 닿으면 랜딩 모션을 출력
            if (_doJump)
            {
                _animator.SetTrigger(_hashLand);
                _doJump = false;
                _jumpDelayTimer.SetTimer(1f);
            }

            // 바닥에 붙어있을 때 y속도가 음수면 너무 떨어지지 않게 고정하고 싶습니다.
            if (_verticalVel < 0.0f)
            {
                _verticalVel = _groundStick;
            }

            if (jumpKeyDown && !_jumpDelayTimer.GetCurrentTimerState)
            {
                _verticalVel = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);
                jumpd = true;
                _doJump = true;
            }
        }
        // 매 프레임 중력을 적용합니다. [ 속도에 가속도를 누적 ] 
        _verticalVel += _gravity * Time.deltaTime;

        return jumpd;
    }
    // 플레이어 회전 마우스로 설계
    private void MouseRotate()
    {
        float mx = Input.GetAxis(_keyMouseX);
        float my = Input.GetAxis(_keyMouseY);

        _mouseX += mx * _mouseSensitivity;
        _mouseY -= my * _mouseSensitivity;

        _mouseY = Mathf.Clamp(_mouseY, _mousePitchMin, _mousePitchMax);

        Quaternion rotTr = Quaternion.Euler(0f, _mouseX, 0f);
        Quaternion rotPivot = Quaternion.Euler(_mouseY, 0f, 0f);
        _cameraPivot.localRotation = rotPivot;
        _playerPivot.localRotation = rotPivot;
        transform.rotation = rotTr;
    }
    private void Move()
    {
        /*
        1. 입력 받기
        2. 이동 방향 계산 [ 카메라 ]
        3. 이동 속도 계산
        4. 점프 + 중력 업데이트 [ Y 속도 계산 ]
        5. 실제 이동 [ 캐릭터 컨트롤러 기준 ]
        6. 회전 처리
        7. 파라미터 업데이트 [ 스테이트 전환 ]
        */

        // 1. 입력 받기
        float h = Input.GetAxisRaw(_keyHor);
        float v = Input.GetAxisRaw(_keyVer);

        // 결국 필요한건 방향 벡터입니다.
        Vector3 input = new Vector3(h, 0, v);

        _animator.SetFloat(_hashInputX, h);
        _animator.SetFloat(_hashInputY, v);

        // ClampMagnitude : 벡터 크기 제한 [ 대각선 이동을 통해서 빠른 이동을 제한 ]
        input = Vector3.ClampMagnitude(input.normalized, 1.0f);

        // 점프
        bool jumpKeyDown = Input.GetKeyDown(_keyJump);

        // 2. 이동 방향 계산
        // 회전 + 애니메이션 처리도 고려함
        Vector3 moveDir = (input.sqrMagnitude > 0.0001f) ? BuildMoveDirection(input) : Vector3.zero;


        // 3. 이동 속도
        bool sprintKeyDown = Input.GetKey(_keySprint);
        float speed = _walkSpeed * (sprintKeyDown && _stemina != 0 ? _sprintMultiply : 1f);

        // 4. 점프
        // 업데이트에서 한번만 체크해서 넘긴다.
        bool jumpedThisFrame = TickJumpAndGravity(jumpKeyDown);

        if (jumpedThisFrame)
        {
            _animator.SetTrigger(_hashJump);
        }

        // 5. 이동
        // 수평 + 수직 속도를 합쳐서 Move
        Vector3 velocity = moveDir * speed;

        // 스프린트 키를 누른 상태에서 수평 이동량이 있다면 스테미너 감소
        if (sprintKeyDown && velocity.sqrMagnitude > 0.0001)
        {
            ReduceStemina();
        }
        else if (!sprintKeyDown && _curentRun && !_steminaRecoverTimer.GetCurrentTimerState)
        {
            _steminaRecoverTimer.SetTimer(3f);
        }

        velocity.y = _verticalVel;

        // 물리 힘은 아니라서 요청한 만큼 이동 시킨다.
        // 다만 충돌은 반영해준다.
        _controller.Move(velocity * Time.deltaTime);

        // 6. 회전 처리
        MouseRotate();

        // 7. 파라미터 업데이트 수행
        float moveAnimationMultiple = (sprintKeyDown && _stemina != 0 ? 1.5f : 1f);

        _animator.SetFloat(_hashAnimationMoveSpeed, moveAnimationMultiple);

    }
    private void Fire()
    {
        if (Input.GetKey(_keyFire))
        {
            if (_weaponManager.Fire())
            {
                _animator.SetTrigger(_hashFire);
                _animator.SetBool(_hashFireDelay, true);
            }
        }
    }
    private void Reload()
    {
        if (Input.GetKeyDown(_keyReload))
        {
            if (_weaponManager.Reload())
            {
                _animator.SetTrigger(_hashReload);
                _rig.enabled = false;
            }
        }
    }
    private void Swap()
    {
        // 애니메이션 안정성
        if (_doJump || _jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (Input.GetKeyDown(_keySlot01))
        {
            if (_weaponManager.SelectSlot(0, out Weapon.HandType type))
            {
                // 슬롯 0번
                _rig.enabled = type != Weapon.HandType.None;
                _animator.SetInteger(_hashHandState, (int)type);
            }
        }
        else if (Input.GetKeyDown(_keySlot02))
        {
            if (_weaponManager.SelectSlot(1, out Weapon.HandType type))
            {
                // 슬롯 1번
                _rig.enabled = type != Weapon.HandType.None;
                _animator.SetInteger(_hashHandState, (int)type);
            }
        }
        else if (Input.GetKeyDown(_keySlot03))
        {
            if (_weaponManager.SelectSlot(2, out Weapon.HandType type))
            {
                // 슬롯 2번
                _rig.enabled = type != Weapon.HandType.None;
                _animator.SetInteger(_hashHandState, (int)type);
            }
        }
    }
    // 아이템 줍기
    private void PickUp()
    {
        // 애니메이션 안정성
        if (_doJump || _jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (Input.GetKeyDown(_keyPickup))
        {
            OnPickup?.Invoke();
        }
    }
    // 아이템 버리기
    private void Drop()
    {
        // 애니메이션 안정성
        if (_doJump || _jumpDelayTimer.GetCurrentTimerState)
        {
            return;
        }

        if (Input.GetKeyDown(_keyDrop))
        {
            OnDrop?.Invoke();
        }
    }
    // 초기 0번 슬롯 무기 착용
    private void InitSwap(int index)
    {
        if (_weaponManager.SelectSlot(index, out Weapon.HandType type))
        {
            _rig.enabled = type != Weapon.HandType.None;
            _animator.SetInteger(_hashHandState, (int)type);
        }
    }
    // 발사완료 트리거
    public void SuccessFireDelay()
    {
        _animator.SetBool(_hashFireDelay, false);
    }
    // 재장전 완료 트리거
    public void SuccessReload()
    {
        _rig.enabled = true;
    }
    // 스테미너 회복
    private void RecoverStemina()
    {
        if (_stemina == _steminaMax)
        {
            return;
        }

        if (!_curentRun)
        {
            _stemina += _steminaConsume * Time.deltaTime;
            _stemina = Mathf.Clamp(_stemina, 0, _steminaMax);
            OnSetStemina?.Invoke(_stemina, _steminaMax);
        }
    }
    // 스테미너 소모
    private void ReduceStemina()
    {
        if (_stemina == 0)
        {
            return;
        }

        _curentRun = true;
        _stemina -= _steminaConsume * Time.deltaTime;
        _stemina = Mathf.Clamp(_stemina, 0, _steminaMax);
        OnSetStemina?.Invoke(_stemina, _steminaMax);
    }
    // 피격
    public void TakeDamage(float damage)
    {
        CPrint.Log($"플레이어는 {damage}의 데미지를 입었습니다.");
        _health -= damage;
        _health = Mathf.Clamp(_health, 0, _healthMax);
        OnSetHealth?.Invoke(_health, _healthMax);

        if (_health == 0 && !_isDead)
        {
            // game over
            CPrint.Warn("플레이어가 사망하였습니다.");
            _animator.SetTrigger(_hashDead);
            _controller.enabled = false;
            _rig.enabled = false;
            _isDead = true;
        }
    }
}
