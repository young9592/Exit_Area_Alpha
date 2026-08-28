using UnityEngine;

public partial class BasicCamera : MonoBehaviour
{
    public enum ECameraMode
    {
        Idle,
        ThirdPerson
    }

    #region Inspector
    [Header("시작 모드")]
    [SerializeField] private ECameraMode _startMode = ECameraMode.ThirdPerson;

    [Header("카메라와 카메라가 따라갈 타겟")]
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _camera;

    // 플레이어가 바라보는 방향
    [Header("Player Dir View")]
    [SerializeField] private Transform _viewTr;
    [SerializeField] private float _offsetForward = 1f;
    [SerializeField] private float _offsetRight = 1f;
    [SerializeField] private float _offsetTop = -2f;

    [Header("사격 포인트")]
    [SerializeField] private Transform _firePointTr;  

    [Header("반동 오프셋")]
    [SerializeField] private float _recoilMultiple = 2f;

    [Header("디버그 연결 목록")]
    [SerializeField] private Transform _playerViewTr;

    #endregion

    #region Field
    private Transform _camTr;
    private ECameraMode _mode;
    #endregion

    private void Start()
    {
        #region Null Check
        if (_target == null || _viewTr == null || _firePointTr == null || _playerViewTr == null)
        {
            CPrint.Error("BasicCamera.cs Null Find");
            enabled = false;
            return;
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }
        #endregion

        _mode = _startMode;

        _camTr = _camera.transform;

        SetMode(_mode, true);
    }

    private void LateUpdate()
    {
        #region Null Check
        if (_target == null || _viewTr == null || _firePointTr == null || _playerViewTr == null || _camera == null)
        {
            CPrint.Error("BasicCamera.cs Null Find");
            enabled = false;
            return;
        }
        #endregion


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
        
        _firePointTr.position = desiredPos;
        _firePointTr.rotation = desiredRot;

        _viewTr.position = _target.position + _target.forward * _offsetForward + _target.right * _offsetRight + _target.up * _offsetTop;
    }

    // Camera Recoil
    public void AddRecoil(float rotX)
    {
        _camTr.rotation *= Quaternion.Euler(-rotX * _recoilMultiple, 0f, 0f);
    }

    private void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_target.transform.position, _target.transform.forward * 10f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_playerViewTr.transform.position, _playerViewTr.transform.forward * 10f);
        */
    }
}
