using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    protected Player _player;
    private ICommand _moveCommand;
    private ICommand _rotateCommand;
    private ICommand _zoomCommand;
    private ICommand _initializeZoomCommand;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _player = GetComponent<Player>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (_player._moveDirection != Vector3.zero && _moveCommand != null)
        {
            _moveCommand.Execute();
        }
        if (_player.mouseX != 0 && _rotateCommand != null)
        {
            _rotateCommand.Execute();
        }
        if(_player.scrollY != 0 && _zoomCommand != null)
        {
            _zoomCommand.Execute();
        }
    }

    private void Update()
    {
        if (_player.mouseY != 0 && _rotateCommand != null)
        {
            _rotateCommand.Execute();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && !_player.isChatMode) // 입력이 감지되었을 때
        {
            Vector2 input = context.ReadValue<Vector2>();
            _player._moveDirection = new(input.x, 0, input.y);

            // 이동 명령 생성
            _moveCommand = new MoveCommand(_playerController, _player._moveDirection);
        }
        else if (context.canceled) // 입력이 취소되었을 때
        {
            _player._moveDirection = Vector3.zero;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed && !_player.isChatMode)
        {
            Vector2 input = context.ReadValue<Vector2>();
            _player.mouseX = input.x * _player._lookSensitivity;
            _player.mouseY = input.y * _player._lookSensitivity;

            // 회전 명령 생성
            _rotateCommand = new RotateCommand(_playerController, _player.mouseX, _player.mouseY);
        }
        else if (context.canceled)
        {
            _player.mouseX = 0f;
            _player.mouseY = 0f;
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_player.isChatMode) CafeUIManager.instance.ToggleInventoryPanel();
            Debug.Log("OnInventory");
        }
    }

    public void OnQuestBook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_player.isChatMode) CafeUIManager.instance.ToggleQuestPanel();
            Debug.Log("OnQuestBook");
        }
    }

    public void OnCursorMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void OnUI(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(!_player.isChatMode) CafeUIManager.instance.ToggleMainPanel();
            Debug.Log("OnUI");
        }
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_player.isChatMode)
            {
                _player.isSideMenuOpen = !_player.isSideMenuOpen;
                CafeUIManager.instance.ToggleSideMenuPanel(_player.isSideMenuOpen);
            }
            else
            {
                _player.isChatMode = false;
                CafeUIManager.instance.AddAllButtonListeners();
            }
            Cursor.visible = true;
            Debug.Log("OnMenu");
        }
    }

    public void OnRemoveChat(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("OnRemoveChat");
        }
    }

    public void OnChatMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CafeUIManager.instance.RemoveAllButtonListeners();
            _player.isChatMode = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("OnChatMode");
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float input = context.ReadValue<float>();
            _player.scrollY = input * 0.01f;

            // Zoom 명령 생성
            _zoomCommand = new ZoomCommand(_playerController, _player.scrollY);
        }
        else if (context.canceled)
        {
            _player.scrollY = 0;
        }
    }

    public void OnInitializeZoom(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Zoom 초기화 명령 생성
            _initializeZoomCommand = new InitializeZoomCommand(_playerController);
            _initializeZoomCommand.Execute();
        }
    }
}