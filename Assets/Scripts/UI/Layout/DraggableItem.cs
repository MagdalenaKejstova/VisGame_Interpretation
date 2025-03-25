using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _image;
    private TextMeshProUGUI _text;
    private Camera _canvasCamera;
    private GameObject _gameScreen;
    [HideInInspector] public Transform parentAfterDrag;

    private void Start()
    {
        _canvasCamera = GameObject.Find("CanvasCamera").GetComponent<Camera>();
        _gameScreen = GameObject.Find("GameScreen");
        _image = transform.GetComponent<Image>();
        var textTransform = transform.Find("LabelText");
        if (textTransform != null)
        {
            _text = textTransform.gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        if (_image != null)
        {
            _image.raycastTarget = false;
        }

        if (_text != null)
        {
            _text.raycastTarget = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        var mousePosition = Input.mousePosition;
        var worldPosition = _canvasCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = _gameScreen.transform.position.z;

        transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        transform.SetAsFirstSibling();
        if (_image != null)
        {
            _image.raycastTarget = true;
        }

        if (_text != null)
        {
            _text.raycastTarget = true;
        }
    }
}
