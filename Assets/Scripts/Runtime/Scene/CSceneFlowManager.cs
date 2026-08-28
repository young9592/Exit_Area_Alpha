using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSceneFlowManager : MonoBehaviour
{
    #region Inspector
    [Header("Caltalog")]
    [SerializeField] private CSceneCatalog _catalog;

    // 연출 담당
    [Header("UI전환")]
    [SerializeField] private CSceneTransitionUI _transitionUI;

    // 디버그 모드
    [Header("옵션 - 핫키")]
    [SerializeField] private bool _enableHotKeys = true;

    [Header("옵션 - 유지")]
    [SerializeField] private bool _dontDestroyOnLoad = true;

    [Header("옵션 - 전환")]
    [SerializeField] private float _fadeDuration = 1.5f;
    #endregion

    #region Field
    // Singleton
    private static CSceneFlowManager _instance;
    private int _cursorIndex = 0;
    private bool _isLoading = false;

    private int _curTempSceneIndex = 0;
    #endregion

    private void Awake()
    {
        // 싱글톤 예외 사항 방지
        if (_instance != null && _instance != this)
        {
            CPrint.Warn("Flow Manager.cs : 싱글톤 중복 감지. 기존 인스턴스가 있으니 현재 오브젝트 제거");
            Destroy(gameObject);
            return;
        }

        //_instance = this;

        // SceneSystem은 파괴되지 않게
        if (_dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // 카탈로그 불러오기 실패
        if (_catalog == null)
        {
            CPrint.Error("Flow Manager.cs : Catalog is Null.");
            Destroy(gameObject);
            return;
        }

        _catalog.BuildMaps();

        SyncCursorToCurrentScene();
    }

    private void Start()
    {
        // UI관련 초기화는 Awake에서 안먹히는 경우가 간혹 있으니 Start에서 하기
        if (_transitionUI != null)
        {
            _transitionUI.Initialize();
        }
    }

    private void Update()
    {
        // Debug Mode
        if (!_enableHotKeys)
        {
            return;
        }

        if (_catalog == null)
        {
            return;
        }

        // 현재 로딩중이면 패스
        if (_isLoading)
        {
            return;
        }

        // 핫키
        HandleHotkeys();
    }

    private void SyncCursorToCurrentScene()
    {
        List<SceneEntry> entries = new List<SceneEntry>();

        if (entries == null || entries.Count == 0)
        {
            return;
        }

        // 현재 활성화된 씬이 무엇인지 이름 가져오기
        string currentName = SceneManager.GetActiveScene().name;


        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].SceneName == currentName)
            {
                CPrint.KV(entries[i].SceneName, currentName);
                _cursorIndex = i;
                return;
            }
        }

        _cursorIndex = 0;
        CPrint.Warn($"커서 싱크 실패 : {currentName}");
    }

    public void LoadScene(ESceneID ID)
    {
        if (_catalog.TryGetSceneName(ID, out string sceneName) == false)
        {
            CPrint.Warn($"ID : {ID} / Scene Load Fail.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            CPrint.Warn($"ID : {ID} / Scene Name is Empty");
            CPrint.KV(ID.ToString(), sceneName);
            return;
        }

        // 씬 전환
        StartCoroutine(Co_LoadSceneWithTransition(ID, sceneName));
    }

    private IEnumerator Co_LoadSceneWithTransition(ESceneID ID, string sceneName)
    {
        // 전환중일땐 브레이크
        if (_isLoading)
        {
            yield break;
        }

        _isLoading = true;

        CPrint.Log($"ID = {ID} / sceneName = {sceneName}");

        // 페이드 인
        if (_transitionUI != null)
        {
            _transitionUI.SetLoadingText("Loading...");

            yield return _transitionUI.Co_FadeTo(1f, _fadeDuration);
        }

        // 비동기 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // 로딩이 끝나도 바로 활성화 되지 않게 만들기
        op.allowSceneActivation = false;

        // 진행도가 0.9쯤일때
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // 다음 프레임에 변경 되기 때문에 true로 변경
        op.allowSceneActivation = true;

        // 한 프레임 확보
        yield return null;

        // 페이드 아웃
        if (_transitionUI != null)
        {
            yield return _transitionUI.Co_FadeTo(0f, _fadeDuration);
            _transitionUI.SetLoadingText("");
        }

        SyncCursorToCurrentScene();

        CPrint.Log($"씬 로드 : {sceneName}");

        _isLoading = false;
    }

    // 현재 씬 재로딩
    private void ReloadCurrent()
    {
        string current = SceneManager.GetActiveScene().name;

        if (_catalog.TryGetSceneID(current, out ESceneID ID))
        {
            CPrint.Log($"리로드 {current}");

            LoadScene(ID);
        }
    }

    // 다음 씬 로드
    private void LoadNext()
    {
        List<SceneEntry> entries = _catalog.GetEntries();

        if (entries == null || entries.Count == 0)
        {
            CPrint.Warn("카탈로그가 비어있습니다.");
            return;
        }

        _cursorIndex++;

        if (_cursorIndex >= entries.Count)
        {
            _cursorIndex = 0;
        }

        string nextSceneName = entries[_cursorIndex].SceneName;

        if (_catalog.TryGetSceneID(nextSceneName, out ESceneID ID) == false)
        {
            CPrint.Warn($"카탈로그 맵에 없습니다. {nextSceneName}");
            return;
        }

        CPrint.Log($"{_cursorIndex} / {nextSceneName}");

        LoadScene(ID);

    }

    // 이전 신 로드
    private void LoadPrev()
    {
        List<SceneEntry> entries = _catalog.GetEntries();

        if (entries == null || entries.Count == 0)
        {
            // 경고
            return;
        }

        _cursorIndex--;

        if (_cursorIndex < 0)
        {
            _cursorIndex = entries.Count - 1;
        }

        // 현재 커서의 이름 필요
        string prevSceneName = entries[_cursorIndex].SceneName;

        if (_catalog.TryGetSceneID(prevSceneName, out ESceneID ID) == false)
        {
            // 경고 → nextSceneName
            CPrint.Warn($"경고 카탈로그 맵에 없습니다. : {prevSceneName}");
            return;
        }

        CPrint.Log($"{_cursorIndex} / {prevSceneName}");

        LoadScene(ID);
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            LoadScene(ESceneID.Title);
            _curTempSceneIndex = 1;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            LoadScene(ESceneID.Game);
            _curTempSceneIndex = 2;
        }

        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadCurrent();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNext();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadPrev();
        }

        */
    }

    // 인스턴스 정리
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnGUI()
    {
        string temp1 = "타이틀 조작키 | 메뉴 이동 : ↑, ↓ ";
        string temp2 = "게임 조작키 | 이동및 시점 : WASD 와 Mouse / 무기 교체 : 1, 2, 3 / 사격 : Mouse0 / 재장전 : R / 점프 : Space";

        // temp
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleRight;
        style.fontSize = 50;

        GUI.Label(new Rect(0, 0, Screen.width, 120), " Key Z : 타이틀 화면 | Key X : 게임 화면", style);
        style.fontSize = 40;

        string finaltemp = "";

        if (_curTempSceneIndex == 1)
        {
            finaltemp = temp1;
        }
        else if (_curTempSceneIndex == 2)
        {
            finaltemp = temp2;
        }


        GUI.Label(new Rect(0, Screen.height - 120, Screen.width, 120), finaltemp, style);
    }
}
