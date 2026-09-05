using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Area
    {
        Enter,
        Area01,
        Area02,
        Area03,
        Area04,
        End
    }

    #region Inspector
    [Header("Object Pool")]
    [SerializeField] private ZombiePool _walkerPool;
    [SerializeField] private ZombiePool _runnerPool;

    [Header("AreaLine")]
    [SerializeField] private GameObject _areaLine01_1;
    [SerializeField] private GameObject _portal01;
    [SerializeField] private List<Transform> _spawnPoints01;

    [SerializeField] private GameObject _areaLine02_1;
    [SerializeField] private GameObject _areaLine02_2;
    [SerializeField] private GameObject _portal02;
    [SerializeField] private List<Transform> _spawnPoints02;

    [SerializeField] private GameObject _areaLine03_1;
    [SerializeField] private GameObject _areaLine03_2;
    [SerializeField] private GameObject _portal03;
    [SerializeField] private List<Transform> _spawnPoints03;

    [SerializeField] private GameObject _areaLine04_1;
    [SerializeField] private GameObject _areaLine04_2;
    [SerializeField] private GameObject _portal04;
    [SerializeField] private List<Transform> _spawnPoints04;

    [Header("현재 스테이지")]
    [SerializeField] private Area _curArea = Area.Enter;
    #endregion

    #region Field
    // 스테이지 변경 시 적들이 0이여도 다음 스테이지가 되지 않게
    CTimer _areaChangeTimer = new CTimer();
    #endregion

    private void Awake()
    {
        Portal portalScript = _portal01.GetComponent<Portal>();
        portalScript.OnNextStage += NextStage;

        portalScript = _portal02.GetComponent<Portal>();
        portalScript.OnNextStage += NextStage;

        portalScript = _portal03.GetComponent<Portal>();
        portalScript.OnNextStage += NextStage;

        portalScript = _portal04.GetComponent<Portal>();
        portalScript.OnNextStage += NextStage;
    }

    private void Update()
    {
        if (_areaChangeTimer.GetCurrentTimerState)
        {
            _areaChangeTimer.AddTimer();
        }

        // 모든 적 처지했나?
        if (Zombie.GetSpawnCount > 0)
        {
            return;
        }

        switch (_curArea)
        {
            case Area.Enter:
                NextStage();
                break;
            case Area.Area01:
                _portal01.SetActive(true);
                break;
            case Area.Area02:
                _portal02.SetActive(true);
                break;
            case Area.Area03:
                _portal03.SetActive(true);
                break;
            case Area.Area04:
                _portal04.SetActive(true);
                break;
            case Area.End:
                break;
        }
    }

    private void NextStage()
    {
        Portal portalScript = null;

        switch (_curArea)
        {
            case Area.Area02:
                portalScript = _portal01.GetComponent<Portal>();
                portalScript.OnNextStage -= NextStage;
                break;
            case Area.Area03:
                portalScript = _portal02.GetComponent<Portal>();
                portalScript.OnNextStage -= NextStage;
                break;
            case Area.Area04:
                portalScript = _portal03.GetComponent<Portal>();
                portalScript.OnNextStage -= NextStage;
                break;
            case Area.End:
                portalScript = _portal04.GetComponent<Portal>();
                portalScript.OnNextStage -= NextStage;
                break;
            default:
                break;
        }

        _curArea++;

        switch (_curArea)
        {
            case Area.Area01:
                for (int i = 0; i < _spawnPoints01.Count; i++)
                {
                    _walkerPool.SpawnZombie(_spawnPoints01[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
                break;
            case Area.Area02:
                _areaLine01_1.SetActive(false);
                _areaLine02_1.SetActive(true);
                _areaLine02_2.SetActive(true);

                for (int i = 0; i < _spawnPoints02.Count - 2; i++)
                {
                    _walkerPool.SpawnZombie(_spawnPoints02[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
                for (int i = _spawnPoints02.Count - 2; i < _spawnPoints02.Count; i++)
                {
                    _runnerPool.SpawnZombie(_spawnPoints02[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
                break;
            case Area.Area03:
                _areaLine02_1.SetActive(false);
                _areaLine02_2.SetActive(false);
                _areaLine03_1.SetActive(true);
                _areaLine03_2.SetActive(true);

                for (int i = 0; i < _spawnPoints03.Count - 4; i++)
                {
                    _walkerPool.SpawnZombie(_spawnPoints03[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
                for (int i = _spawnPoints03.Count - 4; i < _spawnPoints03.Count; i++)
                {
                    _runnerPool.SpawnZombie(_spawnPoints03[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                }
                break;
            case Area.Area04:
                _areaLine03_1.SetActive(false);
                _areaLine03_2.SetActive(false);
                _areaLine04_1.SetActive(true);
                _areaLine04_2.SetActive(true);

                for (int i = 0; i < _spawnPoints04.Count; i += 2)
                {
                    _walkerPool.SpawnZombie(_spawnPoints04[i].position, _spawnPoints04[i].rotation);
                }
                for (int i = 1; i < _spawnPoints04.Count; i += 2)
                {
                    _runnerPool.SpawnZombie(_spawnPoints04[i].position, _spawnPoints04[i].rotation);
                }
                break;
            case Area.End:
                _areaLine04_1.SetActive(false);
                CPrint.Log("스테이지 완료! 다음 스테이지로..[씬 매니저 적용해야합니다]");
                break;
        }

        _areaChangeTimer.SetTimer(3f);
    }

}
