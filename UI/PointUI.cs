using UnityEngine;
using TMPro;

public class PointUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _LP;
    [SerializeField] private TMP_Text _MP;

    private void Start()
    {
        PointManager.instance.OnChangedLP += ShowLP;
        PointManager.instance.OnChangedMP += ShowMP;
    }

    private void ShowLP()
    {
        _LP.text = $"LP: {PointManager.instance.GetCurrentLP()}";
    }

    private void ShowMP()
    {
        _MP.text = $"MP: {PointManager.instance.GetCurrentMP()}";
    }

    private void OnDestroy()
    {
        PointManager.instance.OnChangedLP -= ShowLP;
        PointManager.instance.OnChangedMP -= ShowMP;
    }
}