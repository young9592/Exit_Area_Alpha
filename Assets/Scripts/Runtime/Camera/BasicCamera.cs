using UnityEngine;

public partial class BasicCamera : MonoBehaviour
{
    public enum ECameraMode
    {
        Idle,
        ThirdPerson
    }

    #region Inspector
    [Header("필수 연결 목록")]
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _camera;

    // 생각해보니 1인칭만 필요하니 이것도 굳이..?
    [Header("시작 모드")]
    [SerializeField] private ECameraMode _startMode = ECameraMode.ThirdPerson;
    #endregion

    #region Field
    private Transform _camTr;
    private ECameraMode _mode;
    #endregion

    private void Start()
    {
        if (_target == null)
        {
            CPrint.Error("target is Null.");
            enabled = false;
            return;
        }

        if(_camera == null)
        {
            _camera = Camera.main;
        }

        _mode = _startMode;

        _camTr = _camera.transform;

        SetMode(_mode, true);
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        if (_camera == null)
        {
            return;
        }


        switch (_mode)
        {
            case ECameraMode.Idle:
                break;
            case ECameraMode.ThirdPerson:
                TickThird();
                break;
        }
    }

    private void SetMode(ECameraMode mode, bool snap)
    {
        _mode = mode;

        switch (_mode)
        {
            case ECameraMode.Idle:
                break;
            case ECameraMode.ThirdPerson:
                InitThird(snap);
                break;
        }
    }

    private float GetSmoothT(float sharpness)
    {
        return 1f - Mathf.Exp(-sharpness * Time.deltaTime);
    }

    private void ApplyPose(Vector3 desiredPos, Quaternion desiredRot, float sharpness, bool snap)
    {
        if (snap)
        {
            _camTr.position = desiredPos;
            _camTr.rotation = desiredRot;
            return;
        }

        float t = GetSmoothT(sharpness);

        _camTr.position = Vector3.Lerp(_camTr.position, desiredPos, t);
        _camTr.rotation = Quaternion.Slerp(_camTr.rotation, desiredRot, t);
    }
}
