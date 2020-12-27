using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PenguinRun
{
    public class EnvironmentElement : MonoBehaviour
    {
        private Vector3 m_NextPos = new Vector3();

        void Update()
        {
            Move();
        }

        public void Activate(float speed, Vector3 startPos, bool activateThunder = false)
        {
            this.transform.position = startPos;
            m_NextPos = speed * this.transform.right;

            if (activateThunder)
                StartCoroutine(ActivateThunder());
        }

        private void Move()
        {
            this.transform.localPosition -= m_NextPos;
        }

        private IEnumerator ActivateThunder()
        {
            var thunder = this.transform.GetComponentInChildren<ParticleSystem>();
            var randomTime = Random.Range(0.0f, 2f);
            yield return new WaitForSeconds(randomTime);
            thunder.Play();

            yield return new WaitForSeconds(0.85f);
            this.transform.GetComponent<AudioSource>().Play();
        }

    }
}
