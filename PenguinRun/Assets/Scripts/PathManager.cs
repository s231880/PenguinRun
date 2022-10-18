using System.Collections.Generic;
using UnityEngine;

namespace PenguinRun
{
    public class PathManager : MonoBehaviour
    {
        private const string PATH = "Path";
        private const int NUM_OF_PATH = 3;
        private const int NUM_OF_ELEMENTS = 10;
        private const float OFFSET = 0.25f;
        private const int STARTING_SPEED = 0;

        private PathElement m_LastElement;
        private Vector3 m_ElementsStartingPoint = Vector3.zero;
        private List<GameObject> m_PathList = new List<GameObject>();
        private List<PathElement> m_ActiveElements = new List<PathElement>();
        private float m_ElementLenght;
        private ObjectPoolManager m_Pool;

        private int m_PathToBeInitialised;
        public float m_CurrentPathSpeed = 0;

        public bool m_Ready = false;
        public float m_PreviousElementSpeed = 0;

        private void Update()
        {
            UpdatePath();
        }

        public void Initialise(float bottomRightScreenCornerX)
        {
            InitialiseElements();
            GenerateObjPool();
            FindStartingPointAndPathToInitialise(bottomRightScreenCornerX);
            //SetupPath();
        }

        //-----------------------------------------------------------------------
        //Initialising functions
        private void GenerateObjPool()
        {
            var environmentParent = this.transform.Find("EnviromentsElements").transform;
            var oPParentTransform = environmentParent.Find("ObjectPools").transform;
            Transform activeObjectsTransform = environmentParent.Find("ActiveElements").transform;

            var pool = new GameObject(PATH);
            pool.transform.SetParent(oPParentTransform);
            var activeElements = new GameObject($"Active{PATH}s");
            activeElements.transform.SetParent(activeObjectsTransform);
            m_Pool = pool.AddComponent<ObjectPoolManager>();
            m_Pool.CreateObjPool(m_PathList, NUM_OF_ELEMENTS, pool.transform, activeElements.transform);
        }

        private void InitialiseElements()
        {
            for (int i = 0; i < NUM_OF_PATH; ++i)
            {
                var element = Resources.Load<GameObject>($"Prefabs/Environment/{PATH}/{PATH}{i}");

                var elementScript = element.GetComponent<PathElement>();
                if (elementScript == null)
                    element.AddComponent<PathElement>();

                m_PathList.Add(element);
            }
        }

        private void FindStartingPointAndPathToInitialise(float bottomRightScreenCornerX)
        {
            var element = m_PathList[0];

            var objCollider = element.AddComponent<BoxCollider2D>();
            m_ElementLenght = objCollider.size.x;
            m_ElementLenght -= OFFSET;
            DestroyImmediate(objCollider, true);

            m_ElementsStartingPoint = element.transform.position;
            m_ElementsStartingPoint.x = bottomRightScreenCornerX + (m_ElementLenght / 2);

            m_PathToBeInitialised = (int)(FindScreenLength() / m_ElementLenght);
        }

        private float FindScreenLength()
        {
            Vector2 bottomRightScreenCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0));
            Vector2 bottomLeftScreenCorner = Camera.main.ScreenToWorldPoint(new Vector3(-Screen.width, 0));
            float screenLenght = bottomRightScreenCorner.x - bottomLeftScreenCorner.x;
            return screenLenght;
        }

        public void SetupPath()
        {
            Vector3 startingPoint = m_ElementsStartingPoint;

            for (int count = -m_PathToBeInitialised + 1; count <= 1; ++count)  //Magic number to be changed
            {
                startingPoint.x -= (m_ElementLenght * count);
                var element = m_Pool.GetObject().GetComponent<PathElement>();
                element.Activate(startingPoint, STARTING_SPEED);
                m_ActiveElements.Add(element);

                if (count == -m_PathToBeInitialised + 1)
                    m_LastElement = element;
            }
            m_Ready = true;
        }

        //-----------------------------------------------------------------------
        //Increase the speed of the elements
        public void IncreasePathsSpeed(float newSpeed)
        {
            this.Create<ValueTween>(GameController.Instance.m_GameInitialisationTime, EaseType.Linear, () =>
            {
                m_CurrentPathSpeed = newSpeed;
            }).Initialise(m_CurrentPathSpeed, newSpeed, (f) =>
            {
                foreach (var path in m_ActiveElements)
                    path.IncreaseSpeed(f);
            });
        }

        //-----------------------------------------------------------------------
        private void UpdatePath()
        {
            foreach (var path in m_ActiveElements)
            {
                if (path.transform.position.x < -m_ElementsStartingPoint.x)
                {
                    ReturnElement(path);
                    GetNewElement();
                    break;
                }
            }
        }

        private void ReturnElement(PathElement path)
        {
            m_Pool.ReturnObjectToThePool(path.gameObject);
            m_ActiveElements.Remove(path);
        }

        private void GetNewElement()
        {
            var element = m_Pool.GetObject().GetComponent<PathElement>();
            Vector3 pos = m_LastElement.transform.position;
            pos.x += m_ElementLenght;
            element.Activate(pos, m_CurrentPathSpeed);
            m_ActiveElements.Add(element);
            m_LastElement = element;
        }

        //-----------------------------------------------------------------------
        //Stop the path
        public void Stop()
        {
            if (m_ActiveElements.Count != 0)
            {
                foreach (var path in m_ActiveElements)
                    path.Stop();
            }
        }

        //-----------------------------------------------------------------------
        //Clear all and prepare for restart
        public void ResetManager()
        {
            m_Ready = false;
            if (m_ActiveElements.Count != 0)
            {
                foreach (var path in m_ActiveElements)
                    m_Pool.ReturnObjectToThePool(path.gameObject);

                m_ActiveElements.Clear();
            }
            SetupPath();
        }

        //-----------------------------------------------------------------------
    }
}