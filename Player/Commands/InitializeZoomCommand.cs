public class InitializeZoomCommand : ICommand
{
    private PlayerController _playerController;

    public InitializeZoomCommand(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void Execute()
    {
        _playerController.InitializeZoom();
    }
}