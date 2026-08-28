using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Inspector
    [Header("Hit Effect")]
    [SerializeField] private GameObject _hitGO;

    [Header("Bullet Status")]
    [SerializeField] private float _damage;
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private float _headMultiply = 2f;
    [SerializeField] private float _bodyMultiply = 1f;
    [SerializeField] private float _armMultiply = 0.5f;
    [SerializeField] private float _legMultiply = 0.8f;

    [Header("Layer")]
    [SerializeField] private string _layerMaskName = "Enemy";

    [Header("Tag")]
    [SerializeField] private string _tagHead = "Hit_Head";
    [SerializeField] private string _tagBody = "Hit_Body";
    [SerializeField] private string _tagArm = "Hit_Arm";
    [SerializeField] private string _tagLeg = "Hit_Leg";
    #endregion

    #region Property
    private LineRenderer lineRenderer;
    public float Damage => _damage;
    #endregion

    // 추후 만약 헤드 데미지 증가가 있으면 수정가능
    public void Init(float damage)
    {
        _damage = damage;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * _maxDistance);
        lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        HitCheck();
    }

    private void HitCheck()
    {
        int layerMask = LayerMask.GetMask(_layerMaskName);
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _maxDistance, layerMask);

        if (hit.collider != null)
        {
            Zombie zombie = hit.collider.gameObject.GetComponentInParent<Zombie>();

            if (zombie == null)
            {
                CPrint.Warn("Zombie.cs Component Load Fail.");
                return;
            }

            GameObject go = Instantiate(_hitGO, hit.point, Quaternion.identity);

            float multiply = 1f;

            if (hit.collider.tag == _tagHead)
            {
                multiply = _headMultiply;
                CPrint.Log("Head Hit");
            }
            else if (hit.collider.tag == _tagBody)
            {
                multiply = _bodyMultiply;
                CPrint.Log("Body Hit");
            }
            else if (hit.collider.tag == _tagArm)
            {
                multiply = _armMultiply;
                CPrint.Log("Arm Hit");
            }
            else if (hit.collider.tag == _tagLeg)
            {
                multiply = _legMultiply;
                CPrint.Log("Leg Hit");
            }

            float totalDamage = _damage * multiply;

            zombie.TakeDamage(totalDamage);

            this.gameObject.SetActive(false);
        }
    }
}
