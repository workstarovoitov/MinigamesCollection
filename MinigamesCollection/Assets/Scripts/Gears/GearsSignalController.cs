using UnityEngine;

public class GearsSignalController : MonoBehaviour
{
    [SerializeField] private GameObject[] levelSignals;

    private void Start()
    {
        for (int i = 0; i < levelSignals.Length; i++)
        {
            levelSignals[i].SetActive(false);
        }
    }

    public void SetLevel(int level)
    {
        for (int i = 0; i < levelSignals.Length;i++)
        {
            if (i < level) levelSignals[i].SetActive(true);
            else levelSignals[i].SetActive(false);
        }
    }
}
