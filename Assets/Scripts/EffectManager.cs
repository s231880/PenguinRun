using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PenguinRun
{
    //This script controls particle systems, audio & light effects
    //Background music from PlayOnLoop.com  => https://www.playonloop.com/
    //Licensed under Creative Commons by Attribution 4.0

    public class EffectManager : MonoBehaviour
    {
        private float m_ScreenLimit;
        private ParticleSystem m_Snow;
        public float m_SnowStormSpeed;
        private AudioClip m_SoundTrack;
        private AudioSource m_SoundTrackSource;

        //-----------------------------------------------------------------------
        //Thunder effect variables
        private const string THUNDER = "Thunder";

        private AudioClip m_ThunderSound;
        private AudioSource m_ThunderAudioSource;
        private const int THUNDER_TIME_LAPSE = 20;
        private ParticleSystem m_Thunder;

        //-----------------------------------------------------------------------
        //Wind effect variables
        private const int WIND_TIME_LAPSE = 15; // Change name

        private const string WIND = "Wind";
        private List<ParticleSystem> m_Winds = new List<ParticleSystem>();
        private AudioSource m_WindAudioSource;
        private List<AudioClip> m_WindsSounds = new List<AudioClip>();
        private const int WIND_SOUNDS = 2;
        private int m_PlayBothEffects;

        //-----------------------------------------------------------------------
        //Sun Rays effect variables
        private const int MAX_RAYS_TIME_LAPSE = 25;

        private const string SUN_RAY = "SunRay";
        private ObjectPoolManager m_SunRayPool;
        private Vector3 m_TopRightScreenCorner = Vector3.zero;
        private const float SUN_RAYS_OFFSET = 1.5f;
        private const int SUN_RAYS_PREFAB = 3;

        //To each sun ray is attached an invisible cube at the end of it, when it reaches the end of the screen
        //the ray is removed
        private GameObject m_RayLimit = null;

        private EnvironmentElement m_ActiveRay;
        public float m_SunRaySpeed;
        public bool m_IsPlayerAlive = true;

        //-----------------------------------------------------------------------
        //I need two variables, the first one starts the coroutine that leads to the effect attivation
        //after a random amount of time, the second one keeps track of when the effect is actually played
        private bool m_IsWindActive = false;

        private bool m_IsWindPlaying = false;

        private bool m_IsThunderActive = false;
        private bool m_IsThunderPlaying = false;

        private bool m_IsSunRayActive = false;
        private bool m_IsSunRayPlaying = false;

        //-----------------------------------------------------------------------
        private void Update()
        {
            if (m_IsPlayerAlive)
                UpdateEffects();
        }

        //-----------------------------------------------------------------------
        //Initialise Functions

        public void Initialise(Vector3 topRightScreenCornerX, Vector3 penguinPos)
        {
            m_ScreenLimit = -topRightScreenCornerX.x;
            SetInitialSunRayPosition(topRightScreenCornerX);
            InitialiseParticleSystems();
            InitialiseAudio();
            InitialiseSunRaysPool();
        }

        private void InitialiseAudio()
        {
            var sources = Camera.main.GetComponents<AudioSource>();
            m_ThunderAudioSource = sources[0];
            m_WindAudioSource = sources[1];
            m_SoundTrackSource = sources[2];

            for (int i = 1; i <= WIND_SOUNDS; ++i)
            {
                var windSound = Resources.Load<AudioClip>($"AudioClip/WindWhistle{i}_");
                m_WindsSounds.Add(windSound);
            }
            m_ThunderSound = Resources.Load<AudioClip>($"AudioClip/Thunder");
            m_SoundTrack = Resources.Load<AudioClip>($"AudioClip/Soundtrack");
            m_SoundTrackSource.clip = m_SoundTrack;
#if UNITY_EDITOR
            m_SoundTrackSource.playOnAwake = false;
            m_SoundTrackSource.loop = false;
            m_SoundTrackSource.Stop();
#else

            m_SoundTrackSource.playOnAwake = true;
            m_SoundTrackSource.loop = true;
            m_SoundTrackSource.Play();
#endif
        }

        private void InitialiseSunRaysPool()
        {
            List<GameObject> sunRaysPrefab = new List<GameObject>();
            for (int i = 1; i <= SUN_RAYS_PREFAB; ++i)
            {
                var sunRay = Resources.Load<GameObject>($"Prefabs/Environment/SunRay/SunRay{i}");
                sunRaysPrefab.Add(sunRay);
            }

            var pool = new GameObject("SunRay");
            pool.transform.SetParent(this.transform);
            var activeSunRays = new GameObject($"ActiveSunRays");
            activeSunRays.transform.SetParent(this.transform);
            m_SunRayPool = pool.AddComponent<ObjectPoolManager>();
            m_SunRayPool.CreateObjPool(sunRaysPrefab, 2, pool.transform, activeSunRays.transform);
        }

        private void InitialiseParticleSystems()
        {
            m_Snow = this.transform.Find("Snow").GetComponent<ParticleSystem>();

            var wind1 = this.transform.Find("Wind1").GetComponent<ParticleSystem>();
            m_Winds.Add(wind1);
            var wind2 = this.transform.Find("Wind2").GetComponent<ParticleSystem>();
            m_Winds.Add(wind2);
        }

        //-------------------------------------------------------------------------------
        //Set the sunray starting pos as the topright screen corner, and give a little offset to the y
        private void SetInitialSunRayPosition(Vector3 topRightScreenCornerX)
        {
            m_TopRightScreenCorner = topRightScreenCornerX;
            m_TopRightScreenCorner.y += SUN_RAYS_OFFSET;
        }

        private void UpdateEffects()
        {
            if (!m_IsSunRayActive)
            {
                StartCoroutine(ActivateSunRay(SetRandomTime(MAX_RAYS_TIME_LAPSE)));
                m_IsSunRayActive = true;
            }
            else if (m_IsSunRayPlaying)
            {
                if (!IsRayStillVisible())
                    ResetEffectVariable(SUN_RAY);
            }

            if (!m_IsWindActive)
            {
                StartCoroutine(ActivateWind(SetRandomTime(WIND_TIME_LAPSE)));
                m_IsWindActive = true;
            }
            else if (m_IsWindPlaying)
            {
                if (HasWindBeenPlayed())
                {
                    ResetEffectVariable(WIND);
                }
            }

            if (!m_IsThunderActive)
            {
                StartCoroutine(PlayThunder(SetRandomTime(THUNDER_TIME_LAPSE)));
                m_IsThunderActive = true;
            }
            else if (m_IsThunderPlaying)
            {
                if (HasThunderBeenPlayed())
                {
                    PlaySounds(THUNDER);
                    ResetEffectVariable(THUNDER);
                }
            }
        }

        //-----------------------------------------------------------------------
        //TO DO
        public void SetEffectsSpeed()
        {
            switch (GameController.Instance.gameDifficulty)
            {
                case GameDifficulty.Easy:
                    m_SunRaySpeed = GameController.Instance.EASY_SPEED;
                    break;

                case GameDifficulty.Medium:
                    m_SunRaySpeed = GameController.Instance.MEDIUM_SPEED;
                    break;

                case GameDifficulty.Hard:
                    m_SunRaySpeed = GameController.Instance.HARD_SPEED;
                    break;
            }
        }

        //There could be multiple wind effects enabled ad the same time
        private int HowManyWindEffectActivate()
        {
            int randomVariable = Random.Range(0, 10);
            if (randomVariable <= 5)
                return 0;
            else
                return 1;
        }

        //-----------------------------------------------------------------------
        //Sun rays effect functions
        private IEnumerator ActivateSunRay(float time)
        {
            yield return new WaitForSeconds(time);
            EnvironmentElement ray = m_SunRayPool.GetObject().GetComponent<EnvironmentElement>();
            if (ray != null)
            {
                ray.Activate(m_SunRaySpeed, m_TopRightScreenCorner);
                m_ActiveRay = ray;
                m_RayLimit = ray.gameObject.transform.Find("Limit").gameObject;
                m_IsSunRayPlaying = true;
            }
        }

        private bool IsRayStillVisible()
        {
            //If the ray has exit the screen return it to the pool
            if (m_RayLimit.transform.position.x < m_ScreenLimit)
            {
                ReturnRay();
                return false;
            }
            else
                return true;
        }

        private void ReturnRay()
        {
            m_SunRayPool.ReturnObjectToThePool(m_ActiveRay.gameObject);
            m_RayLimit = null;
            m_ActiveRay = null;
        }

        //-----------------------------------------------------------------------
        //Wind effect functions
        private IEnumerator ActivateWind(float time)
        {
            yield return new WaitForSeconds(time);
            m_PlayBothEffects = HowManyWindEffectActivate();

            if (m_PlayBothEffects == 1)
            {
                int randomWind = Random.Range(0, WIND_SOUNDS);
                m_Winds[randomWind].Play();
            }
            else
            {
                foreach (var wind in m_Winds)
                    wind.Play();
            }
            PlaySounds(WIND);
            m_IsWindPlaying = true;
        }

        //If one of the effect is still playing the function returns false, otherwise return true
        private bool HasWindBeenPlayed()
        {
            foreach (var wind in m_Winds)
            {
                if (wind.isPlaying)
                    return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------
        //Thunder effect functions
        private IEnumerator PlayThunder(float time)
        {
            yield return new WaitForSeconds(time);
            m_Thunder = GameController.Instance.GetThunder();
            if (m_Thunder != null)
            {
                m_Thunder.Play();
                m_IsThunderPlaying = true;
            }
            else
            {
                ResetEffectVariable(THUNDER);
            }
        }

        private bool HasThunderBeenPlayed()
        {
            if (!m_Thunder.isPlaying)
                return false;
            else
                return true;
        }

        //-----------------------------------------------------------------------
        private void PlaySounds(string type)
        {
            switch (type)
            {
                case WIND:
                    int randomSound = Random.Range(0, WIND_SOUNDS);
                    m_WindAudioSource.clip = m_WindsSounds[randomSound];
                    m_WindAudioSource.Play();
                    break;

                case THUNDER:
                    m_ThunderAudioSource.clip = m_ThunderSound;
                    m_ThunderAudioSource.Play();
                    break;
            }
        }

        //-----------------------------------------------------------------------
        //Once the effect has been playied the variables are reset
        private void ResetEffectVariable(string effect)
        {
            switch (effect)
            {
                case SUN_RAY:
                    m_IsSunRayActive = false;
                    m_IsSunRayPlaying = false;
                    break;

                case WIND:
                    m_IsWindActive = false;
                    m_IsWindPlaying = false;
                    break;

                case THUNDER:
                    m_IsThunderActive = false;
                    m_IsThunderPlaying = false;
                    m_Thunder = null;
                    break;
            }
        }

        private float SetRandomTime(float maxValue, float minValue = 0)
        {
            return Random.Range(minValue, maxValue);
        }

        //-----------------------------------------------------------------------
        //Increase effects speed functions
        public void IncreaseEffectsSpeed(float newSpeed)
        {
            IncreaseSunRaySpeed(newSpeed);
        }

        private void IncreaseSunRaySpeed(float newSpeed)
        {
            this.Create<ValueTween>(GameController.Instance.m_GameInitialisationTime, EaseType.Linear, () =>
            {
                m_SunRaySpeed = newSpeed;
            }).Initialise(m_SunRaySpeed, newSpeed, (f) =>
            {
                if (m_ActiveRay != null)
                    m_ActiveRay.IncreaseSpeed(f);
            });
        }

        //-----------------------------------------------------------------------
        //Stop the effects activation and if there is a sun ray active
        public void Stop()
        {
            m_IsPlayerAlive = false;
            if (m_ActiveRay != null)
                m_ActiveRay.Stop();

            foreach (var wind in m_Winds)
                wind.Stop();
            
        }

        //-----------------------------------------------------------------------
        //Reset Manager when game ends
        public void Reset()
        {
            if (m_ActiveRay != null)
                ReturnRay();
            m_IsPlayerAlive = true;
            ResetEffectVariable(THUNDER);
            ResetEffectVariable(SUN_RAY);
            ResetEffectVariable(WIND);
        }
    }
}