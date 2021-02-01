using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PenguinRun
{
    public class GUIManager : MonoBehaviour
    {
        private TextMeshProUGUI m_ScoreText;
        private Button m_PauseBtn;

        private GameObject m_StartView;
        private Button m_StartBtn;
        private Button m_QuitBtn;

        private GameObject m_EndView;
        private Button m_RestartBtn;
        private Button m_ExitBtn;

        public void Initialise()
        {
            m_ScoreText = this.transform.GetComponentInChildren<TextMeshProUGUI>();
            m_PauseBtn = this.transform.Find("PauseBtn").GetComponent<Button>();
            m_PauseBtn.onClick.AddListener(ShowPauseScreen);

            m_StartView = this.transform.Find("StartView").gameObject;
            m_StartBtn = m_StartView.transform.Find("StartBtn").GetComponent<Button>();
            m_StartBtn.onClick.AddListener(Play);
            m_QuitBtn = m_StartView.transform.Find("CloseBtn").GetComponent<Button>();
            m_StartBtn.onClick.AddListener(GameController.Instance.Quit);

            m_EndView = this.transform.Find("EndView").gameObject;
            m_RestartBtn = m_EndView.transform.Find("RestartBtn").GetComponent<Button>();
            m_RestartBtn.onClick.AddListener(RestartGame);
            m_ExitBtn = m_EndView.transform.Find("QuitBtn").GetComponent<Button>();
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
            GameController.Instance.PressedPlayBtn();
        }

        public void ShowEndGameScreen()
        {
            m_EndView.SetActive(true);
        }

        private void RestartGame()
        {
            GameController.Instance.Restart();
            m_EndView.SetActive(false);
        }

        //TO BE CHANGED ONCE SCREENS ARE READY
        private void ShowPauseScreen()
        {
            GameController.Instance.Quit();
        }
    }
}