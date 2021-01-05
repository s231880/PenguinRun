using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;

namespace PenguinRun
{
    public class PathManager : MonoBehaviour
    {
        private const string PATH = "Path";
        private const int NUM_OF_PATH = 3;
        private const int NUM_OF_ELEMENTS = 10;
        private PathElement m_LastElement;
        private Vector3 m_ElementsStartingPoint = Vector3.zero;
        private List<GameObject> m_PathList = new List<GameObject>();
        private List<PathElement> m_ActiveElements = new List<PathElement>();
        private float m_ElementLenght;
        public float pathSpeed = 0;
        private ObjectPoolManager m_Pool;
        private const float OFFSET = 0.25f;

        private void Update()
        {
            UpdatePath();
        }

        public void Initialise(float bottomRightScreenCornerX)
        {
            InitialiseElements();
            GenerateObjPool();
            FindStartingPoint(bottomRightScreenCornerX);

            StartCoroutine(InitialiseScene());
        }

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

        private void FindStartingPoint(float bottomRightScreenCornerX)
        {
            var element = m_PathList[0];

            var objCollider = element.AddComponent<BoxCollider2D>();
            m_ElementLenght = objCollider.size.x;
            m_ElementLenght -= OFFSET;
            DestroyImmediate(objCollider, true);

            m_ElementsStartingPoint = element.transform.position;
            m_ElementsStartingPoint.x = bottomRightScreenCornerX + (m_ElementLenght / 2);
        }

        IEnumerator InitialiseScene()
        {
            while (pathSpeed == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            var element = m_Pool.GetObject().GetComponent<PathElement>();
            element.Activate(m_ElementsStartingPoint, pathSpeed);
            m_ActiveElements.Add(element);
            m_LastElement = element;

            Vector3 thirdStartingPoint = m_ElementsStartingPoint;
            thirdStartingPoint.x -= m_ElementLenght;
            var thirdElement = m_Pool.GetObject().GetComponent<PathElement>();
            thirdElement.Activate(thirdStartingPoint, pathSpeed);
            m_ActiveElements.Add(thirdElement);
        }

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
            element.Activate(pos, pathSpeed);
            m_ActiveElements.Add(element);
            m_LastElement = element;
        }
    }
}
