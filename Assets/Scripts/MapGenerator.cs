using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform navmeshFloor;
    public Transform mapFloor; //18
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;


    [Range(0,1)]
    public float outlinePercent;

    public Transform obstaclePrefab; //gerar obstaculos **10
    
    List<Coord> allTileCoords; //lista com todas as coordenadas dos Tiles *10
    Queue<Coord> shuffledTileCoords; //faz uma lista com embaralhada dos tiles *10
    Queue<Coord> shuffledOpenTileCoords; // 13, faz uma lista dos tiles que estao disponiveis para andar

    public float tileSize; //11 para controlar o tamanho dos tiles

    public Map[] maps; //12
    public int mapIndex; //12
    Map currentMap; //12

    Transform[,] tilemap; //13


    void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave (int waveNumber) { //14
        mapIndex = waveNumber -1;
        GenerateMap();
    }

    public void GenerateMap() {

        currentMap = maps[mapIndex]; //12
        tilemap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y]; //13
        System.Random prng = new System.Random (currentMap.seed);

        //Gerando as coordenadas
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                allTileCoords.Add(new Coord(x,y));
            }
        } 
        shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray(), currentMap.seed));

        //Cria o mapa que segura os objetos
        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Spawna os tiles
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize; //11
                newTile.parent = mapHolder;
                tilemap[x,y] = newTile; //13
            }
        }

        // Spawna os obstaculos
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y]; //10

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0; // 10
        List<Coord> allOpenCoords = new List<Coord> (allTileCoords); //13

        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true; //10
            currentObstacleCount ++; // 10

            if(randomCoord != currentMap.mapCentre && MapIsFullyAcessible(obstacleMap, currentObstacleCount)) { //10
            float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2 , Quaternion.identity) as Transform;
            newObstacle.parent = mapHolder;
            newObstacle.localScale =  new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent)* tileSize); //11

            Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
            Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
            float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
            obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
            obstacleRenderer.sharedMaterial = obstacleMaterial;
            allOpenCoords.Remove(randomCoord); //13

            }
            else { // 10
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount --;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray (allOpenCoords.ToArray(), currentMap.seed)); //13

        // Cria as paredes do mapa
        Transform maskleft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        maskleft.parent = mapHolder;
        maskleft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x)/ 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskright = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;    
        maskright.parent = mapHolder;
        maskright.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x)/ 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform masktop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;    
        masktop.parent = mapHolder;
        masktop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;

        Transform maskbot = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;    
        maskbot.parent = mapHolder;
        maskbot.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;


        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3 (currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize); //18 
    }

    bool MapIsFullyAcessible(bool[,] obstacleMap, int currentObstacleCount){ //10
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (currentMap.mapCentre);
		mapFlags [currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

		int accessibleTileCount = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();

			for (int x = -1; x <= 1; x ++) {
				for (int y = -1; y <= 1; y ++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x == 0 || y == 0) {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {
							if (!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]) {
								mapFlags[neighbourX,neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX,neighbourY));
								accessibleTileCount ++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
    }



    Vector3 CoordToPosition(int x, int y) {
        return new Vector3 (-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize; //11
    }


    public Transform GetTileFromPosition(Vector3 position) { //13
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp (x, 0, tilemap.GetLength(0) -1);
        y = Mathf.Clamp (x, 0, tilemap.GetLength(1) -1);
        return tilemap [x, y];
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue ();
        shuffledTileCoords.Enqueue (randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile() { //13
        Coord randomCoord = shuffledOpenTileCoords.Dequeue ();
        shuffledOpenTileCoords.Enqueue (randomCoord);
        return tilemap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) { //10
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator !=(Coord c1, Coord c2) { //10
			return !(c1 == c2);
		}
    }

    [System.Serializable]
    public class Map {

        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;


        public Coord mapCentre {
            get {
                return new Coord (mapSize.x /2, mapSize.y /2);
            }
        }

    }
}
        