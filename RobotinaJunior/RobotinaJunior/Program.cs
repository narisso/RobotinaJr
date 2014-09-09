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
        Bluetooth bluetooth = new Bluetooth(3);
        Bluetooth.Client client;


        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");

            client = bluetooth.ClientMode;
            bluetooth.SetDeviceName("Gadgeteer");
            bluetooth.SetPinCode("1234");
            bluetooth.DataReceived += new Bluetooth.DataReceivedHandler(bluetooth_DataReceived);
            
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
            string[] ind_data = data.Split(';');
            int left_speed = Int32.Parse(ind_data[0]);
            int right_speed = Int32.Parse(ind_data[0]);

            Debug.Print("received data");
            cerbotController.SetMotorSpeed(left_speed, right_speed);
        }
    }
}
