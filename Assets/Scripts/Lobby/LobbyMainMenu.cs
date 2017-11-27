using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour 
    {
        public LobbyManager lobbyManager;

        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;

        public InputField ipInput;
        public InputField matchNameInput;
        
        public InputField ipManual;

        //dichiaro un booleano per specificare in fase di build dell'app se parto come client di recuperare l'ip del server da internet
        public bool imAClient = false;

        string url = "http://www.danielesangineto.com/ip.txt";
        string URL_to_script = "http://www.danielesangineto.com./writeIP.php?txt=";

        private void Awake()
        {
#if UNITY_EDITOR
            imAClient = false;
#elif UNITY_STANDALONE_WIN
            imAClient = false;
#elif UNITY_STANDALONE_MAC
            imAClient = false;
#elif UNITY_ANDROID
            imAClient = true;
#elif UNITY_IPHONE
            imAClient = true;
#endif
        }


        //recupero dall'url specificato in alto l'indirizzo ip contenuto nel file txt se il booleano imAClient è settato su true
        IEnumerator Start()
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                if (imAClient)
                {
                    var ww = new WWW(url);
                    yield return ww;
                    ipInput.text = ww.text;
                    Debug.Log(ww.text);
                    lobbyManager.clientPanel.SetActive(true);
                    lobbyManager.serverPanel.SetActive(false);
                    OnClickJoin();
                }
                else
                {
                    lobbyManager.serverPanel.SetActive(true);
                    lobbyManager.clientPanel.SetActive(false);
                    string serverIpAdress = Network.player.ipAddress.ToString();
                    WWW w = new WWW(URL_to_script + serverIpAdress);
                    yield return w;
                    //Debug.Log("scrivo sul txt online il mio indirizzo ip in qualità di server " + serverIpAdress);
                    OnClickDedicated();
                }
            }

            else
            {
                ipManual.gameObject.SetActive(true);
                lobbyManager.serverPanel.SetActive(false);

                ipManual.onEndEdit.AddListener(onEndEditIP);

                ipInput.text = ipManual.text;
            }           
        }

        public void OnEnable()
        {
            lobbyManager.topPanel.ToggleVisibility(true);
            ipInput.text = Network.player.ipAddress;
            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);
        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            lobbyManager.ChangeTo(lobbyPanel);
            
            lobbyManager.networkAddress = ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickDedicated()
        {
            lobbyManager.ChangeTo(null);
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
        }

        public void OnClickCreateMatchmakingGame()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.matchMaker.CreateMatch(
                matchNameInput.text,
                (uint)lobbyManager.maxPlayers,
                true,
				"", "", "", 0, 0,
				lobbyManager.OnMatchCreate);

            lobbyManager.backDelegate = lobbyManager.StopHost;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
        }

        public void OnClickOpenServerList()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.ChangeTo(lobbyServerList);
        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        void onEndEditGameName(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickCreateMatchmakingGame();
            }
        }

    }
}
