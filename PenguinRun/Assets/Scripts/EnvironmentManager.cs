using System.Collections.Generic;
using UnityEngine;

//This script controls the background element such as Mountains, Icebergs and clouds
namespace PenguinRun
{
    public class EnvironmentManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------
        //Elements Lists used to initialise Object Pools
        private Dictionary<string, List<GameObject>> m_ElementsDictionary = new Dictionary<string, List<GameObject>>();

        private const int NUM_OF_MOUNTAIN_SPRITES = 6;
        private const int NUM_OF_ICEBERG_SPRITES = 7;
        private const int NUM_OF_CLOUD_SPRITES = 3;
        private float m_BottomRightScreenCornerX;
        private const int NUM_OF_ELEMENT_PER_TYPE = 10;


        //-----------------------------------------------------------------------
        //Object Pools & List used to manage the activation/disactivation
        private const string MOUNTAIN = "Mountain";
        private const string ICEBERG = "Iceberg";
        private const string CLOUD = "Cloud";

        private List<string> m_ElementsKeys = new List<string>
        {
            MOUNTAIN,
            ICEBERG,
            CLOUD
        };

        private Dictionary<string, ObjectPoolManager> m_PoolsDictionary = new Dictionary<string, ObjectPoolManager>();
        private Dictionary<string, List<EnvironmentElement>> m_ActiveElementsDictionary = new Dictionary<string, List<EnvironmentElement>>();
        private Dictionary<string, List<EnvironmentElement>> m_ElementsToBeRemovedDictionary = new Dictionary<string, List<EnvironmentElement>>();
        private Dictionary<string, Dictionary<string, Vector3>> m_ElementsStartingPointsDictionary = new Dictionary<string, Dictionary<string, Vector3>>();
        private Dictionary<string, float> m_DistanceBeforeGenerateNewElement = new Dictionary<string, float>();

        //-----------------------------------------------------------------------
        //Speed variables  & constants

        public float m_ElementsSpeed = 0;

        //-----------------------------------------------------------------------
        private void Update()
        {
            if (GameController.Instance.CurrentState == GameState.Play)
                UpdateEnvironment();
        }

        //-----------------------------------------------------------------------
        //Initialisation Functions
        public void Initialise(float bottomRightScreenCornerX)
        {
            m_BottomRightScreenCornerX = bottomRightScreenCornerX;
            InitialiseElements();
            SetElementsStartingPosition();
            InitialiseObjectPools();

            //Used a coroutine to manage time synchronisation with GameController
            //SetupBackground();
        }

        private void InitialiseElements()
        {
            int count = 0;
            foreach (string key in m_ElementsKeys)
            {
                switch (key)
                {
                    case MOUNTAIN:
                        count = NUM_OF_MOUNTAIN_SPRITES;
                        break;

                    case ICEBERG:
                        count = NUM_OF_ICEBERG_SPRITES;
                        break;

                    case CLOUD:
                        count = NUM_OF_CLOUD_SPRITES;
                        break;
                }
                List<GameObject> prefabList = new List<GameObject>();
                for (int i = 1; i <= count; ++i)
                {
                    var element = Resources.Load<GameObject>($"Prefabs/Environment/{key}/{key}{i}_");

                    var elementScript = element.GetComponent<EnvironmentElement>();
                    if (elementScript == null)
                        element.AddComponent<EnvironmentElement>();

                    prefabList.Add(element);
                }
                m_ElementsDictionary.Add(key, prefabList);
                m_ActiveElementsDictionary.Add(key, new List<EnvironmentElement>());
                m_ElementsToBeRemovedDictionary.Add(key, new List<EnvironmentElement>());
            }
        }

        private void InitialiseObjectPools()
        {
            var environmentParent = this.transform.Find("EnviromentsElements").transform;
            var oPParentTransform = environmentParent.Find("ObjectPools").transform;
            Transform activeObjectsTransform = environmentParent.Find("ActiveElements").transform;

            foreach (var key in m_ElementsKeys)
            {
                var pool = new GameObject(key);
                pool.transform.SetParent(oPParentTransform);
                var activeElements = new GameObject($"Active{key}s");
                activeElements.transform.SetParent(activeObjectsTransform);
                var objPool = pool.AddComponent<ObjectPoolManager>();
                objPool.CreateObjPool(m_ElementsDictionary[key], NUM_OF_ELEMENT_PER_TYPE, pool.transform, activeElements.transform);
                m_PoolsDictionary.Add(key, objPool);
            }
        }

        public void SetupBackground()
        {
            foreach (var key in m_ElementsKeys)
            {
                EnvironmentElement element = m_PoolsDictionary[key].GetObject().GetComponent<EnvironmentElement>();
                float posBeforeActivateNewElement;
                if (element != null)
                {
                    Vector3 startingPos = element.transform.position;
                    startingPos.x = Random.Range(-m_BottomRightScreenCornerX, m_BottomRightScreenCornerX);
                    element.Activate(m_ElementsSpeed, startingPos);
                    m_ActiveElementsDictionary[key].Add(element);
                    posBeforeActivateNewElement = (m_BottomRightScreenCornerX * 2) - m_ElementsStartingPointsDictionary[key][element.name].x + GetRandomValue();
                    if (m_DistanceBeforeGenerateNewElement.ContainsKey(key) == false)
                        m_DistanceBeforeGenerateNewElement.Add(key, posBeforeActivateNewElement);
                    else
                        m_DistanceBeforeGenerateNewElement[key] = posBeforeActivateNewElement;
                }
            }
        }

        private float GetElementLenght(GameObject obj)
        {
            var objCollider = obj.AddComponent<BoxCollider2D>();
            float lenght = objCollider.size.x;
            DestroyImmediate(objCollider, true);
            return lenght;
        }

        private void SetElementsStartingPosition()
        {
            Vector3 elementStartingPos;
            foreach (string key in m_ElementsKeys)
            {
                var elementList = m_ElementsDictionary[key];
                Dictionary<string, Vector3> elementStartingPoint = new Dictionary<string, Vector3>();
                foreach (var element in elementList)
                {
                    float lenght = GetElementLenght(element);
                    elementStartingPos = element.transform.position;
                    elementStartingPos.x = m_BottomRightScreenCornerX + (lenght / 2);

                    elementStartingPoint.Add(element.name, elementStartingPos);
                }
                m_ElementsStartingPointsDictionary.Add(key, elementStartingPoint);
            }
        }

        //-----------------------------------------------------------------------
        //Get and return elements to the object pools
        private void GetElement(string elementType)
        {
            EnvironmentElement element = m_PoolsDictionary[elementType].GetObject().GetComponent<EnvironmentElement>();
            Vector3 startingPos = m_ElementsStartingPointsDictionary[elementType][element.name];
            if (element != null)
            {
                if (elementType != CLOUD)
                    element.Activate(m_ElementsSpeed, startingPos);
                else
                    element.Activate(m_ElementsSpeed, startingPos);

                m_ActiveElementsDictionary[elementType].Add(element);
                float posBeforeActivateNewElement = (m_BottomRightScreenCornerX * 2) - startingPos.x - GetRandomValue();
                m_DistanceBeforeGenerateNewElement[elementType] = posBeforeActivateNewElement;
            }
        }

        private void ReturnElement(EnvironmentElement element, string elementType)
        {
            m_PoolsDictionary[elementType].ReturnObjectToThePool(element.gameObject);
            m_ActiveElementsDictionary[elementType].Remove(element);
        }

        //-----------------------------------------------------------------------
        //Manage the environment => once an element is out of scene is returned to the pool and a new one is activated
        private void UpdateEnvironment()
        {
            foreach (var key in m_ElementsKeys)
            {
                var activeElements = m_ActiveElementsDictionary[key];
                if (activeElements.Count != 0)
                {
                    foreach (var element in activeElements)
                    {
                        if (element.transform.position.x < -m_DistanceBeforeGenerateNewElement[key])
                        {
                            m_ElementsToBeRemovedDictionary[key].Add(element);
                            m_ActiveElementsDictionary[key].Remove(element);
                            GetElement(key);
                            break;
                        }
                    }
                }

                var elementsToBeRemoved = m_ElementsToBeRemovedDictionary[key];
                if (elementsToBeRemoved.Count != 0)
                {
                    foreach (var element in elementsToBeRemoved)
                    {
                        if (element.transform.position.x < -m_ElementsStartingPointsDictionary[key][element.name].x)
                        {
                            m_ElementsToBeRemovedDictionary[key].Remove(element);
                            ReturnElement(element, key);
                            break;
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------
        //Get and return elements to the object pools
        private float GetRandomValue()
        {
            float randomVal = Random.Range(-m_BottomRightScreenCornerX, m_BottomRightScreenCornerX);
            return randomVal;
        }

        //-----------------------------------------------------------------------
        //Get a cloud to the environment manager to activate the thunder effect
        public ParticleSystem GetActiveCloud()
        {
            var list = m_ActiveElementsDictionary[CLOUD];

            foreach (var cloud in list)
            {
                if (cloud.transform.position.x > 0)
                    return cloud.GetComponentInChildren<ParticleSystem>();
            }
            return null;
        }

        //-----------------------------------------------------------------------
        //Increase elements speed when the game diffult is increased or game is started
        public void IncreaseElementsSpeed(float newSpeed)
        {
            this.Create<ValueTween>(GameController.Instance.m_GameInitialisationTime, EaseType.Linear, () =>
            {
                m_ElementsSpeed = newSpeed;
            }).Initialise(m_ElementsSpeed, newSpeed, (f) =>
            {
                foreach (var key in m_ElementsKeys)
                {
                    var activeElements = m_ActiveElementsDictionary[key];
                    if (activeElements.Count != 0)
                    {
                        foreach (var element in activeElements)
                            element.IncreaseSpeed(f);
                    }
                    var elementsToBeRemoved = m_ElementsToBeRemovedDictionary[key];
                    if (elementsToBeRemoved.Count != 0)
                    {
                        foreach (var element in elementsToBeRemoved)
                            element.IncreaseSpeed(f);
                    }
                }
            });
        }

        //-----------------------------------------------------------------------
        //Clear the background 
        public void ClearBackgorund()
        {
            //m_Ready = false;
            foreach (var key in m_ElementsKeys)
            {
                var activeElements = m_ActiveElementsDictionary[key];
                if (activeElements.Count != 0)
                {
                    foreach (var element in activeElements)
                        m_PoolsDictionary[key].ReturnObjectToThePool(element.gameObject);

                    activeElements.Clear();
                }

                var elementsToBeRemoved = m_ElementsToBeRemovedDictionary[key];
                if (elementsToBeRemoved.Count != 0)
                {
                    foreach (var element in elementsToBeRemoved)
                        m_PoolsDictionary[key].ReturnObjectToThePool(element.gameObject);

                    elementsToBeRemoved.Clear();
                }
            }
            //m_IsPlayerAlive = true;
            SetupBackground();
        }

        //-----------------------------------------------------------------------
        //Stop the elements when the player is dead
        public void Stop()
        {
            //m_IsPlayerAlive = false;
            foreach (var key in m_ElementsKeys)
            {
                var activeElements = m_ActiveElementsDictionary[key];
                if (activeElements.Count != 0)
                {
                    foreach (var element in activeElements)
                    {
                        element.Stop();
                        m_PoolsDictionary[key].ReturnObjectToThePool(element.gameObject);
                    }
                }

                var elementsToBeRemoved = m_ElementsToBeRemovedDictionary[key];
                if (elementsToBeRemoved.Count != 0)
                {
                    foreach (var element in elementsToBeRemoved)
                    {
                        element.Stop();
                        m_PoolsDictionary[key].ReturnObjectToThePool(element.gameObject);
                    }
                }
            }
        }
    }
}