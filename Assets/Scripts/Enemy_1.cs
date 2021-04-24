using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy
{
    [Header("Set in Inspector: Enemy_1")]
    // Число секунд полного цикла синусоиды
    public float waveFrequency = 2;
    // Ширина синусоиды в метрах
    public float waveWidth = 4;
    public float waveRotY = 45;

    // Начальное значение координаты X
    private float x0;
    private float birthTime;

    private void Start()
    {
        // Установить начальнуб координату X объекта Enemy_1
        x0 = Pos.x;
        birthTime = Time.time;
    }

    public override void Move()
    {
        Vector3 tempPos = Pos;
        //Значение theta изменяется с течением времени
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        Pos = tempPos;
        // Повернуть немного относительно оси Y
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        transform.rotation = Quaternion.Euler(rot);
        base.Move();
    }
}
