using UnityEngine;

public partial class Player : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _health = 100;
    [SerializeField] private float _healthMax = 100;
    
    [Header("Stemina")]
    [SerializeField] private float _stemina = 100;
    [SerializeField] private float _steminaMax = 100;

    [Header("Move")]
    [SerializeField] private float _walkSpeed = 4.0f;
    [SerializeField] private float _sprintMultiply = 1.5f;

    // 리지드 바디를 안쓰기 때문에 들어온 변수들
    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;
    // 캐릭터가 땅에 박혀있는걸 방지
    [SerializeField] private float _groundStick = -2.0f;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        _health = Mathf.Clamp(_health, 0, _healthMax);
        CPrint.Log($"플레이어는 {damage}의 데미지를 입었습니다.");
        
        if(_health == 0 && !_isDead)
        {
            // game over
            CPrint.Warn("플레이어가 사망하였습니다.");
            _animator.SetTrigger(_hashDead);
            _controller.enabled = false;
            _rig.enabled = false;
            _isDead = true;
        }
    }
}
