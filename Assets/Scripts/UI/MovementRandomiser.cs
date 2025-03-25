using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
#if UNITY_EDITOR
using UnityEditor.Scripting;
#endif
using UnityEngine;
using UnityEngine.UI;

public class MovementRandomiser : MonoBehaviour
{
    public Vector2 _minPosition;
    public Vector2 _maxPosition;

    public float movementSpeed = 0.05f;
    private Vector3 _newPosition;
    public GameObject image;
    
    // Start is called before the first frame update
    void Awake()
    {
        _newPosition = image.transform.position;
        CalculateMovementBounds();
    }

    // Update is called once per frame
    void Update()
    {
        image.transform.position = Vector3.Lerp(image.transform.position, _newPosition, Time.deltaTime * movementSpeed);
        if (Vector3.Distance(image.transform.position, _newPosition) < 1f)
        {
            GetNewPosition();
        }
    }

    private void CalculateMovementBounds()
    {
        var viewPortionRect = image.transform.parent.GetComponent<RectTransform>();
        var imageRect = image.transform.GetComponent<RectTransform>();

        var imageBounds = GetBounds(imageRect);
        var viewPortionBounds = GetBounds(viewPortionRect);

        var imagePos = image.transform.position;
        
        var minX = imagePos.x - (imageBounds.max.x - viewPortionBounds.max.x);
        var minY = imagePos.y - (imageBounds.max.y - viewPortionBounds.max.y);
        var maxX = imagePos.x + (imageBounds.max.y - viewPortionBounds.max.y);
        var maxY = imagePos.y + (viewPortionBounds.min.y - imageBounds.min.y);
        
        _minPosition = new Vector2(minX, minY);
        _maxPosition = new Vector2(maxX, maxY);
    }

    private Bounds GetBounds(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        
        var min = Vector3.positiveInfinity;
        var max = Vector3.negativeInfinity;
        
        foreach (var corner in corners)
        {
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return bounds;
    }
    
    private void GetNewPosition()
    {
        var xPos = Random.Range(_minPosition.x, _maxPosition.x);
        var yPos = Random.Range(_minPosition.y, _maxPosition.y);
        
        _newPosition = new Vector3(xPos, yPos, 0);
    }
}
