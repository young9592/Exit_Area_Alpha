using UnityEngine;

public class ZombiePool : ObjectPool
{
    [Header("Use BulletPool")]
    [SerializeField] private bool _useBulletPool = false;
    [SerializeField] private BulletPool _bulletPool;

    private void Awake()
    {
        _lifeMapUse = false;
    }

    public void SpawnZombie(Vector3 enemySpawnPos, Quaternion enemySpawnViewDir, bool isAlert)
    {
        GameObject enemyPrefab = GetPrefabFromPool();

        if (_useBulletPool)
        {
            Spitter spitterScript = enemyPrefab.GetComponent<Spitter>();
            spitterScript.Initialize(_bulletPool);
        }

        Vector3 spawnPos = enemySpawnPos;
        Quaternion spawnRot = enemySpawnViewDir;
        Vector3 spawnScale = Vector3.one;

        enemyPrefab.transform.position = spawnPos;
        enemyPrefab.transform.rotation = spawnRot;
        enemyPrefab.transform.localScale = spawnScale;

        if (isAlert)
        {
            Zombie.OnAlertMode();
        }

        enemyPrefab.SetActive(true);

        if (!_alivePrefab.Contains(enemyPrefab))
        {
            _alivePrefab.Add(enemyPrefab);
        }
        else
        {
            CPrint.Warn($"중복 스폰 감지 : {enemyPrefab.name}");
        }

        _lifeMap[enemyPrefab] = 0;
    }
}
