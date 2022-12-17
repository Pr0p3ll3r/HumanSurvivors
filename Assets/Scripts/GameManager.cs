using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float timeToStart = 0;  
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI timeSurvivedScoreText;
    [SerializeField] private TextMeshProUGUI zombieKilledText;
    [SerializeField] private TextMeshProUGUI zombieKilledScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private int currentGameTime;
    private Coroutine timerCoroutine;
    private Coroutine countingCo;
    private bool started = false;
    public bool GameStarted => started;

    [SyncObject]
    public readonly SyncList<PlayerInstance> players = new SyncList<PlayerInstance>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Instance = this;
    }

    [Server]
    void StartGame()
    {
        Debug.Log("Game Started");
        foreach (PlayerInstance player in players)
        {
            player.SpawnPlayer();
        }
        InitializeTimer();
    }

    [ObserversRpc(RunLocally = true)]
    void CheckCountdown(int time)
    {
        timer.text = $"Waiting for players... {time}";
    }

    private void Update()
    {
        if (started == false)
        {
            if(countingCo == null) countingCo = StartCoroutine(CountingSound());
            if(IsServer)
            {
                CheckCountdown((int)(1 + timeToStart - NetworkManager.TimeManager.ServerUptime));
                if (NetworkManager.TimeManager.ServerUptime >= timeToStart) StartGame();
            }             
        }
    }

    IEnumerator CountingSound()
    {
        while(!started)
        {
            SoundManager.Instance.PlayOneShot("Countdown");
            yield return new WaitForSeconds(1f);
        }
    }

    public void Gameover()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        
        

        StartCoroutine(Wait(3f));
    }

    [ObserversRpc(RunLocally = true)]
    private void InitializeTimer()
    {
        started = true;
        currentGameTime = 0;
        timer.text = TimeSpan.FromSeconds(currentGameTime).ToString(@"hh\:mm\:ss");
        StartCoroutine(Timer());
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        gameOverPanel.SetActive(true);
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentGameTime += 1;
        timer.text = TimeSpan.FromSeconds(currentGameTime).ToString(@"hh\:mm\:ss");
        timerCoroutine = StartCoroutine(Timer());
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