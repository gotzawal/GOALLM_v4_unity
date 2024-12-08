public class ZoomCommand : ICommand
{
    private PlayerController _playerController;
    private float _scrollY;
    public ZoomCommand(PlayerController playerController, float scrollY)
    {
        _playerController = playerController;
        _scrollY = scrollY;
    }

    public void Execute()
    {
        _playerController.Zoom(_scrollY);
    }
}