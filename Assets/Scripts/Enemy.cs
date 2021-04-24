using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float speed = 10f;
    public float fireRate = 0.3f;
    public float health = 10f;
    public int score = 100;
    public float showDamageDuration = 0.1f; // Длительность эффекта попадания в секундах
    public float powerUpDropChance = 1f; // Вероятность сбросить бонус
    public GameObject explosion;

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials; // Все материалы игрового объекта и его потомков
    public bool showingDamage = false;
    public float damageDoneTime; //Время прекращения отображения эффекта
    public bool notifedOfDestruction = false;

    protected BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        // Получить материалы и цвет этого игрового объекта и его потомков
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
            originalColors[i] = materials[i].color;
    }

    public Vector3 Pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void Update()
    {
        Move();
        if (showingDamage && Time.time > damageDoneTime)
            UnShowDamage();
        if (bndCheck != null && bndCheck.offDown)
            Destroy(gameObject);
    }

    public virtual void Move()
    {
        Vector3 tempPos = Pos;
        tempPos.y -= speed * Time.deltaTime;
        Pos = tempPos;
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGo = coll.gameObject;
        switch (otherGo.tag)
        {
            case "ProjectileHero":
                Projectile p = otherGo.GetComponent<Projectile>();
                // Если вражеский корабль за границами экрана не наносить ему повреждений
                if (!bndCheck.isOnScreen)
                {
                    Destroy(otherGo);
                    break;
                }
                // Поразить вражеский корабль 
                ShowDamage();
                //Получить разрушающую силу из WEAP_DICT в классе Main
                health -= Main.GetWeaponDefinition(p.Type).damageOnHit;
                if (health <= 0)
                {
                    // Сообщить объекту-одиночке Main об уничтожении
                    if (!notifedOfDestruction)
                        Main.S.ShipDestroyed(this);
                    notifedOfDestruction = true;
                    explosion.transform.position = Pos;
                    Instantiate(explosion);
                    Destroy(gameObject);
                }
                Destroy(otherGo);
                break;

            default:
                print($"Enemy hit by non-ProjectileHero: {otherGo.name}");
                break;
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials)
            m.color = Color.red;
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for (int i = 0; i < materials.Length; i++)
            materials[i].color = originalColors[i];
        showingDamage = false;
    }
}
