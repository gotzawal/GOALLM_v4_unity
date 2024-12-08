using UnityEngine;

public class MoveCommand : ICommand
{
    private PlayerController _playerController;
    private Vector3 _direction;

    public MoveCommand(PlayerController playerController, Vector3 direction)
    {
        _playerController = playerController;
        _direction = direction;
    }

    public void Execute()
    {
        _playerController.Move(_direction);
    }
}