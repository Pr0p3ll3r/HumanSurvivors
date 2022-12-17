using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FishNet;
using FishNet.Object;
using FishNet.Utility;

[System.Serializable]
public class ProfileData
{
    public string nickname;
    public int level;

    public ProfileData()
    {
        this.nickname = "NICKNAME";
        this.level = 1;
    }

    public ProfileData(string u, int l, int e)
    {
        this.nickname = u;
        this.level = l;
    }
}

public class MenuManager : NetworkBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Tabs")]
    public GameObject mainTab;
    public GameObject lobbyTab;

    [Header("MainTab")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OpenTab(mainTab);

        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        joinButton.onClick.AddListener(() =>
        {
            InstanceFinder.ClientManager.StartConnection();
        });
    }

    public void OpenTab(GameObject tab)
    {
        CloseTabs();
        tab.SetActive(true);
    }

    private void CloseTabs()
    {
        mainTab.SetActive(false);
        lobbyTab.SetActive(false);
    }

    private void Disconnect()
    {
        InstanceFinder.ServerManager.StopConnection(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
