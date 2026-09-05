using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    #region Inspector
    [Header("아이템 세팅")]
    [SerializeField] private List<GameObject> _items;

    [Header("Case Cover")]
    [SerializeField] private Transform _coverTr;

    [Header("WeaponCase Inspector")]
    [Range(-2f, 0)]
    [SerializeField] private float _positionMin = -0.5f;
    [Range(0, 2f)]
    [SerializeField] private float _positionMax = 0.5f;
    [SerializeField] private float _pushUpForce = 3f;
    [SerializeField] private float _pushRightForce = 1.5f;
    #endregion

    #region Field
    private CTimer _openTimer = new CTimer();
    #endregion

    private void Update()
    {
        if (!_openTimer.GetCurrentTimerState)
        {
            return;
        }

        if (_items.Count == 0)
        {
            _coverTr.localRotation = Quaternion.Slerp(_coverTr.localRotation, Quaternion.Euler(-120f, 0, 0), 1f - Mathf.Exp(-2 * Time.deltaTime));
        }
    }

    public void Open()
    {
        if (_items.Count == 0)
        {
            return;
        }

        CPrint.Log("무기상자 열기");

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items == null)
            {
                // empty 상황 체크
                continue;
            }

            GameObject weapon = Instantiate(_items[i], (transform.position + transform.up * 0.5f + transform.right * Random.Range(_positionMin, _positionMax)), Quaternion.Euler(0, 0, 90));
            Rigidbody rb = weapon.GetComponent<Rigidbody>();

            Vector3 totalForce = Vector3.up * _pushUpForce + transform.forward * _pushRightForce;

            rb.AddForce(totalForce, ForceMode.Impulse);
        }

        _openTimer.SetTimer(3f);
        _items.Clear();
    }
}
