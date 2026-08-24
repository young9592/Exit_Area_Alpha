using UnityEngine;

public class Player : MonoBehaviour
{

    #region Inspector
    // 일반적인 3인칭 캐릭터를 쓸 때 세트
    // ㄴ 애니메이션 붙는 객체는 이 형태가 기본
    [Header("참조")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;

    [Header("카메라 기준 이동 [ 옵션 ]")]
    [SerializeField] private Transform _cameraTr; // 비우면 월드 기준으로 동작 시키겠다.

    [Header("이동")]
    [SerializeField] private float _wallkSpeed = 5.0f;
    [SerializeField] private float _runMultiplier = 1.8f;
    [SerializeField] private float _rotateSharpness = 15.0f; // 클수록 빨리 회전

    // 리지드 바디를 안쓰기 때문에 들어온 변수들
    [Header("점프")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;
    // 캐릭터가 땅에 박혀있는걸 방지
    [SerializeField] private float _groundStick = -2.0f;

    /*
    ㆍ 9.81

    - 지구 중력가속도의 대표값

    v += g * dt;

    9.81 써야 하나?
    ㄴ 기준점으로 생각하면 된다.
     */

    // 헝가리안 추천
    [Header("애니메이터 파라미터")]
    [SerializeField] private string _paramSpeed = "fSpeed";     // float
    [SerializeField] private string _paramRun = "bRun";         // bool
    [SerializeField] private string _paramJump = "tJump";       // trigger
    [SerializeField] private string _paramLand = "tLand";       // trigger

    // 보간값
    [Header("애니메이터 튜닝")]
    [SerializeField] private float _speedDamp = 0.12f;
    #endregion

    #region Field
    // 수직 속도
    private float _verticalVel;

    private int _hashSpeed;
    private int _hashRun;
    private int _hashJump;
    private int _hashLand;
    private bool _hasRunParam;
    private bool _hasJumpParam;
    private bool _hasLandParam;

    // 현재 점프중인가?
    private bool _doJump;
    #endregion

    // 커스텀 함수 아님
    private void Reset()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_cameraTr == null && Camera.main != null)
        {
            _cameraTr = Camera.main.transform;
        }

        // 해시 준비
        _hashSpeed = Animator.StringToHash(_paramSpeed);

        // 런 파라미터 준비 체크 + 생성
        // 런이나 점프는 오브젝트마다 안할수도 있으니 없으면 null로 반환
        _hasRunParam = !string.IsNullOrEmpty(_paramRun);
        if (_hasRunParam)
        {
            _hashRun = Animator.StringToHash(_paramRun);
        }

        _hasJumpParam = !string.IsNullOrEmpty(_paramJump);
        if (_hasJumpParam)
        {
            _hashJump = Animator.StringToHash(_paramJump);
        }

        _hasLandParam = !string.IsNullOrEmpty(_paramLand);
        if (_hasLandParam)
        {
            _hashLand = Animator.StringToHash(_paramLand);
        }
    }

    private void Start()
    {
        bool useCameraRelative = (_cameraTr != null);

        if (_cameraTr != null)
        {
            CPrint.Log($"카메라 = {_cameraTr.name}");
        }
        else
        {
            CPrint.Warn("카메라가 없습니다. / 월드 기준 이동");
        }
    }

    private void Update()
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

        if (_controller == null || _animator == null)
        {
            CPrint.Error("Player.cs null fine");
            return;
        }

        // 1. 입력 받기

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 결국 필요한건 방향 벡터입니다.
        Vector3 input = new Vector3(h, 0, v);

        // ClampMagnitude : 벡터 크기 제한 [ 대각선 이동을 통해서 빠른 이동을 제한 ]
        input = Vector3.ClampMagnitude(input.normalized, 1.0f);

        // 달리기 / 점프
        bool isRunKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space);

        // 2. 이동 방향 계산

        // 회전 + 애니메이션 처리도 고려함
        Vector3 moveDir = (input.sqrMagnitude > 0.0001f) ? BuildMoveDirection(input) : Vector3.zero;

        // 3. 이동 속도
        float speed = _wallkSpeed * (isRunKey ? _runMultiplier : 1.0f);

        // 4. 점프

        // 업데이트에서 한번만 체크해서 넘긴다.
        bool jumpedThisFrame = TickJumpAndGravity(jumpKeyDown);

        if (_hasJumpParam && jumpedThisFrame)
        {
            _animator.SetTrigger(_hashJump);
        }

        // 5. 이동
        // 수평 + 수직 속도를 합쳐서 Move
        Vector3 velocity = moveDir * speed;
        velocity.y = _verticalVel;

        // 물리 힘은 아니라서 요청한 만큼 이동 시킨다.
        // 다만 충돌은 반영해준다.
        _controller.Move(velocity * Time.deltaTime);

        // 6. 회전 처리
        TickRotate(moveDir);

        // 7. 파라미터 업데이트 수행
        float speed01 = moveDir.magnitude * (isRunKey ? 1.0f : 0.5f);

        _animator.SetFloat(_hashSpeed, speed01, _speedDamp, Time.deltaTime);

        if (_hasRunParam)
        {
            _animator.SetBool(_hashRun, isRunKey && moveDir.sqrMagnitude > 0.0001f);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            _animator.SetTrigger("tAttack");
        }
    }

    // 이동방향 설계
    private Vector3 BuildMoveDirection(Vector3 input)
    {
        // 카메라가 없으면 월드 기준
        if (_cameraTr == null)
        {
            // 상하좌우 → 합성 → 대각선 → 속도 2배를 방지
            return input.normalized;
        }

        // 카메라 forward / right를 바닥 평면으로 투영
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTr.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTr.right, Vector3.up).normalized;

        Vector3 dir = camF * input.z + camR * input.x;

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
            }

            // 바닥에 붙어있을 때 y속도가 음수면 너무 떨어지지 않게 고정하고 싶습니다.
            if (_verticalVel < 0.0f)
            {
                _verticalVel = _groundStick;
            }

            if (jumpKeyDown)
            {
                // 1차 → 점프 공식
                // v = sqrt ( h * -2g )

                // → 시작은 등가속도 운동 찾아보기
                // v^ = V0^ + 2 + a Dy
                // v0^ = -2gh

                // 점프는 원하는 높이에서 속도가 0이 되도록 시작 속도를 역으로 계산합니다.
                _verticalVel = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);

                jumpd = true;
                _doJump = true;
            }
        }
        // 매 프레임 중력을 적용합니다. [ 속도에 가속도를 누적 ] 
        _verticalVel += _gravity * Time.deltaTime;

        return jumpd;
    }

    // 이동 방향이 있을 때만 회전
    private void TickRotate(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // 현재 바라보는 방향에서 목표 방향으로 부드럽게 회전
        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

        // 회전 보간
        transform.rotation = Quaternion.Slerp
            (
                transform.rotation,
                targetRot,
                1.0f - Mathf.Exp(-_rotateSharpness * Time.deltaTime)
            );
    }
}
