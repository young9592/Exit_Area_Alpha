using UnityEngine;

public class ZombiePool : ObjectPool
{
    private void Awake()
    {
        _lifeMapUse = false;
    }

    public void SpawnZombie(Vector3 enemySpawnPos, Quaternion enemySpawnViewDir)
    {
        GameObject enemyPrefab = GetPrefabFromPool();

        Vector3 spawnPos = enemySpawnPos;
        Quaternion spawnRot = enemySpawnViewDir;
        Vector3 spawnScale = Vector3.one;

        enemyPrefab.transform.position = spawnPos;
        enemyPrefab.transform.rotation = spawnRot;
        enemyPrefab.transform.localScale = spawnScale;

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
