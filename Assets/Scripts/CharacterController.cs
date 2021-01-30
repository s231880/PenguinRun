using UnityEngine;

namespace PenguinRun
{
    public class CharacterController : MonoBehaviour
    {

        private Animator m_Animator;
        private ParticleSystem m_DustParticleSystem;
        [SerializeField] private Animation m_WalkAnimation;

        private float m_CurrentSpeed = 0;
        private float m_WalkSpeed = 0;
        private const float EASY_WALK_SPEED = 1f;
        private const float MEDIUM_WALK_SPEED = 1.5f;
        private const float HARD_WALK_SPEED = 2f;
        public bool m_Ready = false;

        private void Awake()
        {
            m_Animator = gameObject.transform.GetComponent<Animator>();
            m_DustParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        }

        private void Update()
        {
            Debug.LogError($"{m_Animator.GetBool("isJumping")}");
        }

        public void Jump(bool state)
        {
            m_Animator.SetBool("isJumping", state);
        }

        public void JumpDone(bool state)
        {
            m_Animator.SetBool("isJumping", !state);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Hazard")
            {
                //GameController.Instance.playerState = PlayerState.Dead;
            }
        }

        private void PlayDustAnimation()
        {
            m_DustParticleSystem.Play();
        }

        public void SetWalkSpeed()
        {
            switch (GameController.Instance.gameDifficulty)
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
            if(walkSpeed != m_CurrentSpeed)
            {
                this.Create<ValueTween>(GameController.Instance.m_GameInitialisationTime, EaseType.Linear, () =>
                {
                    //Once the speed has increased to the maximum, inform the GameController
                    m_Ready = true;
                    m_CurrentSpeed = walkSpeed;
                }).Initialise(m_CurrentSpeed, walkSpeed, (f) =>
                {
                    m_Animator.speed = f;//=>Check if works
                });
            }
        }
    }
}
