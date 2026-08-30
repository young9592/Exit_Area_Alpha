using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{


    #region Inspector
    [Header("Prefab")]
    [SerializeField] protected GameObject _prefab = null;

    [Header("SpawnPoint")]
    [SerializeField] protected Transform _spawnPoint = null;

    [Header("Object Pool")]
    [Min(0)]
    [SerializeField] protected int _prewarmCount = 20;


    #endregion

    #region Field
    // 풀 저장고 
    protected readonly Queue<GameObject> _pool = new Queue<GameObject>();
    // 중복체크
    protected readonly HashSet<GameObject> _pooledPrefab = new HashSet<GameObject>();
    // 현재 프리펩 생명주기
    protected readonly Dictionary<GameObject, float> _lifeMap = new Dictionary<GameObject, float>();
    // 활성화된 프리펩
    protected readonly List<GameObject> _alivePrefab = new List<GameObject>();
    // 프리펩 하이어라키 정리
    protected Transform _poolRoot;
    #endregion

    protected void Start()
    {
        #region Null Check
        if (_prefab == null)
        {
            CPrint.Error("ObjectPool.cs Prefab is Null.");
            enabled = false;
            return;
        }
        #endregion

        CreatePoolRoot();
        Prewarm();
    }


    protected void Update()
    {
        UpdateAlivePrefab();
    }

    // 프리펩 하이어라키 저장고
    protected void CreatePoolRoot()
    {
        if (_poolRoot != null)
        {
            return;
        }

        GameObject root = new GameObject("Pool_Root");
        _poolRoot = root.transform;
    }

    // 미리 준비하기
    protected virtual void Prewarm()
    {
        for (int i = 0; i < _prewarmCount; i++)
        {
            GameObject userPrefab = Instantiate(_prefab, _poolRoot);
            userPrefab.SetActive(false);
            _pool.Enqueue(userPrefab);
            _pooledPrefab.Add(userPrefab);
        }

        CPrint.Log($"초기 프리펩 준비 : {_prewarmCount}");
    }

    // 사용 완료된 오브젝트 회수
    protected void ReturnToPool(GameObject userPrefab)
    {
        if (userPrefab == null)
        {
            return;
        }

        userPrefab.SetActive(false);
        userPrefab.transform.SetParent(_poolRoot);

        Rigidbody rb = userPrefab.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }

        // 중복 체크
        if (!_pooledPrefab.Contains(userPrefab))
        {
            _pool.Enqueue(userPrefab);
            _pooledPrefab.Add(userPrefab);
        }

    }
    // 모든 오브젝트 회수
    protected void ReturnAll()
    {
        for (int i = _alivePrefab.Count - 1; i >= 0; i++)
        {
            GameObject userPrefab = _alivePrefab[i];

            // 혹여나 피치못할 사정으로 null상태라면 컨티뉴
            if (userPrefab == null)
            {
                continue;
            }

            ReturnToPool(userPrefab);
        }

        // 수명 장부 및 활성 객체 리스트 초기화
        _alivePrefab.Clear();
        _lifeMap.Clear();

        CPrint.Warn($"전체 프리펩 회수 / Pool = {_pool.Count}");
    }

    // 등록된 수명 정보를 제거
    protected void RemoveLifeIfExists(GameObject userPrefab)
    {
        if (userPrefab == null)
        {
            return;
        }

        // 딕셔너리 수명장부 등록되어 있으면 지우기
        if (_lifeMap.ContainsKey(userPrefab))
        {
            _lifeMap.Remove(userPrefab);
        }
    }

    // 활성화된 오브젝트 관리하기
    protected void UpdateAlivePrefab()
    {
        for (int i = _alivePrefab.Count - 1; i >= 0; i--)
        {
            GameObject userPrefab = _alivePrefab[i];

            // 주로 외부에 의해서 오브젝트가 파괴 혹은 미싱 상태면 제거
            if (userPrefab == null)
            {
                _alivePrefab.RemoveAt(i);
                continue;
            }

            // 오브젝트가 꺼진 상태이면 회수조치
            if (!userPrefab.activeSelf)
            {
                ReturnToPool(userPrefab);
                _alivePrefab.RemoveAt(i);
                RemoveLifeIfExists(userPrefab);

                continue;
            }

            // 수명 장부에 없는지 체크
            if (!_lifeMap.ContainsKey(userPrefab))
            {
                CPrint.Warn($"수명 장부 없음 : {userPrefab.name}");

                ReturnToPool(userPrefab);

                _alivePrefab.RemoveAt(i);

                continue;
            }

            _lifeMap[userPrefab] -= Time.deltaTime;

            if (_lifeMap[userPrefab] <= 0.0f)
            {
                ReturnToPool(userPrefab);
                _alivePrefab.RemoveAt(i);
                _lifeMap.Remove(userPrefab);
            }
        }
    }

    // 풀에 등록된 오브젝트 꺼내기
    // 없으면 추가 생성
    protected GameObject GetPrefabFromPool()
    {
        if (_pool.Count > 0)
        {
            GameObject userPrefab = _pool.Dequeue();
            _pooledPrefab.Remove(userPrefab);

            return userPrefab;
        }

        // 일단 추가만..
        // 초기 생성을 적절히 잘 잡아서 추가 생성분은 최대한 안생기게 유도
        CPrint.Warn("오브젝트 부족하여 추가 생성합니다.");
        GameObject extra = Instantiate(_prefab);
        /*
        extra.transform.SetParent(_poolRoot);
        _pool.Enqueue(extra);
        _pooledPrefab.Add(extra);
        */
        return extra;
    }
}
