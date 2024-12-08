using System;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager instance;

    private int _lovePoint = 0;
    private int _mentalityPoint = 0;
    public Action OnChangedLP;
    public Action OnChangedMP;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // LP �߰�
    public void AddLP(int point)
    {
        if (_lovePoint <= 1000) _lovePoint += point;
        if (_lovePoint > 1000) _lovePoint = 1000;
        OnChangedLP.Invoke();
    }

    // MP �߰�
    public void AddMP(int point)
    {
        if(_mentalityPoint > -100 && _mentalityPoint <= 100) _mentalityPoint += point;
        if (_mentalityPoint > 100) _mentalityPoint = 100;
        if (_mentalityPoint < -100) _mentalityPoint = -100;
        OnChangedMP.Invoke();
    }

    // ���� LP ��ȯ
    public int GetCurrentLP()
    {
        return _lovePoint;
    }

    // ���� MP ��ȯ
    public int GetCurrentMP()
    {
        return _mentalityPoint;
    }
}