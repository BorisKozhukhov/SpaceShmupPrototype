using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_3 : Enemy
{
    // Траектория движения Enemy_3 вычисляется путем линейной интерполяции кривой Безье по более чем двум точкам.
    [Header("Set in Inspector: Enemy_3")]
    public float lifeTime = 5;

    [Header("Set Dynamically: Enemy_3")]
    public Vector3[] points;
    public float birthTime;

    private void Start()
    {
        points = new Vector3[3];
        points[0] = Pos;
        float xMin = -bndCheck.camWidth + bndCheck.radius;
        float xMax = bndCheck.camWidth - bndCheck.radius;
        // Случайно выбрать среднюю точку ниже нижней границы экрана
        points[1] = Vector3.zero;
        points[1].x = Random.Range(xMin, xMax);
        points[1].y = -bndCheck.camHeight * Random.Range(2.75f, 2);
        // Случайно выбрать конечную точку выше верхней границы экрана
        points[2] = Vector3.zero;
        points[2].y = Pos.y;
        points[2].x = Random.Range(xMin, xMax);
        // Записать в birthTime текущее время
        birthTime = Time.time;
    }

    public override void Move()
    {
        // Кривые Безье вычисляются на основе значения u между 0 и 1
        float u = (Time.time - birthTime) / lifeTime;
        if (u > 1)
        {
            Destroy(gameObject);
            return;
        }
        // Интерполировать кривую Безье по трём точкам
        Vector3 p01, p12;
        u -= 0.2f * Mathf.Sin(u * Mathf.PI * 2);
        p01 = (1 - u) * points[0] + u * points[1];
        p12 = (1 - u) * points[1] + u * points[2];
        Pos = (1 - u) * p01 + u * p12;
    }
}
