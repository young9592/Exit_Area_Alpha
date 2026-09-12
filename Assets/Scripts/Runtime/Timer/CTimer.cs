using UnityEngine;

public class CTimer
{
    #region Field
    private float _time = 0f;
    private bool _onTimer = false;
    private bool _setOffTimer = false;
    #endregion

    #region Property
    public bool GetCurrentTimerState
    {
        get { return _onTimer; }
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

    public void OffTimer()
    {
        _setOffTimer = true;
    }
    public bool AddTimer()
    {

        // 타이머가 강제 종료되었을 때
        if (_setOffTimer)
        {
            _time = 0;
            _setOffTimer = false;
            _onTimer = false;
            return true;
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
