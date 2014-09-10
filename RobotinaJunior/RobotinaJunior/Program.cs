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

        private bool takingPicture;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");

            client = bluetooth.ClientMode;
            bluetooth.SetDeviceName("RobotinaJr");
            bluetooth.SetPinCode("4321");
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
        }
        void bluetooth_DataReceived(Bluetooth sender, string data)
        {
            Debug.Print("received data");
            if (data == "takePicture")
            {
                TakePicture();
            }
            else
            {
                try
                {
                    string[] ind_data = data.Split(';');
                    int left_speed = Int32.Parse(ind_data[0]);
                    int right_speed = Int32.Parse(ind_data[1]);
                    cerbotController.SetMotorSpeed(left_speed, right_speed);
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        public void TakePicture()
        {
            if (! takingPicture)
            {
                Debug.Print("Taking Picture");
                Thread cameraThread = new Thread(new ThreadStart(CameraThread));
                cameraThread.Start();
            }

        }

        public void CameraThread()
        {
            this.serCam.StartStreaming();

            while( ! (serCam.isNewImageReady && bluetooth.IsConnected) )
            {
                Thread.Sleep(10);
            }
            byte[] bytes = serCam.dataImage;
            if (bytes.Length != _lastSize)
            {
                Debug.Print(bytes.Length.ToString());
                client.Send(IntegerToByteArray(bytes.Length));
                client.Send(bytes);
                _lastSize = bytes.Length;
                _timeOfLastRefresh = DateTime.Now.Ticks;

                serCam.StopStreaming();
                serCam.StartStreaming();

                takingPicture = false;
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
