using UnityEngine;

public class Title : MonoBehaviour
{
    public enum MenuState
    {
        GameStart,
        GameControls,
        GameExit
    }

    #region Inspector
    [Header("Menu Select")]
    [SerializeField] private RectTransform _menuRectTr;
    [SerializeField] private RectTransform _gameStart;
    [SerializeField] private RectTransform _gameControls;
    [SerializeField] private RectTransform _gameExit;

    [Header("Menu KeyCode")]
    [SerializeField] private KeyCode _upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode _downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode _selectKey = KeyCode.Return;
    #endregion

    #region Field
    private MenuState _state = MenuState.GameStart;
    private float _endPosX = 0;
    #endregion

    #region Property
    private float GetWidth
    {
        get { return _gameStart.rect.width; }
    }
    #endregion

    private void Awake()
    {
        if (
            _menuRectTr == null ||
            _gameStart == null ||
            _gameControls == null ||
            _gameExit == null
          )
        {
            CPrint.Error("Title.cs Null Find. Check Inspector");
            enabled = false;
            return;
        }

    }


    private void Update()
    {
        InitPosX();
        InputMenuKey();
        DrawLine();
    }

    private void InputMenuKey()
    {
        if (Input.GetKeyDown(_upKey))
        {
            // 메뉴 이동
            _state -= 1;
            MenuIndexClamp();
            SetSelectLine();
        }

        if (Input.GetKeyDown(_downKey))
        {
            // 메뉴 이동
            _state += 1;
            MenuIndexClamp();
            SetSelectLine();
        }

        if (Input.GetKeyDown(_selectKey))
        {
            // 키 입력
        }
    }

    private void MenuIndexClamp()
    {
        if (_state > MenuState.GameExit)
        {
            _state = MenuState.GameStart;
        }
        else if (_state < MenuState.GameStart)
        {
            _state = MenuState.GameExit;
        }
    }

    private void SetSelectLine()
    {
        switch (_state)
        {
            case MenuState.GameStart:
                _menuRectTr.SetParent(_gameStart.transform);
                break;
            case MenuState.GameControls:
                _menuRectTr.SetParent(_gameControls.transform);
                break;
            case MenuState.GameExit:
                _menuRectTr.SetParent(_gameExit.transform);
                break;
        }
        _menuRectTr.anchoredPosition = Vector2.zero;
        _menuRectTr.sizeDelta = new Vector2(0, 3);
    }

    private void DrawLine()
    {
        if (_menuRectTr.sizeDelta.x == _endPosX)
        {
            return;
        }

        _menuRectTr.sizeDelta = new Vector2(Mathf.Lerp(_menuRectTr.sizeDelta.x, _endPosX, 1f - Mathf.Exp(-3 * Time.deltaTime)), 3);

        if (_menuRectTr.sizeDelta.x > _endPosX - 5)
        {
            _menuRectTr.sizeDelta = new Vector2(_endPosX, 3);
        }
    }

    private void InitPosX()
    {
        if (_endPosX == GetWidth)
        {
            return;
        }

        _endPosX = GetWidth;
    }
}
