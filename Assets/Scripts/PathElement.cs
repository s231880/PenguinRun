using UnityEngine;

public class PathElement : MonoBehaviour
{
    private Vector3 m_NextPos = Vector3.zero;

    public void Initialise()
    {
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        this.transform.localPosition -= m_NextPos * Time.deltaTime;
    }

    public void Activate(Vector3 startPos, float speed)
    {
        this.transform.position = startPos;
        m_NextPos = speed * this.transform.right;
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