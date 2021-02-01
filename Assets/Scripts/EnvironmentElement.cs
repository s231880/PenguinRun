using UnityEngine;

namespace PenguinRun
{
    public class EnvironmentElement : MonoBehaviour
    {
        private Vector3 m_NextPos = new Vector3();

        private void Update()
        {
            Move();
        }

        public void Activate(float speed, Vector3 startPos)
        {
            this.transform.position = startPos;
            m_NextPos = speed * this.transform.right;
        }

        private void Move()
        {
            this.transform.localPosition -= m_NextPos;
        }

        public void IncreaseSpeed(float speed)
        {
            m_NextPos = speed * this.transform.right;
        }

        public void Stop()
        {
            m_NextPos = Vector3.zero;
        }
    }
}