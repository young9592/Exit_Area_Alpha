using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Area
    {
        Area01,
        Area02_1,
        Area02_2,
        Area03_1,
        Area03_2,
        Area04_1,
        Area04_2,
        Area05_1
    }

    #region Inspector
    [Header("Object Pool")]
    [SerializeField] private ZombiePool _walkerPool;
    [SerializeField] private ZombiePool _runnerPool;

    [Header("Stage01")]
    [SerializeField] private List<GameObject> _areaLine01;
    [SerializeField] private List<GameObject> _backBlock01;
    [SerializeField] private List<Transform> _spawnPoints01;

    [Header("Stage02_1")]
    [SerializeField] private List<GameObject> _areaLine02_1;
    [SerializeField] private List<GameObject> _backBlock02_1;
    [SerializeField] private List<Transform> _spawnPoints02_1;
    [SerializeField] private GameObject _enterTrigger02_1;

    [Header("Stage02_2")]
    [SerializeField] private List<GameObject> _areaLine02_2;
    [SerializeField] private List<GameObject> _backBlock02_2;
    [SerializeField] private List<Transform> _spawnPoints02_2;
    [SerializeField] private GameObject _enterTrigger02_2;

    [Header("현재 스테이지")]
    [SerializeField] private Area _curArea = Area.Area01;

    [Header("Layer Name")]
    [SerializeField] private string _layerName = "Interaction";
    #endregion

    #region Field
    private CTimer _areaDelayTimer = new CTimer();
    private int layerMask;
    #endregion

    private void Awake()
    {
        layerMask = LayerMask.NameToLayer(_layerName);

        TriggerArea triggerAreaScript = null;

        triggerAreaScript = _enterTrigger02_1.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger02_2.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;
    }

    private void Start()
    {
        EnterArea((int)_curArea);
    }

    private void Update()
    {
        if (_areaDelayTimer.GetCurrentTimerState)
        {
            _areaDelayTimer.AddTimer();
        }

        // 모든 적 처지했나?
        if (Zombie.GetSpawnCount > 0)
        {
            return;
        }

        CheckStageClear();
    }

    private void CheckStageClear()
    {
        switch (_curArea)
        {
            case Area.Area01:
                for (int i = 0; i < _areaLine01.Count; i++)
                {
                    // 스테이지 클리어 시 상호작용 가능하게 변경
                    _areaLine01[i].layer = layerMask;
                }
                break;
            case Area.Area02_1:
                if (_enterTrigger02_1.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine02_1.Count; i++)
                {
                    _areaLine02_1[i].layer = layerMask;
                }
                break;
            case Area.Area02_2:
                if (_enterTrigger02_2.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine02_2.Count; i++)
                {
                    _areaLine02_2[i].layer = layerMask;
                }
                break;
        }
    }

    public void EnterArea(int _areaName)
    {
        TriggerArea triggerAreaScript = null;
        _curArea = (Area)_areaName;

        if (_curArea == Area.Area01)
        {
            for (int i = 0; i < _spawnPoints01.Count; i++)
            {
                _walkerPool.SpawnZombie(_spawnPoints01[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            }
        }
        else if (_curArea == Area.Area02_1)
        {
            for (int i = 0; i < _spawnPoints02_1.Count; i++)
            {
                _walkerPool.SpawnZombie(_spawnPoints02_1[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            }

            for (int i = 0; i < _backBlock02_1.Count; i++)
            {
                _backBlock02_1[i].SetActive(true);
            }
            triggerAreaScript = _enterTrigger02_1.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;
        }
        else if (_curArea == Area.Area02_2)
        {
            for (int i = 0; i < _spawnPoints02_2.Count; i++)
            {
                _walkerPool.SpawnZombie(_spawnPoints02_2[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            }

            for (int i = 0; i < _backBlock02_2.Count; i++)
            {
                _backBlock02_2[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger02_2.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;
        }

        _areaDelayTimer.SetTimer(3f);
    }
}
