using UnityEngine;

public class CTimer
{
    #region Field
    private float _time = 0f;
    private bool _onTimer = false;
    #endregion

    #region Property
    public bool ToggleTimer
    {
        get { return _onTimer; }
        set { _onTimer = value; }
    }

    public float GetTime
    {
        get { return _time; }
    }
    #endregion

    public void SetTimer(float time)
    {
        _time = time;
        _onTimer = true;
    }

    public bool AddTimer()
    {

        if (!_onTimer)
        {
            // 타이머가 강제 종료되었을 때
            if (_time > 0)
            {
                _time = 0;
                return true;
            }
            // 타이머가 동작중이 아닐 때
            else
            {
                return false;
            }
        }

        _time -= Time.deltaTime;

        if (_time <= 0)
        {
            _time = 0;
            _onTimer = false;
            return true;
        }

        return false;
    }
}
