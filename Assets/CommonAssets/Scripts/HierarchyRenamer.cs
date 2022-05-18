using UnityEngine;

public class HierarchyRenamer : MonoBehaviour
{
    public string find;
    public string replaceWith;

    [Space]
    [InspectorButton("OnButtonClicked")]
    public bool rename;

    private void OnButtonClicked()
    {
        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childs.Length; i++)
        {
            childs[i].name = childs[i].name.Replace(find, replaceWith);
        }
    }
}
