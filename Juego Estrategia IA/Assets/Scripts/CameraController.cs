using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera movement properties")]
    [Tooltip("Camera movement speed when following the mouse")]
    [SerializeField] private float _panSpeed = 10f;
    [Tooltip("Mouse offset in the X/Z axis to move the camera")]
    [SerializeField] private Vector2 _minMouseOffset = new Vector2(10, 10);
    [Tooltip("Camera zoom in/out speed when scrolling")]
    [SerializeField] private float _zoomSpeed = 2f;

    [Header("Camera boundaries")]
    [Tooltip("Camera limits in the X/Z axis")]
    [SerializeField] private Vector2 _cameraBoundaries;
    [Tooltip("Limits of the camera in the Y axis, x for the minimum height, y for the maximum")]
    [SerializeField] private Vector2 _cameraZoomLimit;

    //Screen center to compare with the mouse position
    private float screenCenterX;
    private float screenCenterY;

    public bool showGizmos = false;

    //-----------------------Getters and Setters---------------------------------
    public float panSpeed { get { return _panSpeed; } }
    public Vector2 mouseMinOffset { get { return _minMouseOffset; } }
    //---------------------------------------------------------------------------


    // Start is called before the first frame update
    void Start()
    {
        screenCenterX = Screen.width / 2;
        screenCenterY = Screen.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float mouseOffsetX = Input.mousePosition.x - screenCenterX;
        float mouseOffsetY = Input.mousePosition.y - screenCenterY;

        //Check if the has moved outside the 
        if (Mathf.Abs(mouseOffsetX) > mouseMinOffset.x ||
                Mathf.Abs(mouseOffsetY) > mouseMinOffset.y)
        {
            //Move  the camera the distance we want
            Move(mouseOffsetX, mouseOffsetY);
        }

        if (Input.mouseScrollDelta != Vector2.zero)
            ZoomCamera(Input.mouseScrollDelta.y);
    }

    private void Move(float mouseOffsetX, float mouseOffsetY)
    {
        //Mover cámara a la velodidad pertinente en cada eje
        float cameraMovementX = (mouseOffsetX / screenCenterX) * _panSpeed;
        float cameraMovementZ = (mouseOffsetY / screenCenterY) * _panSpeed;

        Vector3 newPosition = transform.position + new Vector3(cameraMovementX, 0f, cameraMovementZ);

        if (_cameraBoundaries != Vector2.zero)
        {
            if (Mathf.Abs(newPosition.x) >= _cameraBoundaries.x)
            {
                //Put the camera on X's axis limit - The operation in parentesi, tries to know if it is positive or negative
                newPosition.x = (Mathf.Abs(newPosition.x) / newPosition.x) * _cameraBoundaries.x;
            }

            if (Mathf.Abs(newPosition.z) >= _cameraBoundaries.y)
            {
                //Put the camera on Y's axis limit - The operation in parentesi, tries to know if it is positive or negative
                newPosition.z = (Mathf.Abs(newPosition.z) / newPosition.z) * _cameraBoundaries.y;
            }
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);
    }

    private void ZoomCamera(float zoomOffset)
    {
        //Queremos que la y aumente cuando el zoomOffset sea negativo, es decir cuando se haga scroll abajo
        float newCameraY = transform.position.y + -zoomOffset * _zoomSpeed;
        //Asegurarse de que la altura se encuentre dentro de los limites del zoom, x para el minimo, y para el máximo
        newCameraY = Mathf.Clamp(newCameraY, _cameraZoomLimit.x, _cameraZoomLimit.y);

        transform.position = new Vector3(transform.position.x, newCameraY, transform.position.z);
    }
}
