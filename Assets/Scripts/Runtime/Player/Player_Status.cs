using UnityEngine;

public partial class Player : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _health = 100;
    [SerializeField] private float _healthMax = 100;

    [Header("Move")]
    [SerializeField] private float _runSpeed = 5.0f;

    // 리지드 바디를 안쓰기 때문에 들어온 변수들
    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;
    // 캐릭터가 땅에 박혀있는걸 방지
    [SerializeField] private float _groundStick = -2.0f;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Clamp(0, _healthMax);
        
        if(_health == 0)
        {
            // game over
        }
    }

    private void Clamp(float min, float max)
    {
        if (_health < min)
        {
            _health = min;
        }
        else if (_health > max)
        {
            _health = max;
        }
    }
}
