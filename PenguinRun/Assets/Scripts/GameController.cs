using System.Collections;
using UnityEngine;

namespace PenguinRun
{
    public enum GameDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum GameState
    {
        Begin,
        Play,
        End,
        Quit
    }

    public class GameController : MonoBehaviour
    {
        //----------------------------------------------------------------
        private GameDifficulty m_CurrentDifficulty;
        public GameDifficulty CurrentDifficulty
        {
            get { return m_CurrentDifficulty; }
            set
            {
                m_CurrentDifficulty = value;
                ChangeDifficulty();
            }
        }

        private GameState m_CurrentState;
        public GameState CurrentState
        {
            get { return m_CurrentState; }
            set
            {
                m_CurrentState = value;
                ChangeGameState();
            }
        }
        //----------------------------------------------------------------

        public static GameController Instance;
        [Header("Managers")]

        [SerializeField] private CharacterController m_MainCharacter;
        [SerializeField]private EnvironmentManager m_EnvironmentManager;
        [SerializeField]private GUIManager m_GuiManager;
        [SerializeField]private HazardsManager m_HazardsManager;
        [SerializeField]private PathManager m_PathManager;
        [SerializeField]private EffectManager m_EffectManager;

        private PlayerInput m_PlayerActionController;
        //----------------------------------------------------------------
        //Time and Score
        private int m_Score = 0;

        private float m_TimeRange = 0f;
        public float m_GameInitialisationTime = 5f;
        //----------------------------------------------------------------
        //Score thresholds to change difficult
        private const int MEDIUM_THRESHOLD = 100;
        private const int HARD_THRESHOLD = 200;

        private const float EASY_SPEED = 8f;
        private const float MEDIUM_SPEED = 16f;
        private const float HARD_SPEED = 24f;
        private float m_CurrentSpeed = 0f;

        [SerializeField] private GameObject m_Penguin;
        private SpriteRenderer m_PenguinSpriteRenderer;
        private void Awake()
        {
            Instance = this;

#if UNITY_ANDROID
            Screen.orientation = ScreenOrientation.Landscape;
#endif
            Vector2 topRightScreenCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
            m_PenguinSpriteRenderer = m_Penguin.GetComponent<SpriteRenderer>();
            m_PenguinSpriteRenderer.enabled = false;
            if (m_MainCharacter != null)
            {
                InitialiseControls();
            }
            m_MainCharacter.playerHit += PlayerHit;

            m_EnvironmentManager.Initialise(topRightScreenCorner.x);

            Vector3 penguinPos = m_Penguin.transform.position; ;
            float penguinWidth = m_Penguin.GetComponent<BoxCollider2D>().size.x;
            m_HazardsManager.Initialise(penguinPos, penguinWidth, topRightScreenCorner.x);

            m_GuiManager.Initialise();
            m_GuiManager.pressedPlayBtn += PressedPlayBtn;
            m_GuiManager.pressedRestartBtn += Restart;

            m_EffectManager.Initialise(topRightScreenCorner, penguinPos);

            m_PathManager.Initialise(topRightScreenCorner.x);
        }

        private void OnEnable()
        {
            m_PlayerActionController.Enable();
        }

        private void InitialiseControls()
        {
            m_PlayerActionController = new PlayerInput();
            m_PlayerActionController.Action.Jump.performed += ctx => m_MainCharacter.Jump(true);
            m_PlayerActionController.Action.Jump.canceled += ctx => m_MainCharacter.Jump(false);
        }

        //Timer to set the player score
        private IEnumerator StartTimer()
        {
            m_Score = 0;
            m_GuiManager.SetScore(m_Score.ToString());
            while (CurrentState == GameState.Play)
            {
                yield return new WaitForSeconds(m_TimeRange);
                m_Score++;
                m_GuiManager.SetScore(m_Score.ToString());
                
                if (CurrentDifficulty != GameDifficulty.Hard)
                    CheckScore();
            }
        }

        private void SetTimeRange()
        {
            switch (CurrentDifficulty)
            {
                case GameDifficulty.Easy:
                    m_TimeRange = 1f;
                    break;

                case GameDifficulty.Medium:
                    m_TimeRange = 0.7f;
                    break;

                case GameDifficulty.Hard:
                    m_TimeRange = 0.3f;
                    break;
            }
        }

        //Check the actual score to increase difficulty level
        private void CheckScore()
        {
            if (CurrentDifficulty == GameDifficulty.Easy)
            {
                if (m_Score > MEDIUM_THRESHOLD)
                    CurrentDifficulty = GameDifficulty.Medium;
            }
            else if (CurrentDifficulty == GameDifficulty.Medium)
            {
                if (m_Score > HARD_THRESHOLD)
                    CurrentDifficulty = GameDifficulty.Hard;
            }
        }

        public ParticleSystem GetThunder()
        {
            return m_EnvironmentManager.GetActiveCloud();
        }

        //-----------------------------------------------------------------------
        //These functions controll player interactions whit GUI
        private void PressedPlayBtn()
        {
            CurrentState = GameState.Play;
        }

        private void StartMatch()
        {
            m_EnvironmentManager.SetupBackground();
            m_PathManager.SetupPath();
            m_EffectManager.PlaySnow(true);
            m_PenguinSpriteRenderer.enabled = true;
            CurrentDifficulty = GameDifficulty.Easy;
            StartCoroutine(m_HazardsManager.SetNewHazard());
            StartCoroutine(StartTimer());
        }

        //-----------------------------------------------------------------------
        //When the player die everything must be stopped
        private void PlayerHit()
        {
            CurrentState = GameState.End;
        }

        private void EndMatch()
        {
            m_PenguinSpriteRenderer.enabled = false;

            m_EnvironmentManager.Stop();
            m_PathManager.Stop();
            m_HazardsManager.Stop();
            m_EffectManager.Stop();
            StopAllCoroutines();

            m_GuiManager.ShowEndGameScreen();
        }

        public void Quit()
        {
            Application.Quit();
        }

        //-----------------------------------------------------------------------
        //Reset the managers
        private void Restart()
        {
            Time.timeScale = 1;
            m_PenguinSpriteRenderer.enabled = true;

            m_MainCharacter.Reset();
            m_EffectManager.Reset();

            CurrentState = GameState.Play;
        }

        //-----------------------------------------------------------------------
        //Notify the managers when a game state change occurs
        public void ChangeDifficulty()
        {
            switch (CurrentDifficulty)
            {
                case GameDifficulty.Easy:
                    m_CurrentSpeed = EASY_SPEED;
                    break;
                case GameDifficulty.Medium:
                    m_CurrentSpeed = MEDIUM_SPEED;
                    break;
                case GameDifficulty.Hard:
                    m_CurrentSpeed = HARD_SPEED;
                    break;
            }

            if (CurrentDifficulty != GameDifficulty.Easy)                          //The Hazard manager has to be notified later when the game starts
                m_HazardsManager.IncreaseElementsSpeed(m_CurrentSpeed);

            m_EnvironmentManager.IncreaseElementsSpeed(m_CurrentSpeed);
            m_PathManager.IncreasePathsSpeed(m_CurrentSpeed);
            m_EffectManager.UpdateSpeed(m_CurrentSpeed);                            //Inrcrease snow speed
            m_MainCharacter.SetWalkSpeed();                                         //Increase main character animation speed
            m_HazardsManager.SetHazardCount();                                      //Increase number of different hazards
            SetTimeRange();                                                         //Increase time to calculate score
        }

        private void ChangeGameState()
        {
            switch (CurrentState)
            {
                case GameState.Begin:
                    break;
                case GameState.Play:
                    StartMatch();
                    break;
                case GameState.End:
                    EndMatch();
                    break;
                case GameState.Quit:
                    break;
            }
        }
    }
}