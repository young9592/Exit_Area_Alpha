using UnityEngine;

public partial class BasicCamera : MonoBehaviour
{

    #region Inspector
    [Header("3ÀÎÄª")]
    [SerializeField] private Vector3 _thirdOffset = new Vector3(0f, 2f, -2f);
    [SerializeField] private float _thirdLookAtHeight = 1.5f;
    [SerializeField] private float _thirdSharpness = 5f;
    #endregion

    private void InitThird(bool snap)
    {
        Vector3 desiredPos;
        Quaternion desiredRot;

        BuildThirdPose(out desiredPos, out desiredRot);
        ApplyPose(desiredPos, desiredRot, _thirdSharpness, snap);
    }

    private void TickThird()
    {
        Vector3 desiredPos;
        Quaternion desiredRot;

        BuildThirdPose(out desiredPos, out desiredRot);
        ApplyPose(desiredPos, desiredRot, _thirdSharpness, false);
    }

    private void BuildThirdPose(out Vector3 desiredPos, out Quaternion desiredRot)
    {
        desiredPos = _target.position + (_target.rotation * _thirdOffset);

        Vector3 lookPos = _target.position + Vector3.up * _thirdLookAtHeight;
        desiredRot = Quaternion.LookRotation(lookPos - desiredPos, Vector3.up);
    }
}


