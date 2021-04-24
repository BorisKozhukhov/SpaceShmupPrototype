﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Set in Inspector")]
    // Vector2.x хранит минимальное значение, а Vector2.y максимальное значение для метода Random.Range
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(.25f, 2);
    public float lifeTime = 6f; // Время в секундах существования PowerUp
    public float fadeTime = 4f;

    [Header("Set Dynamically")]
    public WeaponType type; // Тип бонуса
    public GameObject cube; // Ссылка на вложенный куб
    public TextMesh letter; //  Ссылка на TextMesh
    public Vector3 rotPerSecond; // Скорость вращения
    public float birthTime;  

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Renderer cubeRend;

    private void Awake()
    {
        // Получить ссылку на куб и другие компоненты
        cube = transform.Find("Cube").gameObject;
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeRend = cube.GetComponent<Renderer>();
        // Выбрать случайную скорость
        Vector3 vel = Random.onUnitSphere; //Random.onUnitSphere возвращает вектор, указывающий на случайную точку, находящуюся на поверхности сферы с радиусом 1 м и с центром в начале координат
        vel.z = 0; // Отобразить vel на плоскости XY
        vel.Normalize(); // Нормализация устанавливает длину Vector3 равной 1м
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rigid.velocity = vel;
        //  Утановить угол поворота этого игрового объекта равным R:[0,0,0]
        transform.rotation = Quaternion.identity; // Quaternion.identity Равноценно отсутсвию поворота.
        // Выбрать случайную скорость вращения для вложенного куба
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y));
        birthTime = Time.time;
    }

    private void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);
        // Эффект растворения куба PowerUp с течением времени. Со значением по умолчанию бонус существует 10 секунд, а затем растворяется в течении 4 секунд
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime; // В течении lifeTime секунд значение u будет <= 0. Затем оно станет положительным и через fadeTime секунд станет больше 1
        if (u >= 1)
        {
            Destroy(gameObject);
            return;
        }
        // Использовать u для определения альфа-значения куба и буквы
        if (u>0)
        {
            Color c = cubeRend.material.color;
            c.a = 1f - u;
            cubeRend.material.color = c;
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
        if (!bndCheck.isOnScreen)
            Destroy(gameObject);
    }

    public void SetType(WeaponType wt)
    {
        // Получить WeaponDefinition из Main
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        // Установить цвет дочернего куба и буквы на нём
        cubeRend.material.color = def.color;
        letter.color = def.color;
        letter.text = def.letter;
        type = wt;
    }

    public void AbsorbedBy(GameObject target)
    {
        // Эта функция вызывается классом Него, когда игрок подбирает бонус      
        Destroy(gameObject);
    }
}
