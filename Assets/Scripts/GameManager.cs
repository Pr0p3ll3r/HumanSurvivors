using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float timeToStart = 0;

    private TextMeshProUGUI timer;
    private int currentGameTime;
    private Coroutine timerCoroutine;
    private GameObject gameOver;
    [SerializeField] private GameObject wonEffect;

    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI timeSurvivedScoreText;
    [SerializeField] private TextMeshProUGUI zombieKilledText;
    [SerializeField] private TextMeshProUGUI zombieKilledScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private Coroutine countingCo;
    private bool started = false;

    [SyncObject]
    public readonly SyncList<PlayerInstance> players = new SyncList<PlayerInstance>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Instance = this;
        gameOver = GameObject.Find("Canvas").transform.Find("GameOver").gameObject;
        timer = GameObject.Find("GameHUD/Timer/Time").GetComponent<TextMeshProUGUI>();
    }

    [Server]
    void StartGame()
    {
        if (started) return;

        Debug.Log("Game Started");
        started = true;
        foreach (PlayerInstance player in players)
        {
            player.SpawnPlayer();
        }
        InitializeTimer();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (started == false)
        {
            if (countingCo == null) countingCo = StartCoroutine(CountingSound());
            timer.text = "Prepare to fight..." + (1 + (int)(timeToStart - Time.timeSinceLevelLoad));
            if (Time.timeSinceLevelLoad >= timeToStart) StartGame();
            return;
        }
    }

    IEnumerator CountingSound()
    {
        while(!started)
        {
            yield return new WaitForSeconds(1f);
        }
    }

    public void Gameover()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        
        

        StartCoroutine(Wait(3f));
    }

    [ObserversRpc]
    private void InitializeTimer()
    {
        currentGameTime = 0;
        RefreshTimerUI();

        StartCoroutine(Timer());
    }

    private void RefreshTimerUI()
    {
        timer.text = TimeSpan.FromSeconds(currentGameTime).ToString(@"hh\:mm\:ss");
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        gameOver.SetActive(true);
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentGameTime += 1;
        RefreshTimerUI();
        timerCoroutine = StartCoroutine(Timer());
    }

    public void TimerStatus(bool status)
    {
        if (status) timerCoroutine = StartCoroutine(Timer());
        else StopCoroutine(timerCoroutine);
    }

    public void SetGameOverScreen(int zombieKilled)
    {
        timeSurvivedText.text = TimeSpan.FromSeconds(currentGameTime).ToString(@"hh\:mm\:ss");
        timeSurvivedScoreText.text = $"+{currentGameTime}";

        zombieKilledText.text = zombieKilled.ToString();
        int zombieScore = 10 * zombieKilled;
        zombieKilledScoreText.text = $"+{zombieScore}";

        int totalScore = zombieScore + currentGameTime;
        currentScoreText.text = totalScore.ToString();

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = highScore.ToString();

        if (totalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", totalScore);
            highScoreText.text = totalScore.ToString();
        }
    }
}