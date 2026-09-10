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
    [Header("Audio")]
    [SerializeField] private AudioSource _audio;

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

    [Header("Stage03_1")]
    [SerializeField] private List<GameObject> _areaLine03_1;
    [SerializeField] private List<GameObject> _backBlock03_1;
    [SerializeField] private List<Transform> _spawnPoints03_1;
    [SerializeField] private GameObject _enterTrigger03_1;

    [Header("Stage03_2")]
    [SerializeField] private List<GameObject> _areaLine03_2;
    [SerializeField] private List<GameObject> _backBlock03_2;
    [SerializeField] private List<Transform> _spawnPoints03_2;
    [SerializeField] private GameObject _enterTrigger03_2;

    [Header("Stage04_1")]
    [SerializeField] private List<GameObject> _areaLine04_1;
    [SerializeField] private List<GameObject> _backBlock04_1;
    [SerializeField] private List<Transform> _spawnPoints04_1;
    [SerializeField] private GameObject _enterTrigger04_1;

    [Header("Stage04_2")]
    [SerializeField] private List<GameObject> _areaLine04_2;
    [SerializeField] private List<GameObject> _backBlock04_2;
    [SerializeField] private List<Transform> _spawnPoints04_2;
    [SerializeField] private GameObject _enterTrigger04_2;

    [Header("Stage05_1")]
    [SerializeField] private List<GameObject> _areaLine05_1;
    [SerializeField] private List<GameObject> _backBlock05_1;
    [SerializeField] private List<Transform> _spawnPoints05_1;
    [SerializeField] private GameObject _enterTrigger05_1;
    [SerializeField] private GameObject _helpObject;

    [Header("현재 스테이지")]
    [SerializeField] private Area _curArea = Area.Area01;

    [Header("Layer Name")]
    [SerializeField] private string _layerName = "Interaction";
    #endregion

    #region Field
    private CTimer _areaDelayTimer = new CTimer();
    private int _layerMask;
    // 마지막 스테이지 웨이브 진행도
    private int _wave;

    private string _zombieAwakenSoundPath = "Sound/Enemy/Awaken";
    private string _noiseSoundPath = "Sound/Other/noiseloop";
    #endregion

    private void Reset()
    {
        _audio = GetComponent<AudioSource>();
    }
    private void Awake()
    {
        if(_audio == null)
        {
            _audio = GetComponent<AudioSource>();
        }

        _layerMask = LayerMask.NameToLayer(_layerName);

        TriggerArea triggerAreaScript = null;

        triggerAreaScript = _enterTrigger02_1.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger02_2.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger03_1.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger03_2.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger04_1.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger04_2.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;

        triggerAreaScript = _enterTrigger05_1.GetComponent<TriggerArea>();
        triggerAreaScript.OnAreaEnter += EnterArea;
    }

    private void Start()
    {
        #region NullCheck
        if (_audio == null)
        {
            enabled = false;
            return;
        }
        #endregion

        EnterArea((int)_curArea);
    }

    private void Update()
    {
        #region NullCheck
        if (_audio == null)
        {
            enabled = false;
            return;
        }
        #endregion

        if (_areaDelayTimer.GetCurrentTimerState)
        {
            _areaDelayTimer.AddTimer();
        }

        // 마지막 스테이지는 디펜스 형식이라 적이 모두 처치되었는지 체크 불필요.
        LastStageSpawn();

        // 모든 적 처지했나?
        if (Zombie.GetSpawnCount > 0)
        {
            return;
        }

        CheckStageClear();
    }
    // 마지막 스테이지 소환
    private void LastStageSpawn()
    {
        if (_enterTrigger05_1.activeSelf)
        {
            return;
        }

        // 타이머가 다 될 때 마다 스폰을 시키겠다.
        if (!_areaDelayTimer.GetCurrentTimerState)
        {
            int randSpotIndex = Random.Range(0, _spawnPoints05_1.Count);

            _walkerPool.SpawnZombie(_spawnPoints05_1[randSpotIndex].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);

            _areaDelayTimer.SetTimer(Random.Range(3f, 5f));
            _wave += 1;
        }
    }
    private void CheckStageClear()
    {
        switch (_curArea)
        {
            case Area.Area01:
                for (int i = 0; i < _areaLine01.Count; i++)
                {
                    // 스테이지 클리어 시 상호작용 가능하게 변경
                    _areaLine01[i].layer = _layerMask;
                }
                break;
            case Area.Area02_1:
                if (_enterTrigger02_1.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine02_1.Count; i++)
                {
                    _areaLine02_1[i].layer = _layerMask;
                }
                break;
            case Area.Area02_2:
                if (_enterTrigger02_2.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine02_2.Count; i++)
                {
                    _areaLine02_2[i].layer = _layerMask;
                }
                break;
            case Area.Area03_1:
                if (_enterTrigger03_1.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine03_1.Count; i++)
                {
                    _areaLine03_1[i].layer = _layerMask;
                }
                break;
            case Area.Area03_2:
                if (_enterTrigger03_2.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine03_2.Count; i++)
                {
                    _areaLine03_2[i].layer = _layerMask;
                }
                break;
            case Area.Area04_1:
                if (_enterTrigger04_1.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine04_1.Count; i++)
                {
                    _areaLine04_1[i].layer = _layerMask;
                }
                break;
            case Area.Area04_2:
                if (_enterTrigger04_2.activeSelf)
                {
                    return;
                }
                for (int i = 0; i < _areaLine04_2.Count; i++)
                {
                    _areaLine04_2[i].layer = _layerMask;
                }
                for (int i = 0; i < _backBlock04_2.Count - 1; i++)
                {
                    _backBlock04_2[i].SetActive(false);
                }
                break;
        }
    }

    public void EnterArea(int _areaName)
    {
        TriggerArea triggerAreaScript = null;
        _curArea = (Area)_areaName;

        // Stage01
        if (_curArea == Area.Area01)
        {
            for (int i = 0; i < _spawnPoints01.Count; i++)
            {
                _walkerPool.SpawnZombie(_spawnPoints01[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), false);
            }

        }
        // Stage02_1
        else if (_curArea == Area.Area02_1)
        {
            for (int i = 0; i < _spawnPoints02_1.Count - 2; i++)
            {
                _walkerPool.SpawnZombie(_spawnPoints02_1[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }
            for (int i = _spawnPoints02_1.Count - 2; i < _spawnPoints02_1.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints02_1[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }

            for (int i = 0; i < _backBlock02_1.Count; i++)
            {
                _backBlock02_1[i].SetActive(true);
            }
            triggerAreaScript = _enterTrigger02_1.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage02_2
        else if (_curArea == Area.Area02_2)
        {
            for (int i = 0; i < _spawnPoints02_2.Count - 1; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints02_2[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }
            for (int i = _spawnPoints02_2.Count - 1; i < _spawnPoints02_2.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints02_2[i].position, _spawnPoints02_2[i].rotation, true);
            }

            for (int i = 0; i < _backBlock02_2.Count; i++)
            {
                _backBlock02_2[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger02_2.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage03_1
        else if (_curArea == Area.Area03_1)
        {
            for (int i = 0; i < _spawnPoints03_1.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints03_1[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }

            for (int i = 0; i < _backBlock03_1.Count; i++)
            {
                _backBlock03_1[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger03_1.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage03_2
        else if (_curArea == Area.Area03_2)
        {
            for (int i = 0; i < _spawnPoints03_2.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints03_2[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }

            for (int i = 0; i < _backBlock03_2.Count; i++)
            {
                _backBlock03_2[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger03_2.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage04_1
        else if (_curArea == Area.Area04_1)
        {
            for (int i = 0; i < _spawnPoints04_1.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints04_1[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }

            for (int i = 0; i < _backBlock04_1.Count; i++)
            {
                _backBlock04_1[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger04_1.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage04_2
        else if (_curArea == Area.Area04_2)
        {
            for (int i = 0; i < _spawnPoints04_2.Count; i++)
            {
                _runnerPool.SpawnZombie(_spawnPoints04_2[i].position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), true);
            }

            for (int i = 0; i < _backBlock04_2.Count; i++)
            {
                _backBlock04_2[i].SetActive(true);
            }

            triggerAreaScript = _enterTrigger04_2.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_zombieAwakenSoundPath));
        }
        // Stage05_1
        else if (_curArea == Area.Area05_1)
        {
            // 디펜스 게임 시작

            for (int i = 0; i < _backBlock05_1.Count; i++)
            {
                _backBlock05_1[i].SetActive(true);
            }

            _helpObject.SetActive(true);
            triggerAreaScript = _enterTrigger05_1.GetComponent<TriggerArea>();
            triggerAreaScript.OnAreaEnter -= EnterArea;

            _audio.PlayOneShot(Resources.Load<AudioClip>(_noiseSoundPath));
        }


        _areaDelayTimer.SetTimer(3f);
    }
}
