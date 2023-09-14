using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLines : MonoBehaviour
{
    public Camera cam = null;
    public LineRenderer line;
    private Vector3 mousePos;
    Vector3 Pos;
    Vector3 previousPos;
    public List<Vector3> linePositions = new List<Vector3>();
    public float minimumDistance = 0.05f;
    float distance = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
            mousePos.z = 10;
            Pos = cam.ScreenToWorldPoint(mousePos);
            previousPos = Pos;
            linePositions.Add(Pos);
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                mousePos = Input.mousePosition;
                mousePos.z = 10;
                Pos = cam.ScreenToWorldPoint(mousePos);
                distance = Vector3.Distance(Pos, previousPos);

                if (distance >= minimumDistance)
                {
                    previousPos = Pos;
                    linePositions.Add(Pos);
                    line.positionCount = linePositions.Count;
                    line.SetPositions(linePositions.ToArray());
                }
            }
        }
    }
}
