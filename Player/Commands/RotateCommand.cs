public class RotateCommand : ICommand
{
    private PlayerController _playerController;
    private float _mouseX;
    private float _mouseY;

    public RotateCommand(PlayerController playerController, float mouseX, float mouseY)
    {
        _playerController = playerController;
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    public void Execute()
    {
        _playerController.Rotate(_mouseX, _mouseY);
    }
}