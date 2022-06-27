using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public Text scoreUI;
    public RectTransform healthBar; //25
    Player player;

    public RectTransform NewWaveBanner; //21
    public Text waveTitle; //21

    Spawner spawner;

    void Start()
    {
        player = FindObjectOfType<Player>(); 
        player.OnDeath += OnGameOver;
    }

    void Awake () { //21
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Update () {
        scoreUI.text = Score.score.ToString("Enemies Killed: " + Score.score); //25
        float healthPercent = 0;
		if (player != null) {
			healthPercent = player.health / player.startingHealth;
		}
		healthBar.localScale = new Vector3 (healthPercent, 1, 1);
    }

    void OnNewWave (int waveNumber) { //21
        string [] numbers = {"One", "Two", "Three", "Four", "Five"};
        waveTitle.text = "- Wave " + numbers [waveNumber -1] + " -";
        string enemyCountString = ((spawner.waves [waveNumber - 1].infinite) 
        ? "Infinite" : spawner.waves [waveNumber - 1].enemyCount + "");
        
        StopCoroutine (AnimatedNewWaveBanner());        
        StartCoroutine (AnimatedNewWaveBanner());
    }

    IEnumerator AnimatedNewWaveBanner() { //21

        float delayTime = 1f;
        float speed = 2.5f;
        float animatePercent = 0; 
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0) {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1) {
                animatePercent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                }
            }

            NewWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp (170, 45, animatePercent);
            yield return null;
        }
    }

    void OnGameOver () {
        Cursor.visible = true;
        StartCoroutine(Fade (Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from,to,percent);
            yield return null;
        }
    }

    //UI INPUT
    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

}
