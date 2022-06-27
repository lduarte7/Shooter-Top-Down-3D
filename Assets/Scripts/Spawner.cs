using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode; //18


    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity; //13
    Transform playerT; //13

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainSpawn;
    float nextSpawnTime;
    int enemiesRemainAlive;

    MapGenerator map; //13

    float timebtwCampingChecks = 2; //13
    float nextCampCheckTime; //13
    float campThreshDistance = 1.5f; //13
    Vector3 campPositionOld; //13
    bool isCamping; //13

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    void Start() {
        playerEntity = FindObjectOfType<Player>(); //13
        playerT = playerEntity.transform; //13

        nextCampCheckTime = timebtwCampingChecks + Time.time; //13
        campPositionOld = playerT.position; //13
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>(); //13
        NextWave();
    }

    void Update() {
        if (!isDisabled) {

            if (Time.time > nextCampCheckTime) { //13
                nextCampCheckTime = Time.time + timebtwCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThreshDistance);
                campPositionOld = playerT.position;
            }

            if ((enemiesRemainSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
                enemiesRemainSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBtwSpawn;

            StartCoroutine("SpawnEnemy"); //13
            }
        }

        if (devMode) { //18
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy () { //13
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping) {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay) {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics (currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour); //18

    }

    void OnPlayerDeath () {
        isDisabled = true;
        
    }
    void OnEnemyDeath() {
        enemiesRemainAlive --;
        if (enemiesRemainAlive == 0) {
            NextWave();
        }
    }

    void ResetPlayerPosition() { //14
        playerT.position = map.GetTileFromPosition (Vector3.zero).position + Vector3.up * 3;
    }

    void NextWave() {
        if (currentWaveNumber > 0) {
            AudioManager.instance.Play2DSound ("Level Done"); //23
        }
        currentWaveNumber++;

            if (currentWaveNumber - 1 < waves.Length) {
        currentWave = waves [currentWaveNumber -1];

        enemiesRemainSpawn = currentWave.enemyCount;
        enemiesRemainAlive = enemiesRemainSpawn;

            if (OnNewWave != null) { //14
            OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition(); //14
        }
    }

    [System.Serializable]
    public class Wave {
        public int enemyCount;
        public float timeBtwSpawn;

        public bool infinite; //18
        public float moveSpeed;//18
        public int hitsToKillPlayer;//18
        public float enemyHealth;//18
        public Color skinColour;//18

    }
}
