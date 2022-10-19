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
        [SerializeField]private TextMeshProUGUI m_FinalScore;
        
        [SerializeField]private Button m_PauseBtn;

        [SerializeField]private Button m_StartBtn;
        [SerializeField]private Button m_QuitBtn;

        [SerializeField]private Button m_RestartBtn;
        [SerializeField]private Button m_ExitBtn;


        public Action pressedRestartBtn;
        public Action pressedPlayBtn;

        public void Initialise()
        {
            //m_PauseBtn.onClick.AddListener(ShowPauseScreen);

            m_StartBtn.onClick.AddListener(Play);
            m_QuitBtn.onClick.AddListener(GameController.Instance.Quit);

            m_RestartBtn.onClick.AddListener(RestartGame);
            m_ExitBtn.onClick.AddListener(GameController.Instance.Quit);

            m_EndView.SetActive(false);
            ShowView(true, true);
        }

        public void SetScore(string score)
        {
            m_ScoreText.text = score;
        }

        private void Play()
        {
            ShowView(false,true, () =>
            {
                m_StartView.SetActive(false);
                pressedPlayBtn?.Invoke();
            });
        }

        public void ShowEndGameScreen()
        {
            m_EndView.SetActive(true);
            m_FinalScore.text = "Your score was:\n" + m_ScoreText.text;
            ShowView(true, false);
        }

        private void RestartGame()
        {
            ShowView(false, false, () =>
            {
                m_EndView.SetActive(false);
                pressedRestartBtn?.Invoke();
            });
        }

        private void ShowView(bool show,bool startView, Action callback = null)
         {

            float startingPos = show? -200f: 54f;
            float endPos = show? 54f : -200f;

            RectTransform startBtnRect;
            RectTransform quitBtnRect;

            if (startView)
            {
                startBtnRect = m_StartBtn.GetComponent<RectTransform>();
                quitBtnRect = m_ExitBtn.GetComponent<RectTransform>();
            }
            else
            {
                startBtnRect = m_RestartBtn.GetComponent<RectTransform>();
                quitBtnRect = m_QuitBtn.GetComponent<RectTransform>();
            }

            Vector2 startBtnPos = startBtnRect.anchoredPosition;
            Vector2 quitBtnPos = quitBtnRect.anchoredPosition;

            this.Create<ValueTween>(1.2f, EaseType.ExpoInOut, () =>
            {
                
                callback?.Invoke();
            }).Initialise(startingPos,endPos, (f) =>
            {
                startBtnPos.y = f;
                startBtnRect.anchoredPosition = startBtnPos;

                quitBtnPos.y = f;
                quitBtnRect.anchoredPosition = quitBtnPos;
            });
        }
    }
}