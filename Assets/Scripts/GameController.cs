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

    public enum PlayerState
    {
        Alive,
        Dead
    }

    public class GameController : MonoBehaviour
    {
        private GameDifficulty m_CurrentDifficulty;

        public GameDifficulty gameDifficulty
        {
            get { return m_CurrentDifficulty; }
            set
            {
                m_CurrentDifficulty = value;
                NotifyManagers();
            }
        }

        private PlayerState m_PlayerState;

        public PlayerState playerState
        {
            get { return m_PlayerState; }
            set
            {
                m_PlayerState = value;
                if (value == PlayerState.Dead)
                {
                    EndGame();
                }
            }
        }

        public static GameController Instance;

        private CharacterController m_MainCharacter;
        private EnvironmentManager m_EnvironmentManager;
        private PlayerInput m_PlayerActionController;
        private GUIManager m_GuiManager;
        private HazardsManager m_HazardsManager;
        private PathManager m_PathManager;
        private EffectManager m_EffectManager;

        //----------------------------------------------------------------
        //Time and Score
        private int m_Score = 0;

        private float m_TimeRange = 0;
        public float m_GameInitialisationTime = 5;
        public int m_InitialisationsTasks = 0;

        //----------------------------------------------------------------
        //Score thresholds to change difficult
        private const int MEDIUM_THRESHOLD = 100;

        private const int HARD_THRESHOLD = 200;

        public float EASY_SPEED = 0.04f;
        public float MEDIUM_SPEED = 0.15f;
        public float HARD_SPEED = 0.3f;

        private GameObject m_Penguin;
        private SpriteRenderer m_PenguinSpriteRenderer;

        private void Awake()
        {
            Instance = this;

#if UNITY_ANDROID
            Screen.orientation = ScreenOrientation.Landscape;
#endif

            Vector2 topRightScreenCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
            m_Penguin = this.transform.Find("Penguin").gameObject;
            m_PenguinSpriteRenderer = m_Penguin.GetComponent<SpriteRenderer>();
            m_PenguinSpriteRenderer.enabled = false;
            m_MainCharacter = m_Penguin.AddComponent<CharacterController>();
            if (m_MainCharacter != null)
            {
                InitialiseControls();
            }

            var environmentTransform = this.transform.Find("Environment");

            m_EnvironmentManager = environmentTransform.gameObject.AddComponent<EnvironmentManager>();
            if (m_EnvironmentManager == null)
                Debug.LogError("EnvironmentManagerNull");
            m_EnvironmentManager.Initialise(topRightScreenCorner.x);

            Vector3 penguinPos = m_Penguin.transform.position; ;
            float penguinWidth = m_Penguin.GetComponent<BoxCollider2D>().size.x;
            m_HazardsManager = environmentTransform.gameObject.AddComponent<HazardsManager>();
            m_HazardsManager.Initialise(penguinPos, penguinWidth, topRightScreenCorner.x);

            m_GuiManager = this.transform.Find("GUI").gameObject.AddComponent<GUIManager>();
            m_GuiManager.Initialise();

            m_EffectManager = this.transform.Find("ParticleEffects&Lights").gameObject.AddComponent<EffectManager>();
            m_EffectManager.Initialise(topRightScreenCorner, penguinPos);

            m_PathManager = environmentTransform.gameObject.AddComponent<PathManager>();
            m_PathManager.Initialise(topRightScreenCorner.x);
        }

        private void Update()
        {
            if (m_CurrentDifficulty != GameDifficulty.Hard)
                CheckScore();
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
        private IEnumerator Timer()
        {
            while (m_PlayerState == PlayerState.Alive)
            {
                m_Score++;
                m_GuiManager.SetScore(m_Score.ToString());
                yield return new WaitForSeconds(m_TimeRange);
            }
        }

        private void SetTimeRange()
        {
            switch (m_CurrentDifficulty)
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
            if (m_CurrentDifficulty == GameDifficulty.Easy)
            {
                if (m_Score > MEDIUM_THRESHOLD)
                    m_CurrentDifficulty = GameDifficulty.Medium;
            }
            else if (m_CurrentDifficulty == GameDifficulty.Medium)
            {
                if (m_Score > HARD_THRESHOLD)
                    m_CurrentDifficulty = GameDifficulty.Hard;
            }
        }

        public ParticleSystem GetThunder()
        {
            return m_EnvironmentManager.GetActiveCloud();
        }

        //-----------------------------------------------------------------------
        //These functions controll player interactions whit GUI
        public void PressedPlayBtn()
        {
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            m_PenguinSpriteRenderer.enabled = true;
            while (!m_EnvironmentManager.m_Ready || !m_PathManager.m_Ready)
            {
                yield return new WaitForEndOfFrame();
            }

            playerState = PlayerState.Alive;
            gameDifficulty = GameDifficulty.Easy;
            StartCoroutine(Timer());

            while (!m_MainCharacter.m_Ready)
            {
                yield return new WaitForEndOfFrame();
            }
            StartCoroutine(m_HazardsManager.SetNewHazard());
        }

        //-----------------------------------------------------------------------
        //When the player die everything must be stopped
        private void EndGame()
        {
            //Stop the game
            Time.timeScale = 0;
            m_PenguinSpriteRenderer.enabled = false;        // Disabling the penguin
            m_EnvironmentManager.Stop();                    // Stop the background
            m_PathManager.Stop();                           // Stop the paths
            m_HazardsManager.Stop();                        //Stop the hazards
            m_EffectManager.Stop();                         // Stop the effects
            m_GuiManager.ShowEndGameScreen();               // Show end game screen
        }

        public void Quit()
        {
            Application.Quit();
        }

        //-----------------------------------------------------------------------
        //Reset the managers
        public void Restart()
        {
            Time.timeScale = 1;
            m_PenguinSpriteRenderer.enabled = true;
            m_PathManager.Reset();
            m_HazardsManager.Reset();
            m_EnvironmentManager.Reset();
            m_MainCharacter.Reset();
            m_EffectManager.Reset();
            StartCoroutine(StartGame());
        }

        //-----------------------------------------------------------------------
        //Notify the managers when a game state change occurs
        private void NotifyManagers()
        {
            if (m_CurrentDifficulty != GameDifficulty.Easy)                          //The Hazard manager has to be notified later when the game starts
                m_HazardsManager.IncreaseElementsSpeed();

            m_EnvironmentManager.IncreaseElementsSpeed();
            m_PathManager.IncreasePathsSpeed();
            m_EffectManager.SetEffectsSpeed();                                      //Inrcrease snow speed
            m_MainCharacter.SetWalkSpeed();                                         //Increase main character animation speed
            m_HazardsManager.SetHazardCount();                                      //Increase number of different hazards
            SetTimeRange();                                                         //Increase time to calculate score
        }
    }
}