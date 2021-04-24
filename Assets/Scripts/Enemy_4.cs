using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Part - ещё один сереализуемый класс подобно WeaponDefinition, предназначенный для хранения данных
/// </summary>
[System.Serializable]
public class Part
{
    // Значения этих трёх полей должны определяться в инспекторе
    public string name; // Имя этой части
    public float health; // Здоровье этой части
    public string[] protectedBy; // Другие части, защищающие эту

    // Эти два поля инициализируются автоматически в Start(). Кэширование ускоряет получение необходимых данных
    [HideInInspector]
    public GameObject go; // Игровой объект этой части
    [HideInInspector]
    public Material mat; // Материал для отображения повреждений
}

/// <summary>
/// Enemy_4 создаётся за верхней границей, выбирает случайную точку на экране и перемещается к ней. Добравшись до места, выбирает другую случайную точку и продолжает двигаться, пока игрок не уничтожит его.
/// </summary>
public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts; // Массив частей, составляющих корабль 

    private Vector3 p0, p1; // Две точки для интерполяции
    private float timeStart; // Время создания этого корабля 
    private float duration = 4; // Продолжительность перемещения

    private void Start()
    {
        // Начальная позиция уже выбрана в Main.SpawnEnemy()
        p0 = p1 = Pos;
        InitMovement();

        // Записать в кэш игровой объект и материал каждой части в parts
        Transform t;
        foreach (Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        p0 = p1;
        // Выбрать новую точку p1 на экране
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Сбросить время 
        timeStart = Time.time;
    }

    public override void Move()
    {
        // Этот метод переопределяет Enemy.Move() и реализует линейную интерполяцию
        float u = (Time.time - timeStart) / duration;
        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }
        u = 1 - Mathf.Pow(1 - u, 2); // Применить плавное замедление
        Pos = (1 - u) * p0 + u * p1; // Простая линейная интерполяция
    }

    // Эти две функции выполняют поиск частей в массиве parts по имени или по ссылке на игровой объект
    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name == n)
                return prt;
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go == go)
                return prt;
        }
        return null;
    }

    // Эти функции возвращают true если данная часть уничтожена
    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }

    bool Destroyed(Part prt)
    {
        // Если ссылка на часть не была передана, вернуть true
        if (prt == null)
            return true;
        return prt.health <= 0;
    }

    // Окрашивает в красный толко одну часть, а не весь корабль
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                // Если корабль за границами экрана, не повреждать его
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }
                // Поразить вражеский корабль
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null) // Если prtHit не найден
                {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                // Проверить защищена ли ещё эта часть корабля
                if (prtHit.protectedBy != null )
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        // Если хотя бы одна из защищающих частей ещё не разрушена не наносить повреждений этой части
                        if (!Destroyed(s))
                        {
                            Destroy(other);
                            return;
                        }
                    }
                }
                // Эта часть не защищена, нанести ей повреждение
                prtHit.health -= Main.GetWeaponDefinition(p.Type).damageOnHit;
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    // Вместо разрушения всего корабля, деактивировать уничтоженную часть
                    prtHit.go.SetActive(false);
                }
                // Проверить был ли корабль полностью разрушен
                bool allDestroyed = true; // Прредположить что разрушен
                foreach (Part prt in parts)
                {
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                if (allDestroyed)
                {
                    // Уведомить объект-одиночку Main, что этот корабль разрушен
                    Main.S.ShipDestroyed(this);
                    explosion.transform.position = Pos;
                    Instantiate(explosion);
                    Destroy(gameObject);
                }
                Destroy(other);
                break;
        }
    }
}
