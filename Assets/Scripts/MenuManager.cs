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

public class MenuManager : NetworkBehaviour, IPointerEnterHandler, IPointerClickHandler
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

    [Header("LobbyTab")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private GameObject playerListItem;

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

        //readyButton.onClick.AddListener(() =>
        //{
        //    SetIsReady();
        //});

        //leaveButton.onClick.AddListener(() =>
        //{
        //    Disconnect();
        //});

        //startButton.onClick.AddListener(() =>
        //{
        //    GameManager.Instance.StartGameMenu();
        //});
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

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        startButton.GetComponent<Button>().gameObject.SetActive(IsServer);
        GetCurrentPlayerList();
    }

    private void SetIsReady()
    {
        if (readyButton.GetComponent<Image>().color == Color.red)
        {
            readyButton.GetComponent<Image>().color = Color.green;
            PlayerInstance.Instance.ServerSetIsReady(true);
            leaveButton.interactable = false;
        }

        else if (readyButton.GetComponent<Image>().color == Color.green)
        {
            readyButton.GetComponent<Image>().color = Color.red;
            PlayerInstance.Instance.ServerSetIsReady(false);
            leaveButton.interactable = true;
        }
    }

    private void Disconnect()
    {
        InstanceFinder.ServerManager.StopConnection(true);
    }

    public void GetCurrentPlayerList()
    {
        Transform content = lobbyTab.transform.Find("PlayerList");
        foreach (Transform player in content) Destroy(player.gameObject);

        GameManager gameManager = GameManager.Instance;
        foreach (PlayerInstance player in gameManager.players)
        {
            GameObject newPlayerItem = Instantiate(playerListItem, content) as GameObject;
            newPlayerItem.transform.Find("Nickname").GetComponent<TextMeshProUGUI>().text = player.nickname;
            Debug.Log(player.isReady);
            if (player.isReady) newPlayerItem.transform.Find("Checkmark").GetComponent<Image>().color = Color.green;
            else newPlayerItem.transform.Find("Checkmark").GetComponent<Image>().color = Color.red;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Button>() != null) 
            SoundManager.Instance.PlayOneShot("Hover");    
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Button>() != null)
            SoundManager.Instance.PlayOneShot("Click");
    }
}
