using UnityEngine;

public class Zombie : MonoBehaviour
{
    #region Inspector
    [Header("필수 참조")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;

    [Header("Zombie Status")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private float _healthMax = 100f;

    [Header("Animation Parameter")]
    [SerializeField] private string _paramDead = "tDead";
    #endregion

    #region Field
    private int _hashDead;
    #endregion

    private void OnEnable()
    {
        _health = _healthMax;
    }
    private void Awake()
    {
        _hashDead = Animator.StringToHash(_paramDead);
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        CPrint.Log($"좀비는 {damage}데미지를 입었습니다.");
        _health -= damage;

        Mathf.Clamp(_health, 0, _healthMax);

        if(_health <= 0)
        {
            CPrint.Log("좀비는 죽었습니다.");
            _animator.SetTrigger(_hashDead);
        }
    }
}
