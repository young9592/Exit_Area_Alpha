using System.Collections.Generic;
using UnityEngine;

/*
외부에서 가져온 코드입니다.

Mesh mesh = new Mesh();
mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
combinedObject.GetComponent<MeshFilter>().sharedMesh = mesh;

많은 법선을 처리하기 위해 추가로 투입된 코드입니다.
 */

public class MeshCombiner : MonoBehaviour
{
    public GameObject Parent;
    public Material Material;
    public bool DeactivateParentAfterMerge = true;
    public bool DestroyParentAfterMerge = false;

    [ContextMenu("Merge")]
    public void MergeMeshes()
    {
        MeshFilter[] meshFilters = Parent.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combineList = new List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh != null)
            {
                CombineInstance combineInstance = new CombineInstance
                {
                    mesh = meshFilters[i].sharedMesh,
                    transform = meshFilters[i].transform.localToWorldMatrix
                };
                combineList.Add(combineInstance);
            }
        }

        GameObject combinedObject = new GameObject("Combined Mesh");
        combinedObject.AddComponent<MeshFilter>();
        combinedObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        combinedObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        combinedObject.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combineList.ToArray());
        combinedObject.GetComponent<MeshRenderer>().material = Material;


        if (DeactivateParentAfterMerge)
        {
            Parent.SetActive(false);
        }

        if (DestroyParentAfterMerge)
        {
            Destroy(Parent);
        }

    }

}