using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PenguinRun
{
    public class GUIManager : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField]private GameObject m_EndView;
        [SerializeField]private GameObject m_StartView;

        [Header("Views Elements")]
        [SerializeField]private TextMeshProUGUI m_ScoreText;
        [SerializeField]private Button m_PauseBtn;

        [SerializeField]private Button m_StartBtn;
        [SerializeField]private Button m_QuitBtn;

        [SerializeField]private Button m_RestartBtn;
        [SerializeField]private Button m_ExitBtn;

        public Action pressedRestartBtn;
        public Action pressedPlayBtn;

        public void Initialise()
        {
            m_PauseBtn.onClick.AddListener(ShowPauseScreen);

            m_StartBtn.onClick.AddListener(Play);
            m_StartBtn.onClick.AddListener(GameController.Instance.Quit);

            m_RestartBtn.onClick.AddListener(RestartGame);
            m_StartBtn.onClick.AddListener(GameController.Instance.Quit);
            m_EndView.SetActive(false);
        }

        public void SetScore(string score)
        {
            m_ScoreText.text = score;
        }

        private void Play()
        {
            m_StartView.SetActive(false);
            pressedPlayBtn?.Invoke();
        }

        public void ShowEndGameScreen()
        {
            m_EndView.SetActive(true);
        }

        private void RestartGame()
        {
            pressedRestartBtn?.Invoke();
            m_EndView.SetActive(false);
        }

        //TO BE CHANGED ONCE SCREENS ARE READY
        private void ShowPauseScreen()
        {
            GameController.Instance.Quit();
        }
    }
}