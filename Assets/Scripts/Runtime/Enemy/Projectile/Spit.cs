using UnityEngine;

public class Spit : MonoBehaviour
{
    #region Inspector
    [Header("필수참조")]
    [SerializeField] private HitPool _hitPoolManager;

    [Header("TargetLayer")]
    [SerializeField] private string _hitName = "Player";
    [SerializeField] private string _blockName = "Block";
    #endregion

    #region Field
    private float _damage;
    private int _hitLayerMask;
    private int _blockLayerMask;
    #endregion

    private void Awake()
    {
        _hitLayerMask = LayerMask.NameToLayer(_hitName);
        _blockLayerMask = LayerMask.NameToLayer(_blockName);
    }

    // 첫 초기화
    public void Initialize(HitPool hitPoolManager)
    {
        _hitPoolManager = hitPoolManager;

        if (_hitPoolManager == null)
        {
            CPrint.Warn("HitPoolManager Connect Fail.");
            enabled = false;
            return;
        }
    }
    public void SetBullet(float damage)
    {
        _damage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_hitPoolManager == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if(collision == null)
        {
            return;
        }

        // 벽에 부딪힘
        if (collision.gameObject.layer.Equals(_blockLayerMask))
        {
            gameObject.SetActive(false);
            return;
        }

        // 플레이어에게 부딪힘
        if (collision.gameObject.layer.Equals(_hitLayerMask))
        {
            // 플레이어에게 데미지
            Player playerScript = collision.gameObject.GetComponent<Player>();
            playerScript.TakeDamage(_damage);
            // 피격 이펙트
            _hitPoolManager.SpawnHitEffect(collision.contacts[0].point);
            // 비활성화
            gameObject.SetActive(false);
        }
    }
}
