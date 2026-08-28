using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
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
    // 풀 저장고 
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    // 중복체크
    private readonly HashSet<GameObject> _pooledPrefab = new HashSet<GameObject>();
    // 현재 프리펩 생명주기
    private readonly Dictionary<GameObject, float> _lifeMap = new Dictionary<GameObject, float>();
    // 활성화된 프리펩
    private readonly List<GameObject> _alivePrefab = new List<GameObject>();
    // 프리펩 하이어라키 정리
    private Transform _poolRoot;
    #endregion

    private void Start()
    {
        if (_prefab == null)
        {
            CPrint.Error("ObjectPool.cs Prefab is Null.");
            enabled = false;
            return;
        }

        if(_spawnPoint == null)
        {

        }

        CreatePoolRoot();
        Prewarm();
    }


    private void Update()
    {
        UpdateAlivePrefab();
    }

    // 프리펩 하이어라키 저장고
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
            GameObject userPrefab = Instantiate(_prefab, _poolRoot);
            userPrefab.SetActive(false);
            _pool.Enqueue(userPrefab);
            _pooledPrefab.Add(userPrefab);
        }

        CPrint.Log($"초기 프리펩 준비 : {_prewarmCount}");
    }

    // 사용 완료된 오브젝트 회수
    private void ReturnToPool(GameObject userPrefab)
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
    private void ReturnAll()
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
    private void RemoveLifeIfExists(GameObject userPrefab)
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
    private void UpdateAlivePrefab()
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
    private GameObject GetPrefabFromPool()
    {
        if (_pool.Count > 0)
        {
            GameObject userPrefab = _pool.Dequeue();
            _pooledPrefab.Remove(userPrefab);

            return userPrefab;
        }

        
        // 일단 추가 
        CPrint.Warn("오브젝트 부족하여 추가 생성합니다.");
        GameObject extra = Instantiate(_prefab);
        /*
        extra.transform.SetParent(_poolRoot);
        _pool.Enqueue(extra);
        _pooledPrefab.Add(extra);
        */
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

    // 히트 스캔 방식
    public void SpawnBullet(float damage = 0f, float curRecoil = 0f, float lifeTime = 0.1f, Transform where = null, float scale = 1f, Vector3 offset = default)
    {
        // 풀에서 가져오기
        GameObject bullet = GetPrefabFromPool();
        
        Bullet bulletCS = bullet.GetComponent<Bullet>();

        bulletCS.Init(damage);

        // 위치와 회전
        Vector3 basePos = where != null ? where.position : _spawnPoint.position;
        Quaternion baseRot = where != null ? where.rotation : _spawnPoint.rotation;
        // 크기는 일단 보류 히트 스캔 방식이라 레이져를 쏠 예정
        Vector3 baseScale = Vector3.one;

        // 오프셋 필요하면 넣을수 있게..
        Vector3 spawnPos = basePos + offset;
        // 총기의 반동 구현
        Quaternion spawnRot = baseRot * Quaternion.Euler(Random.Range(-curRecoil, curRecoil), Random.Range(-curRecoil, curRecoil), 0);
        Vector3 spawnScale = baseScale * scale;

        // 실제 pose 적용
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = spawnRot;
        bullet.transform.localScale = spawnScale;

        bullet.SetActive(true);

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

    // 총알 발사 위치 조정
    // 수정 필요사항 : 현재 히트 스캔 방식 필요.
    public void SpawnBulletRb(float curRecoil = 0f, float scale = 1f, float pushForce = 0f, float lifeTime = 8f, Transform where = null, Vector3 offset = default, ForceDirection direction = ForceDirection.Forward)
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

    // 좀비 생성 함수 구현 예정
    public void SpawnEnemy()
    {

    }
}
