using GT = Gadgeteer;

namespace GadgeteerCamera
{
    class BreakOut
    {
        public GT.SocketInterfaces.DigitalInput rightForwardSensor;
        public GT.SocketInterfaces.DigitalInput leftForwardSensor;
        //public GT.SocketInterfaces.DigitalInput rightBackwardSensor;
        //public GT.SocketInterfaces.DigitalInput leftBackwardSensor;
        private GT.Socket socket;
        public bool stop;


        public BreakOut()
        {
            socket = GT.Socket.GetSocket(9, true, null, null);
            //this.leftBackwardSensor = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Nine, GT.SocketInterfaces.GlitchFilterMode.On, GT.SocketInterfaces.ResistorMode.Disabled, null);
            this.leftForwardSensor = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Four, GT.SocketInterfaces.GlitchFilterMode.On, GT.SocketInterfaces.ResistorMode.Disabled, null);
            this.rightForwardSensor = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Five, GT.SocketInterfaces.GlitchFilterMode.On, GT.SocketInterfaces.ResistorMode.Disabled, null);
            //this.rightBackwardSensor = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Five, GT.SocketInterfaces.GlitchFilterMode.On, GT.SocketInterfaces.ResistorMode.Disabled, null);
        }

    }
}
