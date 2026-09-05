using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    #region Inspector
    [Header("Case Cover")]
    [SerializeField] private Transform _coverTr;
    #endregion

    #region Field
    private CTimer _openTimer = new CTimer();
    #endregion

    private void Update()
    {
        if (!_openTimer.GetCurrentTimerState)
        {
            return;
        }
        else
        {
            _coverTr.localRotation = Quaternion.Slerp(_coverTr.localRotation, Quaternion.Euler(-240f, 0, 0), 1f - Mathf.Exp(-2 * Time.deltaTime));
        }
    }

    public void Open()
    {
        CPrint.Log("탄약상자 열기");
        _openTimer.SetTimer(3f);
    }
}
