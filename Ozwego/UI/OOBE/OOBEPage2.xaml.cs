using Ozwego.Server;
using Ozwego.Server.MessageProcessors;
using Ozwego.Storage;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ozwego.UI.OOBE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OOBEPage2 : Page
    {
        private string _enteredAlias;

        public OOBEPage2()
        {
            this.InitializeComponent();
        }

        private async void CheckIfAvailableClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DataBaseMessageProcessor.DataBaseMessageReceivedEvent += AliasAvailableCallback;

            _enteredAlias = AliasTextBox.Text;

            var serverProxy = ServerProxy.GetInstance();

            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientQueryIfAliasAvailable, _enteredAlias);
            }
        }


        private void AliasAvailableCallback(object sender, string message)
        {
            if (message == "true")
            {
                Settings.Alias = _enteredAlias;
                PopUpStatus.Text = "Alias Available and Set!";
            }
            else
            {
                PopUpStatus.Text = "Alias Taken!";
            }

            // ToDo: Once the alias has been accepted, then upload it to the server.
        }

        private void ShowPopupOffsetClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            //if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
        }


        private void CloseButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }
    }
}
