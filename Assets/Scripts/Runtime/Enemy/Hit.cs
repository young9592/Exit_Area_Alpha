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
    private int _layerMask;
    #endregion

    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void Awake()
    {
        _layerMask = LayerMask.NameToLayer(_targetLayerName);
    }
    public void Initialize(float damage)
    {
        _damage = damage;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer.Equals(_layerMask))
        {
            Player player = other.gameObject.GetComponent<Player>();
            player.TakeDamage(_damage);

            this.gameObject.SetActive(false);
        }
    }
}
