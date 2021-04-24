using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    static public Main S;
    static protected Dictionary<WeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Set in Inspector")]
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 2f;
    public float enemyDefaultPadding = 1.5f; // Отступ для позиционированния
    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;
    public WeaponType[] powerUpFrequency = new WeaponType[]
    {
        WeaponType.blaster, WeaponType.spread, WeaponType.shield
    };

    private BoundsCheck bndCheck;

    public void ShipDestroyed(Enemy e)
    {
        // Сгенерировать бонус с заданной вероятностью
        if (Random.value <= e.powerUpDropChance)
        {
            // Выбрать тип бонуса
            int ndx = Random.Range(0, powerUpFrequency.Length);
            WeaponType puType = powerUpFrequency[ndx];
            // Создание экземпляра PowerUp
            GameObject go = Instantiate(prefabPowerUp) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            // Установка соответствующего типа WeaponType
            pu.SetType(puType);
            // Поместить на место разрушенного корабля
            pu.transform.position = e.transform.position;   
        }
    }

    private void Awake()
    {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();
        Invoke("SpawnEnemy", enemySpawnPerSecond);

        // Словарь с ключами типа WeaponType
        WEAP_DICT = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
            WEAP_DICT[def.type] = def;
    }

    public void SpawnEnemy()
    {
        Vector3 pos = Vector3.zero;
        // Выбрать случайный шаблон Enemy для создания
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate(prefabEnemies[ndx]);
        // Разместить вражеский корабль над экраном в случайной позиции x
        float enemyPadding = enemyDefaultPadding;
        if (go.GetComponent<BoundsCheck>() != null)
            enemyPadding = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        // Установить начальные координаты созданного вражеского корабля
        float xMin = -bndCheck.camWidth + enemyPadding;
        float xMax = bndCheck.camWidth - enemyPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyPadding;
        go.transform.position = pos;
        Invoke("SpawnEnemy", enemySpawnPerSecond);
    }

    public void DelayedRestart (float delay)
    {
        // Вызвать метод Restart через delay секунд
        Invoke("Restart", delay);
    }

    public void Restart()
    {
        // Перезагрузить Scene_0, чтобы перезапустить игру
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Статическая функция, возвращающая WeaponDefinition из статического защищённого поля WEAP_DICT класса Main
    /// </summary>
    /// <returns> Экземпляр WeaponDefinition или, если нет такого определения для указанного WeaponType, возвращает новый экземпляр WeaponDefinition с типом none. </returns>
    /// <param name="wt"> тип оружия WeaponType, для которого требуется получить WeaponDefinition </param>

    static public WeaponDefinition GetWeaponDefinition(WeaponType wt)
    {
        // Проверить наличие указанного ключа в словаре, попытка извлечь значение по отсутсвующему ключу вызовет ошибку
        if (WEAP_DICT.ContainsKey(wt))
            return WEAP_DICT[wt];
        // Следующая инструкция возвращает новый экземпляр WeaponDefinition с типом оружия WeaponType.none, что означает неудачную попытку найти требуемое определение WeaponDefinition
        return new WeaponDefinition();
    }
}
