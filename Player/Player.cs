using UnityEngine;

public class Player : MonoBehaviour
{
    public float _moveSpeed;
    public Vector3 _moveDirection;
    public float mouseX;
    public float mouseY;
    public float scrollY;
    public float _lookSensitivity = 0.5f;

    public bool isChatMode;
    public bool isSideMenuOpen;
}