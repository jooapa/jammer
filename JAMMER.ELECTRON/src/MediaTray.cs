#if WINDOWS && USE_FORMS
// using System.Windows;
// using System.Windows.Media;

namespace jammer;
public static class MediaTray{
    // public static void MediaTray()
    // {
    //     // this.InitializeComponent();

    //     systemControls = SystemMediaTransportControls.GetForCurrentView();

    //     // Register to handle the following system transpot control buttons.
    //     systemControls.ButtonPressed += SystemControls_ButtonPressed;

    //     mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;


    //     systemControls.IsPlayEnabled = true;
    //     systemControls.IsPauseEnabled = true;
    // }

    // private static void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
    // {
    //     switch (mediaElement.CurrentState)
    //     {
    //         case MediaElementState.Playing:
    //             systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
    //             break;
    //         case MediaElementState.Paused:
    //             systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
    //             break;
    //         case MediaElementState.Stopped:
    //             systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
    //             break;
    //         case MediaElementState.Closed:
    //             systemControls.PlaybackStatus = MediaPlaybackStatus.Closed;
    //             break;
    //         default:
    //             break;
    //     }
    // }

    // static void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    // {
    //     switch (args.Button)
    //     {
    //         case SystemMediaTransportControlsButton.Play:
    //             PlayMedia();
    //             break;
    //         case SystemMediaTransportControlsButton.Pause:
    //             PauseMedia();
    //             break;
    //         case SystemMediaTransportControlsButton.Stop:
    //             StopMedia();
    //             break;
    //         default:
    //             break;
    //     }
    // }

    // private static async void StopMedia()
    // {
    //     await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    //     {
    //         mediaElement.Stop();
    //     });
    // }

    // static async void PlayMedia()
    // {
    //     await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    //     {
    //         if (mediaElement.CurrentState == MediaElementState.Playing)
    //             mediaElement.Pause();
    //         else
    //             mediaElement.Play();
    //     });
    // }

    // static async void PauseMedia()
    // {
    //     await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    //     {
    //         mediaElement.Pause();
    //     });
    // }

}
#endif