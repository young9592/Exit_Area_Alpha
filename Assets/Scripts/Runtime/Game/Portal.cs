using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public event Action OnNextStage;

    #region Inspector
    [Header("Portal Interact Object Name")]
    [SerializeField] private string layerName = "Player";
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.gameObject.layer.Equals(LayerMask.NameToLayer(layerName)))
        {
            OnNextStage?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
