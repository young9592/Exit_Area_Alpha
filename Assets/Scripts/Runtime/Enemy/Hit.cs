using UnityEngine;

public class Hit : MonoBehaviour
{
    #region Inspector
    [Header("Damage Inspector")]
    [SerializeField] private float _damage;
    [SerializeField] private string _targetLayerName = "Player"; 
    #endregion

    #region Field
    private Rigidbody _rb;
    #endregion

    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();   
    }
    public void Initialize(float damage)
    {
        _damage = damage;
    }
    private void OnTriggerEnter(Collider other)
    {
        int layerMask = LayerMask.GetMask(_targetLayerName);

        if (other.gameObject.layer == layerMask)
        {
            Player player = other.gameObject.GetComponent<Player>();
            player.TakeDamage(_damage);

            this.gameObject.SetActive(false);
        }
    }
}
