using System;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public enum Area
    {
        Area01,
        Area02_1,
        Area02_2,
        Area03_1,
        Area03_2,
        Area04_1,
        Area04_2,
        Area05_1
    }

    public event Action<int> OnAreaEnter;

    #region Inspector
    [Header("SelectArea")]
    [SerializeField] private Area _selectArea;

    [Header("EnterTargetName")]
    [SerializeField] private string _targetLayerName = "Player";
    #endregion

    #region Field
    private int _layerMask;
    #endregion

    private void Awake()
    {
        _layerMask = LayerMask.NameToLayer(_targetLayerName);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 타겟 지역 진입
        if (other.gameObject.layer.Equals(_layerMask))
        {
            OnAreaEnter?.Invoke((int)_selectArea);
            gameObject.SetActive(false);
        }
    }
}
