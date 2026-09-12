using UnityEngine;

public class TransportChopper : MonoBehaviour
{
    public enum State : byte
    {
        GoChopper,
        Around,
        Landing
    }
    #region Inspector
    [Header("Chopper Inspector")]
    [SerializeField] private State _curState = State.GoChopper;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _landingYOffset = 4f;
    [SerializeField] private MeshRenderer _body;
    [SerializeField] private MeshRenderer _rotor_back;
    [SerializeField] private MeshRenderer _rotor_front;

    [Header("Exit Time")]
    [SerializeField] private float _goChopperTime = 240f;
    [SerializeField] private float _landingTime = 60f;

    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Trigger")]
    [SerializeField] private TriggerArea _triggerArea;
    #endregion

    #region Field
    private CTimer _nextStateTimer = new CTimer();
    private bool _rescue = false;
    #endregion

    private void Start()
    {
        #region NullCheck
        if (_target == null)
        {
            CPrint.Error("TransportChopper.cs Null Find.");
            enabled = false;
            return;
        }
        if (_body == null || _rotor_back == null || _rotor_front == null)
        {
            CPrint.Error("TransportChopper.cs Null Find.");
            enabled = false;
            return;
        }
        #endregion

        _triggerArea.OnAreaEnter += RescueStart;
    }


    private void Update()
    {
        #region NullCheck
        if (_target == null)
        {
            enabled = false;
            return;
        }
        if (_body == null || _rotor_back == null || _rotor_front == null)
        {
            enabled = false;
            return;
        }
        #endregion

        if (!_rescue)
        {
            return;
        }

        if (_nextStateTimer.GetCurrentTimerState)
        {
            if (_nextStateTimer.AddTimer())
            {
                _curState++;
            }
        }

        CurrentStateActive();

        switch (_curState)
        {
            case State.GoChopper:
                GoChopper();
                break;
            case State.Around:
                Around();
                break;
            case State.Landing:
                Landing();
                break;
        }
    }
    public void RescueStart(int Area)
    {
        // Area 변수는 GameStart에서만 필요하니 사용 안함
        CPrint.Log("구조 무전 받음");
        _rescue = true;
        _triggerArea.OnAreaEnter -= RescueStart;
    }
    private void GoChopper()
    {
        if (!_nextStateTimer.GetCurrentTimerState)
        {
            CPrint.Log("구조 헬기 출발");
            _nextStateTimer.SetTimer(_goChopperTime);
        }
    }

    private void Around()
    {
        if (!_nextStateTimer.GetCurrentTimerState)
        {
            CPrint.Log("구조 헬기 착륙 준비중");
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.Play();

            _nextStateTimer.SetTimer(_landingTime);
            _body.enabled = true;
            _rotor_back.enabled = true;
            _rotor_front.enabled = true;
        }
    }

    private void Landing()
    {
        if (!_nextStateTimer.GetCurrentTimerState)
        {
            CPrint.Log("구조 헬기 착륙중");
            _nextStateTimer.SetTimer(999f);
        }
    }

    private void CurrentStateActive()
    {
        Vector3 prevPos, desirePos, directionToTarget;
        float rot;

        switch (_curState)
        {
            case State.GoChopper:
                break;
            case State.Around:
                rot = _speed * Time.deltaTime;

                prevPos = transform.position;

                transform.RotateAround(_target.transform.position, Vector3.up, rot);

                desirePos = transform.position;

                directionToTarget = desirePos - prevPos;
                directionToTarget.y = 0.0f;

                transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
                break;
            case State.Landing:
                desirePos = _target.transform.position + new Vector3(0, _landingYOffset, 0);

                transform.position = Vector3.Lerp(transform.position, desirePos, 1f - Mathf.Exp(-0.5f * Time.deltaTime));

                directionToTarget = desirePos - transform.position;
                directionToTarget.y = 0.0f;

                transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
                break;
        }
    }
}
