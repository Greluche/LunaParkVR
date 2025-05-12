using UnityEngine;
//Inspried  by https://github.com/SunnyValleyStudio/VR-Archery-in-Unity-2022/blob/main/Vid%201-2/BowString.cs
public class BowString : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   [SerializeField]
    private Transform endpoint_1, endpoint_2, midpoint;
    
    private LineRenderer lineRenderer;
    private Vector3  xMidpoint, middlepoint;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void Update(){
        //lineRenderer.SetPosition(1,midpoint.position);
        middlepoint = (endpoint_1.localPosition-endpoint_2.localPosition)/2 + endpoint_2.localPosition;
    
        xMidpoint = transform.InverseTransformPoint(midpoint.position);
        if(xMidpoint.x<0){
             CreateString(Vector3.Project(xMidpoint,middlepoint));

        }
     

    }

    public void CreateString(Vector3? midPosition)
    {
        Vector3[] linePoints = new Vector3[midPosition == null ? 2 : 3];
        linePoints[0] = endpoint_1.localPosition;
        if (midPosition != null)
        {
            linePoints[1] = midPosition.Value;
        }
        linePoints[^1] = endpoint_2.localPosition;

        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);
    }

    private void Start()
    {
        //CreateString(midpoint.position);
    }
    public void onRelease(){
        midpoint.localPosition = (endpoint_1.localPosition-endpoint_2.localPosition)/2 + endpoint_2.localPosition;
    }
}
