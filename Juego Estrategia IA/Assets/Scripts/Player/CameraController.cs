using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera movement properties")]
    [Tooltip("Camera movement speed when following the mouse")]
    [SerializeField] private float _panSpeed = 10f;
    private float _distanceMouseOffset;
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

    private bool _movingToTarget;
    private Vector3 targetPosition;

    public bool showGizmos = false;

    //-----------------------Getters and Setters---------------------------------
    public float panSpeed { get { return _panSpeed; } }
    //---------------------------------------------------------------------------


    // Start is called before the first frame update
    void Start()
    {
        screenCenterX = Screen.width / 2;
        screenCenterY = Screen.height / 2;

        _distanceMouseOffset = Mathf.Min(screenCenterY, screenCenterX);
        _distanceMouseOffset = _distanceMouseOffset - _distanceMouseOffset / 8;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_movingToTarget)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float mouseOffsetX = Input.mousePosition.x - screenCenterX;
            float mouseOffsetY = Input.mousePosition.y - screenCenterY;

            Vector2 screenCenter = new Vector2(screenCenterX, screenCenterY);
            //Check if the has moved outside the 
            if (Vector2.Distance(screenCenter, Input.mousePosition) >= _distanceMouseOffset)
            {
                //Move  the camera the distance we want
                Move(mouseOffsetX, mouseOffsetY);
            }

            if (Input.mouseScrollDelta != Vector2.zero)
                ZoomCamera(Input.mouseScrollDelta.y);
        }
        else
        {
            MoveToTarget();
        }
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

    private void MoveToTarget()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSpeed);
        if(Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            _movingToTarget = false;
        }
    }

    public void SetCameraPosition(Vector3 worldPos)
    {
        worldPos.y = transform.position.y;

        _movingToTarget = true;
        targetPosition = worldPos;
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
