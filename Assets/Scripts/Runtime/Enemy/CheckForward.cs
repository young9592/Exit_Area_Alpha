using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForward : MonoBehaviour
{
    #region Field
    private int _layerMask;
    private bool _hit = false;
    #endregion

    #region Property
    public bool HitCheck => _hit;
    #endregion

    private void Awake()
    {
        _layerMask = LayerMask.NameToLayer("Block");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer.Equals(_layerMask))
        {
            _hit = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(_layerMask))
        {
            _hit = false;
        }
    }
}
