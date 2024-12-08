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

    // LP 추가
    public void AddLP(int point)
    {
        if (_lovePoint <= 1000) _lovePoint += point;
        if (_lovePoint > 1000) _lovePoint = 1000;
        OnChangedLP.Invoke();
    }

    // MP 추가
    public void AddMP(int point)
    {
        if(_mentalityPoint > -100 && _mentalityPoint <= 100) _mentalityPoint += point;
        if (_mentalityPoint > 100) _mentalityPoint = 100;
        if (_mentalityPoint < -100) _mentalityPoint = -100;
        OnChangedMP.Invoke();
    }

    // 현재 LP 반환
    public int GetCurrentLP()
    {
        return _lovePoint;
    }

    // 현재 MP 반환
    public int GetCurrentMP()
    {
        return _mentalityPoint;
    }
}