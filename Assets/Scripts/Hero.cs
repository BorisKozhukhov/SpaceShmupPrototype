using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S;
    [Header("Set in Inspector")]
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestrartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;
    public GameObject explosion;

    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

    // Эта переменная хранит ссылку на последний столкнувшийся GameObject
    private GameObject lastTriggerGo = null;
    // Поле для хранения Shield
    private GameObject shield = null;
    
    public delegate void WeaponFireDelegate();
    public WeaponFireDelegate fireDelegate;

    void Start()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        // Очистить массив weapons и начать игру с 1 бластером
        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
        shield = transform.Find("Shield").gameObject;
    }

    void Update()
    {
        // Извлечь информацию из класса Input
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");
        // Изменить transofrm.position опираясь на информацию по осям
        transform.position += new Vector3(xAxis * speed * Time.deltaTime, yAxis * speed * Time.deltaTime, 0.0f);
        // Повернуть корабль чтобы придать ощущение динамизма
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        if (Input.GetAxis("Jump") == 1 && fireDelegate!= null)
        {
            fireDelegate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        // Гарантировать невозможность повторного столкновения с тем же объектом
        if (go == lastTriggerGo)
            return;
        lastTriggerGo = go;
        if (go.CompareTag("Enemy"))
        {
            ShieldLevel--;
            Destroy(go);
            explosion.transform.position = go.transform.position;
            Instantiate(explosion);
            ClearWeapons();
            weapons[0].SetType(WeaponType.blaster);
        }
        else if(go.CompareTag("PowerUp"))
        {
            AbsorbPowerUp(go);
        }
        else
            print($"Triggered by non-enemy {go.name}");
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type)
        {
            case WeaponType.shield:
                ShieldLevel++;
                break;
            default:
                // Если оружие того же типа, установить в pu.type
                if (pu.type == weapons[0].Type)
                {
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null)
                        w.SetType(pu.type);
                }
                else // Если оружие другого типа
                {
                    ClearWeapons();
                    weapons[0].SetType(pu.type);
                }
                break;
        }
        pu.AbsorbedBy(gameObject);
    }

    public float ShieldLevel
    {
        get
        {
            return _shieldLevel;
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            if (value < 0)
            {
                explosion.transform.position = gameObject.transform.position;
                Instantiate(explosion);
                Destroy(gameObject);
                // Сообщить объекту Main.S о необходимости перезапустить игру
                Main.S.DelayedRestart(gameRestrartDelay);
            }
            if (value == 0)
                shield.SetActive(false);
            else
                shield.SetActive(true);
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].Type == WeaponType.none)
                return weapons[i];
        }
        return null;
    }

    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
            w.SetType(WeaponType.none);
    }
}
