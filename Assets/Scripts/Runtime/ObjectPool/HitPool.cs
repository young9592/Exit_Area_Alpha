
using UnityEngine;

public class HitPool : ObjectPool
{
    // Hit Effect
    public void SpawnHitEffect(Vector3 hitPos, float lifeTime = 1f)
    {
        GameObject HitPrefab = GetPrefabFromPool();

        Vector3 spawnPos = hitPos;
        Quaternion spawnRot = Quaternion.identity;
        Vector3 spawnScale = Vector3.one;

        HitPrefab.transform.position = spawnPos;
        HitPrefab.transform.rotation = spawnRot;
        HitPrefab.transform.localScale = spawnScale;

        HitPrefab.SetActive(true);

        if (!_alivePrefab.Contains(HitPrefab))
        {
            _alivePrefab.Add(HitPrefab);
        }
        else
        {
            CPrint.Warn($"중복 스폰 감지 : {HitPrefab.name}");
        }

        _lifeMap[HitPrefab] = lifeTime;
    }
}
