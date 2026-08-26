using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected string _name;
    protected float _damage;
    protected float _fireDelay;
    protected float _reloadDelay;

    protected int _magazine;
    protected int _bulletCount;
    protected int _burstCount;


}
