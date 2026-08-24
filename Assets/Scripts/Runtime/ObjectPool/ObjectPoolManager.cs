using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public enum ForceDirection : byte
    {
        Forward,
        Up
    }

    #region Inspector   
    [Header("Prefab")]
    [SerializeField] private GameObject _prefab = null;

    [Header("SpawnPoint")]
    [SerializeField] private Transform _spawnPoint = null;

    [Header("Object Pool")]
    [Min(0)]
    [SerializeField] private int _prewarmCount = 20;
    #endregion

    #region Field
    private readonly List<GameObject> _alivePrefab = new List<GameObject>();
    private readonly Dictionary<GameObject, float> _lifeMap = new Dictionary<GameObject, float>();
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private readonly HashSet<GameObject> _pooledPrefab = new HashSet<GameObject>();
    private Transform _poolRoot;
    #endregion

    private void Start()
    {
        if (_prefab == null)
        {
            CPrint.Error("Prefab is Null.");
            enabled = false;
            return;
        }

        CreatePoolRoot();
        Prewarm();
    }


    private void Update()
    {
        UpdateAlivePrefab();
    }

    // 프리펩 저장고
    private void CreatePoolRoot()
    {
        if (_poolRoot != null)
        {
            return;
        }

        GameObject root = new GameObject("Pool_Root");
        _poolRoot = root.transform;
    }

    // 미리 준비하기
    private void Prewarm()
    {
        for (int i = 0; i < _prewarmCount; i++)
        {
            GameObject bullet = Instantiate(_prefab, _poolRoot);
            bullet.SetActive(false);
            _pool.Enqueue(bullet);
            _pooledPrefab.Add(bullet);
        }

        CPrint.Log($"초기 총알 준비 : {_prewarmCount}");
    }

    // 사용 완료된 오브젝트 회수
    private void ReturnToPool(GameObject bullet)
    {
        if (bullet == null)
        {
            return;
        }



        bullet.SetActive(false);
        bullet.transform.SetParent(_poolRoot);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }

        if (!_pooledPrefab.Contains(bullet))
        {
            _pool.Enqueue(bullet);
            _pooledPrefab.Add(bullet);
        }

    }
    // 모든 오브젝트 회수
    private void ReturnAll()
    {
        for (int i = _alivePrefab.Count - 1; i >= 0; i++)
        {
            GameObject bullet = _alivePrefab[i];

            // 혹여나 피치못할 사정으로 null상태라면 컨티뉴
            if (bullet == null)
            {
                continue;
            }

            ReturnToPool(bullet);
        }

        // 수명 장부 및 활성 객체 리스트 초기화
        _alivePrefab.Clear();
        _lifeMap.Clear();

        CPrint.Warn($"전체 프리펩 회수 / Pool = {_pool.Count}");
    }

    // 등록된 수명 정보를 제거
    private void RemoveLifeIfExists(GameObject bullet)
    {
        if (bullet == null)
        {
            return;
        }

        // 딕셔너리 수명장부 등록되어 있으면 지우기
        if (_lifeMap.ContainsKey(bullet))
        {
            _lifeMap.Remove(bullet);
        }
    }

    // 활성화된 오브젝트 관리하기
    private void UpdateAlivePrefab()
    {
        for (int i = _alivePrefab.Count - 1; i >= 0; i--)
        {
            GameObject bullet = _alivePrefab[i];

            // 주로 외부에 의해서 오브젝트가 파괴 혹은 미싱 상태면 제거
            if (bullet == null)
            {
                _alivePrefab.RemoveAt(i);
                continue;
            }

            // 오브젝트가 꺼진 상태이면 회수조치
            if (!bullet.activeSelf)
            {
                ReturnToPool(bullet);
                _alivePrefab.RemoveAt(i);
                RemoveLifeIfExists(bullet);

                continue;
            }

            // 수명 장부에 없는지 체크
            if (!_lifeMap.ContainsKey(bullet))
            {
                CPrint.Warn($"수명 장부 없음 : {bullet.name}");

                ReturnToPool(bullet);

                _alivePrefab.RemoveAt(i);

                continue;
            }

            _lifeMap[bullet] -= Time.deltaTime;

            if (_lifeMap[bullet] <= 0.0f)
            {
                ReturnToPool(bullet);
                _alivePrefab.RemoveAt(i);
                _lifeMap.Remove(bullet);
            }
        }
    }

    // 풀에 등록된 오브젝트 꺼내기
    // 없으면 추가 생성
    private GameObject GetPrefabFromPool()
    {
        if (_pool.Count > 0)
        {
            GameObject bullet = _pool.Dequeue();
            _pooledPrefab.Remove(bullet);

            return bullet;
        }

        CPrint.Warn("오브젝트 부족하여 추가 생성합니다.");
        GameObject extra = Instantiate(_prefab);
        extra.transform.SetParent(_poolRoot);
        _pool.Enqueue(extra);
        _pooledPrefab.Add(extra);
        return extra;
    }

    // 총알 물리 적용
    private void ApplyFire(GameObject bullet, float pushForce, ForceDirection direction = ForceDirection.Forward)
    {
        if (bullet == null)
        {
            return;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // 리지드 바디 가져오기 실패
        if (rb == null)
        {
            return;
        }

        // 물리상태 초기화 및 깨우기
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        // 물리 적용 구역
        Vector3 dir = Vector3.zero;

        switch (direction)
        {
            case ForceDirection.Forward:
                dir = bullet.transform.forward;
                break;
            case ForceDirection.Up:
                dir = bullet.transform.up;
                break;
        }


        Vector3 force = dir * pushForce;

        // 힘 적용
        rb.AddForce(force, ForceMode.Force);
    }

    // 총알 발사 위치 조정
    public void SpawnBullet(float curRecoil = 0f, float scale = 1f, float pushForce = 0f, float lifeTime = 8f, Transform where = null, Vector3 offset = default, ForceDirection direction = ForceDirection.Forward)
    {
        // 풀에서 가져오기
        GameObject bullet = GetPrefabFromPool();

        // 위치와 회전
        Vector3 basePos = where != null ? where.position : _spawnPoint.position;
        Quaternion baseRot = where != null ? where.rotation : _spawnPoint.rotation;
        Vector3 baseScale = Vector3.one;

        // 오프셋 필요하면 넣을수 있게..
        Vector3 spawnPos = basePos + offset;
        Quaternion spawnRot = baseRot * Quaternion.Euler(Random.Range(-curRecoil, curRecoil), Random.Range(-curRecoil, curRecoil), 0);
        Vector3 spawnScale = baseScale * scale;

        // 실제 pose 적용
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = spawnRot;
        bullet.transform.localScale = spawnScale;

        bullet.SetActive(true);

        // 총알 물리 적용
        ApplyFire(bullet, pushForce, direction);

        // 리스트에 등록 만약 중복 등록되는 상황이 생기면 중복 방어
        if (!_alivePrefab.Contains(bullet))
        {
            _alivePrefab.Add(bullet);
        }
        else
        {
            CPrint.Warn($"중복 스폰 감지 : {bullet.name}");
        }

        // 생명 장부 등록
        _lifeMap[bullet] = lifeTime;
    }
}
