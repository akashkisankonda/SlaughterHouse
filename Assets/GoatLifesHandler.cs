using UnityEngine;

public class GoatLifesHandler : MonoBehaviour
{
    public GameObject lifePrefab;

    public void AddLife()
    {
        Instantiate(lifePrefab).transform.SetParent(transform);
    }

    public void Clear()
    {
        int len = transform.childCount;
        for (int i = 0; i < len; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void OnDisable()
    {
        Clear();
    }
}
