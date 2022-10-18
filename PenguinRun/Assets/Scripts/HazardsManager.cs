using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Some things could have be done differently, for example the width, start and end positions could have
//been set as private/public variable into the hazard script instead of been store into a dictionary here
//I'll change them in future
namespace PenguinRun
{
    public enum HazardType
    {
        SingleFire,
        LowBird,
        HighBird,
        DoubleCloseFires,
        DoubleDistantFires,
        DoubleHighBird,
        DoubleLowBird,
        TripleDistantFires,
        TripleLowBird
    }

    public class HazardsManager : MonoBehaviour
    {
        private Dictionary<string, GameObject> m_HazardPrefabDictionary = new Dictionary<string, GameObject>();

        private List<string> m_HazardListKeys = new List<string>
        {
            FIRE,
            BIRD
        };

        //-------------------------------------------------------------------------------
        //Item containers => to get and return elements during the gameplay
        private Dictionary<string, ObjectPoolManager> m_ObjPoolsDictionary = new Dictionary<string, ObjectPoolManager>();

        private Dictionary<string, List<HazardElement>> m_ActiveHazardsDictionary = new Dictionary<string, List<HazardElement>>();
        private Dictionary<string, List<HazardElement>> m_ElementsToBeRemovedDictionary = new Dictionary<string, List<HazardElement>>();
        private Dictionary<string, float> m_HazardsEndX = new Dictionary<string, float>();
        private Dictionary<string, Vector3> m_HazardsStartPos = new Dictionary<string, Vector3>();

        //-------------------------------------------------------------------------------
        //Constant variables
        private const int EASY_HAZARDS = 3;
        private const int MEDIUM_HAZARDS = 7;
        private const int HARD_HAZARDS = 9;
        private const int HIGH_BIRD_MULTIPLIER_CONST = 7;
        private const int NUM_OF_HAZARDS_PER_TYPE = 5;
        private const string FIRE = "Fire";
        private const string BIRD = "Bird";
        //-------------------------------------------------------------------------------

        private int m_HazardsCount;
        private float m_HazardBreak = 0;
        [SerializeField] private float m_HazardsSpeed = 10f;/*HAZARD_EASY_SPEED*/ 

        private Vector3 m_PenguinPosition = new Vector3();
        private float m_PenguinWidth = 0;
        private Dictionary<string, float> m_HazardsWidthDictionary = new Dictionary<string, float>();

        //-------------------------------------------------------------------------------

        private HazardType m_CurrentHazard;

        public HazardType CurrentHazard
        {
            get { return m_CurrentHazard; }
            set
            {
                m_CurrentHazard = value;
                GetHazard();
            }
        }

        //-----------------------------------------------------------------------
        private void Update()
        {
            if(GameController.Instance.CurrentState == GameState.Play)
                UpdateHazards();
        }

        //-----------------------------------------------------------------------
        //Initialising functions
        public void Initialise(Vector3 penguinPosition, float penguinWidth, float bottomRightScreenCornerX)
        {
            m_PenguinPosition = penguinPosition;
            m_PenguinWidth = penguinWidth * 2f;

            //Find all hazards and initialise Object Pools
            InitialiseVariables();
            InitialiseHazards();
            InitialiseObjectPools();
            InitialiseHazardsKeyPositions(bottomRightScreenCornerX);
        }

        private void InitialiseVariables()
        {
            //Initialising container to get and return elements
            foreach (var key in m_HazardListKeys)
            {
                m_ActiveHazardsDictionary.Add(key, new List<HazardElement>());
                m_ElementsToBeRemovedDictionary.Add(key, new List<HazardElement>());
            }
        }

        private void InitialiseHazards()
        {
            foreach (var key in m_HazardListKeys)
            {
                var prefab = Resources.Load<GameObject>($"Prefabs/Hazards/{key}");
                float prefabWidth = prefab.GetComponent<BoxCollider2D>().size.x;
                m_HazardsWidthDictionary.Add(key, prefabWidth);

                var elementScript = prefab.GetComponent<HazardElement>();
                if (elementScript == null)
                    prefab.AddComponent<HazardElement>();

                m_HazardPrefabDictionary.Add(key, prefab);
            }
        }

        private void InitialiseObjectPools()
        {
            //Finding the transforms where store the elements
            var environmentParent = this.transform.Find("EnviromentsElements").transform;
            var oPParentTransform = environmentParent.Find("ObjectPools").transform;
            Transform activeObjectsTransform = environmentParent.Find("ActiveElements").transform;

            foreach (var key in m_HazardListKeys)
            {
                var pool = new GameObject(key);
                pool.transform.SetParent(oPParentTransform);
                var activeHazards = new GameObject($"Active{key}");
                activeHazards.transform.SetParent(activeObjectsTransform);
                var objPool = pool.AddComponent<ObjectPoolManager>();
                objPool.CreateObjPool(m_HazardPrefabDictionary[key], NUM_OF_HAZARDS_PER_TYPE, pool.transform, activeHazards.transform);
                m_ObjPoolsDictionary.Add(key, objPool);
            }
        }

        //-----------------------------------------------------------------------
        //Function to set the elements starting pos. I've set variables through this approach
        //because I took into account that differnt devices have diffenrent screen width
        private void InitialiseHazardsKeyPositions(float bottomRightScreenCornerX)
        {
            foreach (var key in m_HazardListKeys)
            {
                Vector3 hazardStartingPosition = m_HazardPrefabDictionary[key].transform.position;
                hazardStartingPosition.x = bottomRightScreenCornerX + (m_HazardsWidthDictionary[key] / 2);
                m_HazardsStartPos.Add(key, hazardStartingPosition);
                m_HazardsEndX.Add(key, -hazardStartingPosition.x);
            }
        }

        //-----------------------------------------------------------------------
        //Set different types of hazard according to the difficulty
        public void SetHazardCount()
        {
            switch (GameController.Instance.CurrentDifficulty)
            {
                case GameDifficulty.Easy:
                    m_HazardsCount = EASY_HAZARDS;
                    m_HazardBreak = 6;
                    break;

                case GameDifficulty.Medium:
                    m_HazardsCount = MEDIUM_HAZARDS;
                    m_HazardBreak = 4;
                    break;

                case GameDifficulty.Hard:
                    m_HazardsCount = HARD_HAZARDS;
                    m_HazardBreak = 3;
                    break;
            }
        }

        //-----------------------------------------------------------------------
        //Set new random hazard to be activated
        public IEnumerator SetNewHazard()
        {
            float time = Random.Range(0, m_HazardBreak);
            yield return new WaitForSeconds(time);
            CurrentHazard = (HazardType)Random.Range(0, m_HazardsCount);
        }

        private void GetHazard()
        {
            switch (m_CurrentHazard)
            {
                case HazardType.SingleFire:
                    ActivateSingleHazard(FIRE);
                    break;

                case HazardType.LowBird:
                    ActivateSingleHazard(BIRD);
                    break;

                case HazardType.HighBird:
                    ActivateSingleHazard(BIRD, true);
                    break;

                case HazardType.DoubleCloseFires:
                    ActivateMultipleHazard(FIRE, 2);
                    break;

                case HazardType.DoubleDistantFires:
                    ActivateMultipleHazard(FIRE, 2, false);
                    break;

                case HazardType.DoubleHighBird:
                    ActivateMultipleHazard(BIRD, 2, false, true);
                    break;

                case HazardType.DoubleLowBird:
                    ActivateMultipleHazard(BIRD, 2, false, false);
                    break;

                case HazardType.TripleDistantFires:
                    ActivateMultipleHazard(FIRE, 3, false);
                    break;

                case HazardType.TripleLowBird:
                    ActivateMultipleHazard(BIRD, 3, false, true);
                    break;

                default:
                    break;
            }
        }

        private void ActivateSingleHazard(string type, bool highBird = false)
        {
            HazardElement hazard = m_ObjPoolsDictionary[type].GetObject().GetComponent<HazardElement>();
            if (hazard != null)
            {
                Vector3 startingPos = m_HazardsStartPos[type];

                if (highBird)
                {
                    startingPos.y *= HIGH_BIRD_MULTIPLIER_CONST;
                }

                hazard.Activate(startingPos, m_HazardsSpeed);
                m_ActiveHazardsDictionary[type].Add(hazard);
            }
        }

        private void ActivateMultipleHazard(string type, int count, bool close = true, bool highBird = false)
        {
            HazardElement hazard;
            Vector3 startingPos;

            for (int i = 0; i < count; ++i)
            {
                startingPos = m_HazardsStartPos[type];
                hazard = m_ObjPoolsDictionary[type].GetObject().GetComponent<HazardElement>();
                if (hazard != null)
                {
                    if (close)
                        startingPos.x += m_HazardsWidthDictionary[type] * i;
                    else
                        startingPos.x += m_PenguinWidth * i;

                    hazard.Activate(startingPos, m_HazardsSpeed);
                    m_ActiveHazardsDictionary[type].Add(hazard);
                }
            }
        }

        private void UpdateHazards()
        {
            foreach (var key in m_HazardListKeys)
            {
                if (GameController.Instance.CurrentDifficulty == GameDifficulty.Easy)
                {
                    List<HazardElement> list = m_ActiveHazardsDictionary[key];
                    if (list.Count != 0)
                    {
                        foreach (var hazard in list)//Check every element in the list
                        {
                            if (hazard.transform.position.x < m_PenguinPosition.x)//If the element has passed the penguin
                            {
                                //If hazard is no more visible
                                if (hazard.transform.position.x < m_HazardsEndX[key])
                                {
                                    m_ObjPoolsDictionary[key].ReturnObjectToThePool(hazard.gameObject);
                                    m_ActiveHazardsDictionary[key].Remove(hazard);
                                    StartCoroutine(SetNewHazard());
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //When the difficult is not easy, hazards are generated once they pass the penguin and not when they
                    // are out of scene
                    List<HazardElement> activeElementList = m_ActiveHazardsDictionary[key];
                    if (activeElementList.Count != 0)
                    {
                        foreach (var hazard in activeElementList)
                        {
                            //If the element has passed the penguin a new hazard is generated and the element is moved into
                            //another list, ready to be removed once it goes out of screen
                            if (hazard.transform.position.x < m_PenguinPosition.x)
                            {
                                activeElementList.Remove(hazard);
                                m_ElementsToBeRemovedDictionary[key].Add(hazard);
                                StartCoroutine(SetNewHazard());
                                break;
                            }
                        }
                    }
                    List<HazardElement> toBeRemovedElementList = m_ElementsToBeRemovedDictionary[key];
                    if (toBeRemovedElementList.Count != 0)
                    {
                        foreach (var hazard in toBeRemovedElementList)
                        {
                            //if true, the element is out of screen
                            if (hazard.transform.position.x < m_HazardsEndX[key])
                            {
                                //Return the object to the obj pool and remove it from the list
                                m_ObjPoolsDictionary[key].ReturnObjectToThePool(hazard.gameObject);
                                toBeRemovedElementList.Remove(hazard);
                                break;
                            }
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------
        //Increase hazard speed when the game diffult is increased
        public void IncreaseElementsSpeed(float newSpeed)
        {
            if(m_HazardsSpeed != 0)
            {
                this.Create<ValueTween>(GameController.Instance.m_GameInitialisationTime, EaseType.Linear, () =>
                {
                    m_HazardsSpeed = newSpeed;
                }).Initialise(m_HazardsSpeed, newSpeed, (f) =>
                {
                    foreach (var key in m_HazardListKeys)
                    {
                        var activeElements = m_ActiveHazardsDictionary[key];
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
            else
            {
                m_HazardsSpeed = newSpeed;
                foreach (var key in m_HazardListKeys)
                {
                    var activeElements = m_ActiveHazardsDictionary[key];
                    if (activeElements.Count != 0)
                    {
                        foreach (var element in activeElements)
                            element.IncreaseSpeed(m_HazardsSpeed);
                    }
                }
            }

        }

        //-----------------------------------------------------------------------
        //Reset Manager when game ends
        public void Stop()
        {
            foreach (var key in m_HazardListKeys)
            {
                List<HazardElement> activeElementList = m_ActiveHazardsDictionary[key];
                if (activeElementList.Count != 0)
                {
                    foreach (var hazard in activeElementList)
                        m_ObjPoolsDictionary[key].ReturnObjectToThePool(hazard.gameObject);

                    m_ActiveHazardsDictionary[key].Clear();
                }
                List<HazardElement> toBeRemovedElementList = m_ElementsToBeRemovedDictionary[key];
                if (toBeRemovedElementList.Count != 0)
                {
                    foreach (var hazard in toBeRemovedElementList)
                        m_ObjPoolsDictionary[key].ReturnObjectToThePool(hazard.gameObject);

                    m_ElementsToBeRemovedDictionary[key].Clear();
                }
            }
        }
    }
}