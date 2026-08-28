using System;
using System.Collections.Generic;
using UnityEngine;
public enum ESceneID
{
    Title = 0,
    Game = 1
}

[Serializable]
public class SceneEntry
{
    public ESceneID ID;
    public string SceneName;
}

public class CSceneCatalog : MonoBehaviour
{
    #region Inspector
    [Header("씬 카탈로그")]
    [SerializeField] private List<SceneEntry> _scenes = new List<SceneEntry>();

    [Header("옵션")]
    [SerializeField] private bool _buildOnAwake = true;
    // 딕셔너리를 준비하지 않았다면 외부에서 딕셔너리가 필요합니다.
    #endregion

    #region Field
    // ID로 씬 이름을 빠르게 찾기
    private readonly Dictionary<ESceneID, string> _IDToName = new Dictionary<ESceneID, string>();
    // 로그 / 디버그 / 히스토리 추척할 때 이중 검증
    private readonly Dictionary<string, ESceneID> _nameToID = new Dictionary<string, ESceneID>();
    #endregion

    // 외부로 넘겨주되 수정 불가능하게
    public IReadOnlyList<SceneEntry> Entries => _scenes;

    private void Awake()
    {
        if (_buildOnAwake)
        {
            BuildMaps();
        }
    }

    // ContextMenu : 스크립트가 붙어 있는 컴포넌트의 메뉴에 항목을 추가
    [ContextMenu("BuildMaps (Rebuild Catalog)")]
    public void BuildMaps()
    {
        _IDToName.Clear();
        _nameToID.Clear();

        for(int i = 0; i < _scenes.Count; i++)
        {
            SceneEntry e = _scenes[i];

            if(e == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(e.SceneName))
            {
                CPrint.Warn("$SceneEntry가 비어있습니다. 확인이 필요합니다.");
                continue;
            }

            // 중복 체크
            // 같은 씬 ID가 이미 등록되어 있는 경우
            if (_IDToName.ContainsKey(e.ID))
            {
                CPrint.Warn($"ID 중복 {e.ID} / 기존 = {_IDToName[e.ID]} / 신규 = {e.SceneName}");
            }

            // 같은 씬 이름이 이미 등록되어 있는 경우
            if (_nameToID.ContainsKey(e.SceneName))
            {
                CPrint.Warn($"Name 중복 {e.SceneName} / 기존 = {_nameToID[e.SceneName]} / 신규 = {e.ID}");
                continue;
            }

            _IDToName.Add(e.ID, e.SceneName);
            _nameToID.Add(e.SceneName, e.ID);
        }

        CPrint.Log("[Scene Catalog Build]");
        CPrint.Log($"List Count {_scenes.Count}");
        CPrint.Log($"Map Count (ID → Name) {_IDToName.Count}");
        CPrint.Log($"Map Count (Name → ID) {_nameToID.Count}");
    }

    // id로 SceneName 가져오기
    public bool TryGetSceneName(ESceneID ID, out string sceneName)
    {
        return _IDToName.TryGetValue(ID, out sceneName);
    }

    // id로 SceneName 가져오기
    public string GetSceneName(ESceneID ID)
    {
        if (_IDToName.TryGetValue(ID, out string sceneName))
        {
            return sceneName;
        }

        return string.Empty;
    }

    // 현재 씬 이름만 있을 때 다시 열거형 ID로 역변환하는 용도
    public bool TryGetSceneID(string sceneName, out ESceneID ID)
    {
        return _nameToID.TryGetValue(sceneName, out ID);
    }

    // Readonly인 자료구조를 넘겨줌으로써 자료 손상을 방지합니다.
    public List<SceneEntry> GetEntries()
    {
        return _scenes;
    }
}
