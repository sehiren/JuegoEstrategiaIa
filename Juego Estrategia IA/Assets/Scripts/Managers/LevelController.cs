using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;

    public GameObject unitPrefab;
    public Grid grid;

    //--------------------------- ELEMENTOS  PARA LOS EQUIPOS -------------------------
    [Header("Player controllers")]
    public PlayerController[] players;

    [Header("Elementos del equipo")]
    [SerializeField] TeamSet[] teamSets;

    [Header("Sprites para cada classe")]
    [SerializeField] private Sprite swordSprite;
    [SerializeField] private Sprite lanceSprite;
    [SerializeField] private Sprite shieldSprite;
    //------------------------------------------------------------------------------------
    public static Sprite SwordSprite { get; set; }
    public static Sprite ShieldSprite { get; set; }
    public static Sprite LanceSprite { get; set; }
    //-------------------------------------------------------------------------------------

    private int _currentTurn;

    private int currentPlayer = 0;
    private Team winnerTeam;

    [Header("Camera reference")]
    [SerializeField] private CameraController _camera;

    [Header("UI references")]
    [SerializeField] private GameObject winnerText;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < players.Length; i++)
            players[i].InitPlayer(teamSets[i]);
        
        StartCoroutine(CoreLoop());
    }

    /// <summary>
    /// Bucle principal del juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoreLoop()
    {
        winnerTeam = null;
        currentPlayer = 0;
        _currentTurn = 0;

        while (!TestEndGameCondition())
        {
            _currentTurn++;
            UpgradeAreaManager.instance.UpdateManager(_currentTurn);

            yield return StartCoroutine(PlayerTurn()); //Player turn       
        }

        winnerText.SetActive(true);
        Text text = winnerText.GetComponentInChildren<Text>();

        if (winnerTeam != null)
        {
            //Mostrar el texto personalizado por color
            string coloredText = "<color=#" + ColorUtility.ToHtmlStringRGB(winnerTeam.teamColor) + ">" + winnerTeam.teamId.ToString().ToUpper()  + " TEAM" + "</color>";
            text.text = coloredText + " WINS";
        }
        else
            text.text = "DRAW";
    }

    /// <summary>
    /// Darle el turno al jugador correspondiente
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayerTurn()
    {
        PlayerController player = players[currentPlayer];
        _camera.SetCameraPosition(player.team.flagPosition.worldPosition);
        player.StartTurn();

        while (player.HasPlayerFinished() && !TestEndGameCondition())
        {
            player.UpdatePlayer();
            yield return null;
        }

        player.FinishTurn();

        currentPlayer++;
        if (currentPlayer >= players.Length)
            currentPlayer = 0;
    }

    /// <summary>
    /// Comprobar condiciones de victoria
    /// </summary>
    /// <returns>Devuelve si la partida debe acabarse</returns>
    private bool TestEndGameCondition()
    {
        bool endGame = false;

        if (winnerTeam != null)
            endGame = true;
        else
        {
            if (OnePlayerLeft())
            {
                winnerTeam = GetWinnerTeam();
                endGame = true;
            }
        }
        return endGame;
    }

    /// <summary>
    /// Comprueba si un jugador o mas de 1 unidad
    /// </summary>
    /// <returns></returns>
    private bool OnePlayerLeft()
    {
        int counter = 0;

        foreach (PlayerController player in players)
            if (player.GetTeamCount() > 0)
                counter++;

        return counter <= 1;
    }

    /// <summary>
    /// Comprobar cual es el ganador
    /// </summary>
    /// <returns>Devuelve al equipo ganador, o null en caso de empate</returns>
    private Team GetWinnerTeam()
    {
        Team winner = null;

        foreach(PlayerController p in players)
        {
            if(p.GetTeamCount() > 0)
            {
                if (winner == null)
                    winner = p.team;
                else
                {
                    winner = null;
                    break;
                }
            }
        }

        return winner;
    }

    /// <summary>
    /// Funcion a llamar cuando una unidad llega a la bandera enemiga
    /// </summary>
    /// <param name="winnerUnit">Unidad ganadora</param>
    public void PlayerInEnemyFlag(Unit winnerUnit)
    {
        winnerTeam = winnerUnit.unitTeam;
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Esta funcion actualiza las referencias  al sprite y el materiial de cada equipo
    /// </summary>
    private void OnValidate()
    {
        SwordSprite = swordSprite;
        ShieldSprite = shieldSprite;
        LanceSprite = lanceSprite;
    }
}
