using UnityEngine;

public class BulletPool : ObjectPool
{
    public enum ForceDirection : byte
    {
        Forward,
        Up
    }

    [Header("Bullet Hit Pool[총알 전용]")]
    [SerializeField] private HitPool _hitPoolManager;
    [SerializeField] private string _setScriptName = "Bullet";


    protected override void Prewarm()
    {
        for (int i = 0; i < _prewarmCount; i++)
        {
            GameObject userPrefab = Instantiate(_prefab, _poolRoot);
            userPrefab.SetActive(false);
            _pool.Enqueue(userPrefab);
            _pooledPrefab.Add(userPrefab);

            // 만약 총알이면 hitPool 초기화 등록
            if (_hitPoolManager == null)
            {
                CPrint.Error("HitPoolManager Connect Fail.");
                enabled = false;
                return;
            }

            if(_setScriptName == "Bullet")
            {
                Bullet bulletScript = userPrefab.GetComponent<Bullet>();
                bulletScript.Initialize(_hitPoolManager);
            }
            else if(_setScriptName == "Spit")
            {
                Spit spitScript = userPrefab.GetComponent<Spit>();
                spitScript.Initialize(_hitPoolManager);
            }

        }

        CPrint.Log($"BulletPool.cs 초기 프리펩 준비 : {_prewarmCount}");
    }

    // 히트 스캔 방식
    public void SpawnBullet(float damage = 0f, float curRecoil = 0f, float lifeTime = 0.1f, Transform where = null, float scale = 1f, Vector3 offset = default)
    {
        // 풀에서 가져오기
        GameObject bulletPrefab = GetPrefabFromPool();

        if(_setScriptName == "Bullet")
        {
            Bullet bulletScript = bulletPrefab.GetComponent<Bullet>();
            bulletScript.SetBullet(damage);
        }
        else if(_setScriptName == "Spit")
        {
            Spit spitScript = bulletPrefab.GetComponent<Spit>();
            spitScript.SetBullet(damage);
        }



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
        bulletPrefab.transform.position = spawnPos;
        bulletPrefab.transform.rotation = spawnRot;
        bulletPrefab.transform.localScale = spawnScale;

        bulletPrefab.SetActive(true);

        // 리스트에 등록 만약 중복 등록되는 상황이 생기면 중복 방어
        if (!_alivePrefab.Contains(bulletPrefab))
        {
            _alivePrefab.Add(bulletPrefab);
        }
        else
        {
            CPrint.Warn($"중복 스폰 감지 : {bulletPrefab.name}");
        }

        // 생명 장부 등록
        _lifeMap[bulletPrefab] = lifeTime;
    }

    // 총알 발사 위치 조정
    // 수정 필요사항 : 현재 히트 스캔 방식 필요.
    public void SpawnBulletRb(float damage = 0f, float curRecoil = 0f, float scale = 1f, float pushForce = 0f, float lifeTime = 8f, Transform where = null, Vector3 offset = default, ForceDirection direction = ForceDirection.Forward)
    {
        // 풀에서 가져오기
        GameObject bulletPrefab = GetPrefabFromPool();

        if (_setScriptName == "Bullet")
        {
            Bullet bulletScript = bulletPrefab.GetComponent<Bullet>();
            bulletScript.SetBullet(damage);
        }
        else if (_setScriptName == "Spit")
        {
            Spit spitScript = bulletPrefab.GetComponent<Spit>();
            spitScript.SetBullet(damage);
        }


        // 위치와 회전
        Vector3 basePos = where != null ? where.position : _spawnPoint.position;
        Quaternion baseRot = where != null ? where.rotation : _spawnPoint.rotation;
        Vector3 baseScale = Vector3.one;

        // 오프셋 필요하면 넣을수 있게..
        Vector3 spawnPos = basePos + offset;
        Quaternion spawnRot = baseRot * Quaternion.Euler(Random.Range(-curRecoil, curRecoil), Random.Range(-curRecoil, curRecoil), 0);
        Vector3 spawnScale = baseScale * scale;

        // 실제 pose 적용
        bulletPrefab.transform.position = spawnPos;
        bulletPrefab.transform.rotation = spawnRot;
        bulletPrefab.transform.localScale = spawnScale;

        bulletPrefab.SetActive(true);

        // 총알 물리 적용
        ApplyFire(bulletPrefab, pushForce, direction);

        // 리스트에 등록 만약 중복 등록되는 상황이 생기면 중복 방어
        if (!_alivePrefab.Contains(bulletPrefab))
        {
            _alivePrefab.Add(bulletPrefab);
        }
        else
        {
            CPrint.Warn($"중복 스폰 감지 : {bulletPrefab.name}");
        }

        // 생명 장부 등록
        _lifeMap[bulletPrefab] = lifeTime;
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
}
