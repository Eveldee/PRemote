using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using PRemote.Shared;
using MessagePack;

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

        IPEndPoint _ipServer;
        NetworkStream _networkStream;

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
            // Serialize ISO
            byte[] iso = MessagePackSerializer.Serialize(new PPacket(PDataType.ISO, spr_ISO.SelectedItem));
            _networkStream.Write(iso, 0, iso.Length);

            // Serialize Aperture
            byte[] aperture = MessagePackSerializer.Serialize(new PPacket(PDataType.Aperture, spr_Aperture.SelectedItem));
            _networkStream.Write(aperture, 0, aperture.Length);

            // Serialize Shutter
            byte[] shutter = MessagePackSerializer.Serialize(new PPacket(PDataType.ShutterSpeed, spr_Shutter.SelectedItem));
            _networkStream.Write(shutter, 0, shutter.Length);
        }



        // Receive UDP broadcast to get discovered IP
        private void UDP_Thread()
        {
            UdpClient udpClient = new UdpClient(0)
            {
                EnableBroadcast = true
            };

            var ipEndPoint = new IPEndPoint(IPAddress.Any, PConnection.UDPPort);

            while (true)
            {
                bool packetEqual = true;
                byte[] data = udpClient.Receive(ref _ipServer);

                // Check if the packet is the good one
                if (data.Length == PConnection.UDPPacketDataLenght)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != PConnection.UDPPacketData[i])
                        {
                            packetEqual = false;
                        }
                    }

                    // This is the good IP
                    if (packetEqual)
                    {
                        _ipServer = ipEndPoint;
                        break;
                    }
                }
            }

            //Tcp connection
            Thread tcp = new Thread(TCP_ConnectionThread);
            tcp.Start();
        }

        // Send PPacket to Server
        private void TCP_ConnectionThread()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(_ipServer);

            _networkStream = tcpClient.GetStream();
        }
    }
}

