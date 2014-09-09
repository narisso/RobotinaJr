using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.Velloso;
using Gadgeteer.Modules.GHIElectronics;

namespace RobotinaJunior
{
    public partial class Program
    {
        Bluetooth bluetooth = new Bluetooth(5);
        Bluetooth.Client client;
        private int _lastSize;
        private long _timeOfLastRefresh;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");

            client = bluetooth.ClientMode;
            bluetooth.SetDeviceName("Gadgeteer");
            bluetooth.SetPinCode("1234");
            bluetooth.DataReceived += new Bluetooth.DataReceivedHandler(bluetooth_DataReceived);

            this.serCam.StartStreaming();

            GT.Timer timer = new GT.Timer(15000);
            timer.Behavior = GT.Timer.BehaviorType.RunOnce;
            timer.Tick += initialize;
            timer.Start();
        }
        private void initialize(GT.Timer timer)
        {
            //Debug.Print("hla");
            client.EnterPairingMode();
            Thread cameraThread = new Thread(new ThreadStart(CameraThread));
            cameraThread.Start();
        }
        void bluetooth_DataReceived(Bluetooth sender, string data)
        {
            string[] ind_data = data.Split(';');
            int left_speed = Int32.Parse(ind_data[0]);
            int right_speed = Int32.Parse(ind_data[1]);

            Debug.Print("received data");
            cerbotController.SetMotorSpeed(left_speed, right_speed);
        }
        public void CameraThread()
        {
            //_bitmap = new Bitmap(320, 240);
            this.serCam.StartStreaming();
            while (true)
            {
                Debug.Print("SerCamready: " + serCam.isNewImageReady);
                Debug.Print("bluethootConnected: " + bluetooth.IsConnected);

                if (serCam.isNewImageReady && bluetooth.IsConnected)
                {
                    //serCam.DrawImage(_bitmap, 0, 0, 320, 240);
                    byte[] bytes = serCam.dataImage;
                    if (bytes.Length != _lastSize)
                    {
                        byte[] arr=IntegerToByteArray(bytes.Length);
                        Debug.Print(arr.ToString());
                        client.Send(arr.ToString());
                        _lastSize = bytes.Length;
                        _timeOfLastRefresh = DateTime.Now.Ticks;

                        serCam.StopStreaming();
                        serCam.StartStreaming();
                    }
                }
                Thread.Sleep(200);
            }
        }
        private byte[] IntegerToByteArray(int length)
        {
            byte[] bytes = new byte[3];
            bytes[2] = (byte)(length % 256);
            length /= 256;
            bytes[1] = (byte)(length % 256);
            length /= 256;
            bytes[0] = (byte)(length % 256);
            return bytes;
        }
    }
}
