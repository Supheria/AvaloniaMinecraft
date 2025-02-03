using Avalonia.Controls;

namespace AvaMc.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        Game.FrameInfoUpdated += FrameInfoUpdated;
    }

    private void FrameInfoUpdated(object? sender, GlEsControl.FrameInfo e)
    {
        Text.Text = e.ToString();
    }
}