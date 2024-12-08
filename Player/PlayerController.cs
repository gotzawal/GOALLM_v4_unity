using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    private Camera _mainCamera;
    private CharacterController _controller;
    private Player _player;
    private float _gravity;
    private Vector3 _movePosition;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<Player>();
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        _mainCamera.gameObject.transform.eulerAngles = Vector3.zero;
    }

    public void Move(Vector3 direction)
    {
        Vector3 move = _player._moveSpeed * Time.deltaTime * transform.TransformDirection(direction);
        _controller.Move(move);
    }

    public void Rotate(float mouseX, float mouseY)
    {
        if(mouseX != 0 || mouseY != 0)
        {
            transform.Rotate(0, mouseX, 0);
            // 카메라의 로컬 회전을 X축으로만 적용
            Vector3 currentRotation = _mainCamera.gameObject.transform.localEulerAngles;
            currentRotation.x -= mouseY;
            if (currentRotation.x > 180)
            {
                currentRotation.x -= 360;
            }
            currentRotation.x = Mathf.Clamp(currentRotation.x, -30, 30);
            _mainCamera.gameObject.transform.localEulerAngles = currentRotation;
        }
    }

    public void Zoom(float scrollY)
    {
        if (scrollY != 0)
        {
            float zoomAmount = scrollY * 10;
            _mainCamera.fieldOfView = Mathf.Clamp(_mainCamera.fieldOfView - zoomAmount, 15f, 75f);
        }
    }

    public void InitializeZoom()
    {
        _mainCamera.fieldOfView = 75;
    }
}