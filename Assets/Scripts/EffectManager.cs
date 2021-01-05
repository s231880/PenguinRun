using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PenguinRun
{
    //This script controls particle systems, audio & light effects
    public class EffectManager : MonoBehaviour
    {
        private ParticleSystem m_Snow;
        private List<ParticleSystem> m_Winds = new List<ParticleSystem>();
        private AudioSource m_ThunderAudioSource;
        private AudioSource m_WindAudioSource;
        private AudioClip m_ThunderSound;
        private List<AudioClip> m_WindsSounds = new List<AudioClip>();
        private Image m_Background;
        private Light m_MainLight;
        private ObjectPoolManager m_SunRayPool;
        public float m_SnowStormSpeed;


        private const float MAX_BACKGROUND_ALPHA = 255;
        private const float NORMAL_BACKGROUND_ALPHA = 130;
        private const int SUN_RAYS_PREFAB= 3;
        private const int WIND_SOUNDS = 2;

       public void Initialise()
        {
            m_Background = this.transform.parent.Find("GUI").GetComponentInChildren<Image>();

            InitialiseParticleSystems();
            InitialiseAudio();
            InitialiseSunRaysPool();
        }
        void Update()
        {

        }

        private void InitialiseAudio()
        {
            var sources = Camera.main.GetComponents<AudioSource>();
            m_ThunderAudioSource = sources[0];
            m_WindAudioSource = sources[1];

            for (int i = 1; i <= WIND_SOUNDS; ++i)
            {
                var windSound = Resources.Load<AudioClip>($"AudioClip/WindWhistle{i}_");
                m_WindsSounds.Add(windSound);
            }
            m_ThunderSound = Resources.Load<AudioClip>($"AudioClip/Thunder");
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
            m_SunRayPool.CreateObjPool(sunRaysPrefab, 10, pool.transform, activeSunRays.transform);
        }

        private void InitialiseParticleSystems()
        {
            m_Snow = this.transform.Find("Snow").GetComponent<ParticleSystem>();

            var wind1 = this.transform.Find("Wind1").GetComponent<ParticleSystem>();
            m_Winds.Add(wind1);
            var wind2 = this.transform.Find("Wind2").GetComponent<ParticleSystem>();
            m_Winds.Add(wind2);
        }
        // TO BE TESTED
        private void ActivateWind(bool playBoth)
        {
            int randomSound = Random.Range(0, 1);
            Debug.LogError($"PlaySound{randomSound}");
            m_WindAudioSource.clip = m_WindsSounds[randomSound];
            m_WindAudioSource.Play();

            if (!playBoth)
            {
                int randomWind = Random.Range(0, 1);
                Debug.LogError($"PlayWind{randomWind}");
                m_Winds[randomWind].Play();
            }
            else
            {
                foreach (var wind in m_Winds)
                    wind.Play();
            }
            
        }

        private IEnumerator PlayThunder()
        {
            var thunder = GameController.Instance.GetThunder();
            thunder.Play();

            //Change background light: TO DO => Use twins


            var randomTime = Random.Range(0.5f, 0.9f);
            yield return new WaitForSeconds(randomTime);
            m_ThunderAudioSource.clip = m_ThunderSound;
            m_ThunderAudioSource.Play();
        }

        //TO DO
        public void SetSnowStormSpeed(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    break;
                case GameDifficulty.Medium:
                    break;
                case GameDifficulty.Hard:
                    break;
            }
        }
    }
}
