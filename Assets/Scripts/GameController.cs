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
            //m_GameState = GameState.Begin;
        }

        private void Update()
        {
            if (CurrentState == GameState.Play && CurrentDifficulty != GameDifficulty.Hard)
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
            while (CurrentState == GameState.Play)
            {
                m_Score++;
                m_GuiManager.SetScore(m_Score.ToString());
                yield return new WaitForSeconds(m_TimeRange);
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
        public void PressedPlayBtn()
        {
            CurrentState = GameState.Play;
        }

        private IEnumerator StartMatch()
        {
            m_PenguinSpriteRenderer.enabled = true;
            CurrentDifficulty = GameDifficulty.Easy;
            yield return new WaitForSeconds(m_GameInitialisationTime);
            StartCoroutine(m_HazardsManager.SetNewHazard());
            StartCoroutine(Timer());
        }

        //-----------------------------------------------------------------------
        //When the player die everything must be stopped
        private void EndMatch()
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
            m_PathManager.ResetManager();
            m_HazardsManager.ResetManager();
            m_EnvironmentManager.ClearBackgorund();
            m_MainCharacter.Reset();
            m_EffectManager.Reset();
            StartCoroutine(StartMatch());
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
                    m_EnvironmentManager.SetupBackground();
                    m_PathManager.SetupPath();
                    m_EffectManager.PlaySnow(true);
                    StartCoroutine(StartMatch());
                    break;
                case GameState.End:
                    EndMatch();
                    m_EffectManager.PlaySnow(true);
                    m_EnvironmentManager.ClearBackgorund();
                    break;
                case GameState.Quit:
                    break;
            }
        }
    }
}