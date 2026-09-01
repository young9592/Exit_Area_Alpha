using UnityEngine;

public partial class Player : MonoBehaviour
{
    #region Inspector
    [Header("Health")]
    [SerializeField] private float _health = 100;
    [SerializeField] private float _healthMax = 100;
    
    [Header("Stemina")]
    [SerializeField] private float _stemina = 100;
    [SerializeField] private float _steminaMax = 100;
    [SerializeField] private float _steminaConsume = 1f;

    [Header("Move")]
    [SerializeField] private float _walkSpeed = 4.0f;
    [SerializeField] private float _sprintMultiply = 1.5f;

    // 리지드 바디를 안쓰기 때문에 들어온 변수들
    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;
    // 캐릭터가 땅에 박혀있는걸 방지
    [SerializeField] private float _groundStick = -2.0f;
    #endregion

    #region Property
    public float Health => _health;
    public float HealthMax => _healthMax;
    public float Stemina => _stemina;
    public float SteminaMax => _steminaMax;
    #endregion
}
