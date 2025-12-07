using MauiApp = Microsoft.Maui.Controls.Application;

namespace HolyConnect.Maui;

public partial class App : MauiApp
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "HolyConnect" };
    }
}
