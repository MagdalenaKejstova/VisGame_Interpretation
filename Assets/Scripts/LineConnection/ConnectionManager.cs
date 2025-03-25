using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Events;

public class ConnectionManager : MonoBehaviour
{
    public UnityEvent<int> correctConnectionCreated;
    public UnityEvent unorderedConnectionCreated;
    public UnityEvent incompleteConnectionCreated;
    public UnityEvent unexpectedStartConnectionCreated;
    public UnityEvent incorrectTendencyConnectionCreated;
    public UnityEvent tooShortTrendCreated;

    public LineRenderer lineRenderer;
    public LayerMask targetLayerMask;
    private GameObject _gameScreen;
    private int _possibleObjectsToConnectCount;
    private int _expectedObjectsToConnectCount;
    private int _totalConnectedObjectsCounts;
    private int _actualStartIndex;
    private int _expectedStartIndex;
    private Camera _camera;
    private bool _isDrawing;
    private bool _isDisabled;
    private bool _isConnectionInOrder = true;
    private int _connectionSegments = 0;
    private Tendency _currentTendency;

    [HideInInspector] public Tendency targetTendency;
    [HideInInspector] public int maxConnectionSegments = 0;
    [HideInInspector] public bool checkTendency;
    
    public List<GameObject> connectedObjects = new();
    public List<Vector3> drawPositions = new();

    // Start is called before the first frame update
    void Start()
    {
        _camera = GameObject.Find("CanvasCamera").GetComponent<Camera>();
        _gameScreen = GameObject.Find("GameScreen");
        AdjustLineDepth();
    }

    private void AdjustLineDepth()
    {
        var depthAdjustedPosition = lineRenderer.transform.position;
        depthAdjustedPosition.z = 0;
        lineRenderer.transform.position = depthAdjustedPosition;
    }

    public float GetScreenDepth()
    {
        return _gameScreen.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDisabled)
        {
            return;
        }

        var isMouseClick = Input.GetMouseButtonDown(0);
        var isTouchBegin = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

        if (isMouseClick || isTouchBegin)
        {
            var ray = GetRayOnPointedPosition();
            if (Physics.Raycast(ray, out var raycastHit, 100f, targetLayerMask))
            {
                SetDrawingConditions(raycastHit.transform.gameObject);
                _isDrawing = true;
                connectedObjects.Add(raycastHit.transform.gameObject);
                lineRenderer.gameObject.SetActive(true);
            }
        }

        var isMouseAction = Input.GetMouseButton(0);
        var isTouchAction = Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Began ||
                                                     Input.GetTouch(0).phase == TouchPhase.Moved ||
                                                     Input.GetTouch(0).phase == TouchPhase.Stationary);

        if ((isMouseAction || isTouchAction) && _isDrawing)
        {
            var ray = GetRayOnPointedPosition();
            if (Physics.Raycast(ray, out var raycastHit, 100f, targetLayerMask))
            {
                var targetObject = raycastHit.transform.gameObject;
                var targetAnchor = targetObject.GetComponent<Anchor>();

                if (targetAnchor != null)
                {
                    if (!connectedObjects.Contains(targetObject))
                    {
                        var isExpectedAnchor = targetAnchor.order == _actualStartIndex + connectedObjects.Count;
                        _isConnectionInOrder &= isExpectedAnchor;
                        connectedObjects.Add(raycastHit.transform.gameObject);
                    }
                }
            }

            DrawLine();
        }

        var isMouseRelease = Input.GetMouseButtonUp(0);
        var isTouchRelease = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;

        if ((isMouseRelease || isTouchRelease) && _isDrawing)
        {
            var hasCorrectTendency = checkTendency ? _currentTendency == targetTendency : true;
            var hasExpectedStart = _actualStartIndex == _expectedStartIndex;
            var areAllExpectedConnected = connectedObjects.Count == _expectedObjectsToConnectCount;
            // Incorrect connection
            if (!hasExpectedStart || !_isConnectionInOrder || !areAllExpectedConnected || !hasCorrectTendency)
            {
                if (!areAllExpectedConnected)
                {
                    if (_expectedObjectsToConnectCount < 0)
                    {
                        tooShortTrendCreated.Invoke();
                    }
                    else
                    {
                        incompleteConnectionCreated.Invoke();
                    }
                }
                else if (!_isConnectionInOrder)
                {
                    unorderedConnectionCreated.Invoke();
                }
                else if (!hasExpectedStart)
                {
                    unexpectedStartConnectionCreated.Invoke();
                }
                else if (!hasCorrectTendency)
                {
                    incorrectTendencyConnectionCreated.Invoke();
                }
                

                DeactivateDrawing();
            }
            // Correct connection
            else
            {
                _connectionSegments++;
                if (_connectionSegments == maxConnectionSegments)
                {
                    _isDisabled = true;
                }

                // Remove hanging piece of line if user ended draw beyond last point
                // Compensate for quadruple adding of each position
                if (lineRenderer.positionCount > _expectedObjectsToConnectCount * 4)
                {
                    lineRenderer.positionCount -= 1;
                }

                correctConnectionCreated.Invoke(_actualStartIndex);
            }

            _isDrawing = false;
            _isConnectionInOrder = true;
            connectedObjects.Clear();
        }
    }

    private void SetDrawingConditions(GameObject dataPoint)
    {
        var anchor = dataPoint.GetComponent<Anchor>();
        lineRenderer = anchor.lineRenderer;
        AdjustLineDepth();
        _actualStartIndex = anchor.order;
        _expectedStartIndex = anchor.expectedStartIndex;
        _possibleObjectsToConnectCount = anchor.possibleObjectsToConnectCount;
        _expectedObjectsToConnectCount = anchor.expectedObjectsToConnectCount;
        _currentTendency = anchor.currentTendency;
    }

    private Ray GetRayOnPointedPosition()
    {
        return _camera.ScreenPointToRay(GetPosition());
    }

    private Vector3 GetPosition()
    {
        if (Input.touchSupported)
        {
            return Input.GetTouch(0).position;
        }

        return Input.mousePosition;
    }

    private void DrawLine()
    {
        if (connectedObjects.Count > 0 && connectedObjects.Count <= _possibleObjectsToConnectCount)
        {
            drawPositions.Clear();
            foreach (var targetObject in connectedObjects)
            {
                drawPositions.Add(targetObject.transform.position);
                drawPositions.Add(targetObject.transform.position);
                drawPositions.Add(targetObject.transform.position);
                drawPositions.Add(targetObject.transform.position);
            }

            if (connectedObjects.Count < _possibleObjectsToConnectCount)
            {
                var inputDrawPosition = GetWorldInputPosition();
                drawPositions.Add(inputDrawPosition);
            }

            lineRenderer.positionCount = drawPositions.Count;
            lineRenderer.SetPositions(drawPositions.ToArray());
        }
    }

    private Vector3 GetWorldInputPosition()
    {
        var position = GetPosition();
        var z = _gameScreen.transform.position.z;
        var targetInputPos = _camera.ScreenToWorldPoint(new Vector3(position.x, position.y, z));
        return targetInputPos;
    }

    private void DeactivateDrawing()
    {
        lineRenderer.positionCount = 0;
        drawPositions.Clear();
        lineRenderer.gameObject.SetActive(false);
    }

    public void SetObjectsToConnect(int objectsToConnect)
    {
        _possibleObjectsToConnectCount = objectsToConnect;
    }

    public void Disable()
    {
        _isDisabled = true;
    }

    public void Enable()
    {
        _isDisabled = false;
    }
}