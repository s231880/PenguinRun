using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace PenguinRun
{
    public class GUIManager : MonoBehaviour
    {
        private TextMeshProUGUI m_ScoreText;
        private Button m_PauseBtn;
        public void Initialise()
        {
            m_ScoreText = this.transform.GetComponentInChildren<TextMeshProUGUI>();
            m_PauseBtn = this.transform.Find("PauseBtn").GetComponent<Button>();
            m_PauseBtn.onClick.AddListener(GameController.Instance.Quit);
        }
        public void SetScore(string score)
        {
            m_ScoreText.text = score;
        }

        public void ShowEndGameScreen() { }
    }
}
