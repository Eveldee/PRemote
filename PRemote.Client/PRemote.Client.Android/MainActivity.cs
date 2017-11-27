using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using PRemote.Shared;
using MessagePack;
using System.Linq;

namespace PRemote.Client.Android
{
    /// <summary>
    /// Main Activity, all is in it
    /// </summary>
    [Activity(Label = "PRemote", MainLauncher = true, Theme = "@android:style/Theme.Material")]
    public class MainActivity : Activity
    {
        TextView txt_State;
        ImageView img_State;
        Spinner spr_ISO;
        Spinner spr_Aperture;
        Spinner spr_Shutter;
        Button btn_Picture;
        Button btn_Vocal;

        IPEndPoint _serverIp;
        PPacketStream _packetStream;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Register View
            txt_State    = FindViewById<TextView>(Resource.Id.txt_State);
            img_State    = FindViewById<ImageView>(Resource.Id.img_State);
            spr_ISO      = FindViewById<Spinner>(Resource.Id.spr_ISO);
            spr_Aperture = FindViewById<Spinner>(Resource.Id.spr_Aperture);
            spr_Shutter  = FindViewById<Spinner>(Resource.Id.spr_Shutter);
            btn_Picture  = FindViewById<Button>(Resource.Id.btn_Picture);
            btn_Vocal    = FindViewById<Button>(Resource.Id.btn_Vocal);

            spr_ISO.ItemClick += Spr_ItemClick;
            spr_Aperture.ItemClick += Spr_ItemClick;
            spr_Shutter.ItemClick += Spr_ItemClick;

            btn_Picture.Click += Btn_Picture_Click;
            btn_Vocal.Click += Btn_Vocal_Click;

            Thread t = new Thread(UDP_Thread);
            t.Start();
        }

        private void Btn_Vocal_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Btn_Picture_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        // Send Config
        private void Spr_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            throw new System.NotImplementedException();
        }



        // Receive UDP broadcast to get discovered IP
        private void UDP_Thread()
        {
            UdpClient udpClient = new UdpClient(PConnection.UDPPort)
            {
                EnableBroadcast = true
            };

            _serverIp = new IPEndPoint(IPAddress.Any, PConnection.UDPPort);
            byte[] data;

            // Wait that the server send the right UDP packet
            while (true)
            {
                data = udpClient.Receive(ref _serverIp);

                if (data.SequenceEqual(PConnection.UDPPacketData))
                    break;
            }

            //Tcp connection
            TCP_Connection();
        }

        // Connect to server
        private void TCP_Connection()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(_serverIp);

            _packetStream = new PPacketStream(tcpClient.GetStream());
        }
    }
}

