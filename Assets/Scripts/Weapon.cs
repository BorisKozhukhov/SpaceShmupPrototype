using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Это перечисление всех возможных типов оружия.
/// Также включает тип shield, чтобы дать возможность совершенствовать защиту.
/// </summary>

public enum WeaponType
{
    none, // По умолчанию / нет оружия
    blaster, // Простой бластер
    spread, // Веерная пушка, стреляющая несколькими снарядами
    phaser, // Волновой фазер
    shield // Увеличивает ShieldLevel
}

/// <summary>
/// Класс WeaponDefinition позволяет настраивать свойства конкретного вида оружия в инспекторе. Для этого класс Main будет хранить массив элементов типа WeaponDefinition.
/// </summary>

[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter; // Буква на кубике, изображающем бонус
    public Color color = Color.white; // Цвет ствола оружия и кубика бонуса
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0; // Разрушительная мощность
    public float delayBetweenShots = 0; // Задержка между выстрелами
    public float velocity = 20; // Скорость полёта снарядов
}
public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime;
    private Renderer collarRend;

    private float birthTime;

    private void Start()
    {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();
        // Вызвать SetType чтобы заменить тип оружия по умолчанию WeaponType.none
        SetType(_type);
        // Динамически создать точку привязки для всех снарядов
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        // Найти fireDelegate в корневом игровом объекте
        GameObject rootGo = transform.root.gameObject;
        if (rootGo.GetComponent<Hero>() != null)
            rootGo.GetComponent<Hero>().fireDelegate += Fire;
        birthTime = Time.time;
    }

    public WeaponType Type
    {
        get { return _type; }
        set { SetType( value ); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (_type == WeaponType.none)
        {
            gameObject.SetActive(false);
            return;
        }
        else
            gameObject.SetActive(true);
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0; // Сразу после установки _type можно выстрелить
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy)
            return;
        // Если между выстрелами прошло недостаточно много времени, выйти
        if (Time.time - lastShotTime < def.delayBetweenShots)
            return;
        Projectile p = null;
        Vector3 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0)
            vel.y = -vel.y;
        switch (Type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.velocity = vel;
                break;
            case WeaponType.spread:
                p = MakeProjectile(); // Снаряд летящий прямо
                p.rigid.velocity = vel;
                p = MakeProjectile(); // Снаряд летящий вправо
                p.transform.rotation = Quaternion.AngleAxis(7.5f, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); // Снаряд летящий влевоx
                p.transform.rotation = Quaternion.AngleAxis(-7.5f, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); // Снаряд летящий ещё првее
                p.transform.rotation = Quaternion.AngleAxis(15, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); // Снаряд летящий ещё левее 
                p.transform.rotation = Quaternion.AngleAxis(-15, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                break;
            case WeaponType.phaser:
                p = MakeProjectile();
                float x0 = p.transform.position.x;
                Vector3 tempPos = p.transform.position;
                float age = Time.time - birthTime;
                float theta = Mathf.PI * 2 * age / 3f;
                float sin = Mathf.Sin(theta);
                tempPos.x = x0 + 4 * sin;
                p.transform.position = tempPos;
                Vector3 rot = new Vector3(0, sin * 45, 0);
                p.transform.rotation = Quaternion.Euler(rot);
                p.rigid.velocity = vel;
                p = MakeProjectile();
                x0 = p.transform.position.x;
                tempPos = p.transform.position;
                tempPos.x = x0 + 4 * -sin;
                p.transform.position = tempPos;
                p.transform.rotation = Quaternion.Euler(rot);
                p.rigid.velocity = vel;
                break;
        }
    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate(def.projectilePrefab);
        if (transform.parent.gameObject.CompareTag("Hero"))
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.Type = Type;
        lastShotTime = Time.time;
        return p;
    }
}
