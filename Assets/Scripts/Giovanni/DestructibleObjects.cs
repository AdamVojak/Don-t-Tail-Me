using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Postavke uništenja")]
    [SerializeField] private GameObject brokenVersionPrefab;

    void Start()
    {
        if (brokenVersionPrefab != null)
        {
            brokenVersionPrefab.SetActive(false);
        }
    }

    public void DestroyAndReplace()
    {
        if (brokenVersionPrefab != null)
        {
            brokenVersionPrefab.transform.parent = null;
            brokenVersionPrefab.SetActive(true);
        }

        Destroy(gameObject);
    }
}