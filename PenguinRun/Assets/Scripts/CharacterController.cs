using System;
using UnityEngine;

namespace PenguinRun
{
    public class CharacterController : MonoBehaviour
    {
        private Animator m_Animator;
        //private ParticleSystem m_DustParticleSystem;

        private float m_WalkSpeed = 0;
        private const float EASY_WALK_SPEED = 1f;
        private const float MEDIUM_WALK_SPEED = 1.5f;
        private const float HARD_WALK_SPEED = 2f;
        public bool m_Ready = false;

        public Action playerHit;

        private void Awake()
        {
            m_Animator = gameObject.transform.GetComponent<Animator>();
            //m_DustParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        }

        public void Jump()
        {
            m_Animator.SetTrigger("isJumping");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Hazard")
            {
                playerHit?.Invoke();
            }
        }

        public void SetWalkSpeed()
        {
            switch (GameController.Instance.CurrentDifficulty)
            {
                case GameDifficulty.Easy:
                    IncrementWalkSpeed(EASY_WALK_SPEED);
                    break;

                case GameDifficulty.Medium:
                    IncrementWalkSpeed(MEDIUM_WALK_SPEED);
                    break;

                case GameDifficulty.Hard:
                    IncrementWalkSpeed(HARD_WALK_SPEED);
                    break;
            }
        }

        private void IncrementWalkSpeed(float walkSpeed)
        {
            if (walkSpeed != m_WalkSpeed)
            {
                this.Create<ValueTween>(0.5f, EaseType.Linear, () =>
                {
                    //Once the speed has increased to the maximum, inform the GameController
                    m_Ready = true;
                    m_WalkSpeed = walkSpeed;
                }).Initialise(m_WalkSpeed, walkSpeed, (f) =>
                {
                    m_Animator.speed = f;
                });
            }
        }

        public void Reset()
        {
            m_Animator.Rebind();
        }
    }
}