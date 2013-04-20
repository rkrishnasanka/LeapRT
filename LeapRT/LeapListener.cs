using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace LeapRT
{
    public enum LeapDeviceState
	{
	    initialized = 0,
        connected = 1,
        disconnected = 2,
        exited = 3
	}

    public class LeapListenerArgs : EventArgs
    {
        #region Public Properties

        public string logdata { get; set; }

        public LeapDeviceState devicestatus { get; set; }

        public Frame currentframe { get; set; }

        public SwipeGesture swipegesture { get; set; }

        public KeyTapGesture keytapgesture { get; set; }

        public CircleGesture circlegesture { get; set; }

        public ScreenTapGesture screentapgesture { get; set; }

        #endregion

        #region Constructors
        
        public LeapListenerArgs(Frame frame)
        {
            this.currentframe = frame;
        }

        public LeapListenerArgs(LeapDeviceState status)
        {
            this.devicestatus = status;
        }

        public LeapListenerArgs(string line)
        {
            this.logdata = line;
        }

        #region Gesture Related
        
        public LeapListenerArgs(SwipeGesture gesture)
        {
            this.swipegesture = gesture;
        }

        public LeapListenerArgs(KeyTapGesture gesture)
        {
            this.keytapgesture = gesture;
        }

        public LeapListenerArgs(ScreenTapGesture gesture)
        {
            this.screentapgesture = gesture;
        }

        public LeapListenerArgs(CircleGesture gesture)
        {
            this.circlegesture = gesture;
        }
        
        #endregion
        
        #endregion
    }

    public class LeapListener : Listener
    {

        #region Event Definitons

        public delegate void LogUpdateHandler(object sender, LeapListenerArgs e);
        public event LogUpdateHandler OnLogUpdate;

        public delegate void DeviceStatusUpdateHandler(object sender, LeapListenerArgs e);
        public event DeviceStatusUpdateHandler OnDeviceStatusUpdate;

        public delegate void FrameUpdateHandler(object sender, LeapListenerArgs e);
        public event FrameUpdateHandler OnFrameUpdate;

        public delegate void KeyTapGestureHandler(object sender, LeapListenerArgs e);
        public event KeyTapGestureHandler OnKeyTapGesture;

        public delegate void CircleGestureHandler(object sender, LeapListenerArgs e);
        public event CircleGestureHandler OnCircleGesture;

        public delegate void ScreenTapGestureHandler(object sender, LeapListenerArgs e);
        public event ScreenTapGestureHandler OnScreenTapGesture;

        public delegate void SwipeGuestureHandler(object sender, LeapListenerArgs e);
        public event SwipeGuestureHandler OnSwipeGesture;

        #endregion

        #region Event Dispatching Methods

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                // System.Diagnostics.Debug.WriteLine(line);

                System.Diagnostics.Debug.WriteLine("Leap Output:" + line);

                //fires the event only in the case of an event handler being present
                if (OnLogUpdate == null) return;

                LeapListenerArgs args = new LeapListenerArgs(line);
                OnLogUpdate(this, args);
            }
        }

        private void UpdateDeviceStatus(LeapDeviceState status)
        {
            lock (thisLock)
            {

                //fires the event only in the case of an event handler being present
                if (OnDeviceStatusUpdate == null) return;

                LeapListenerArgs args = new LeapListenerArgs(status);
                OnDeviceStatusUpdate(this, args);
            }
        }

        private void UpdateFrame(Frame frame)
        {
            lock (thisLock)
            {
                //fires the event only in the case of an event handler being present
                if (OnFrameUpdate == null) return;

                LeapListenerArgs args = new LeapListenerArgs(frame);
                OnFrameUpdate(this, args);
            }
        }


        #endregion

        #region Leap Default Stuff

        private Object thisLock = new Object();

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");

            UpdateDeviceStatus(LeapDeviceState.initialized);
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");

            UpdateDeviceStatus(LeapDeviceState.connected);

            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            SafeWriteLine("Disconnected");

            UpdateDeviceStatus(LeapDeviceState.disconnected);
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");

            UpdateDeviceStatus(LeapDeviceState.exited);
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();

            SafeWriteLine("Frame id: " + frame.Id
                        + ", timestamp: " + frame.Timestamp
                        + ", hands: " + frame.Hands.Count
                        + ", fingers: " + frame.Fingers.Count
                        + ", tools: " + frame.Tools.Count
                        + ", gestures: " + frame.Gestures().Count);

            UpdateFrame(frame);

            if (!frame.Hands.Empty)
            {
                // Get the first hand
                Hand hand = frame.Hands[0];

                // Check if the hand has any fingers
                FingerList fingers = hand.Fingers;
                if (!fingers.Empty)
                {
                    // Calculate the hand's average finger tip position
                    Vector avgPos = Vector.Zero;
                    foreach (Finger finger in fingers)
                    {
                        avgPos += finger.TipPosition;
                    }
                    avgPos /= fingers.Count;
                    //SafeWriteLine("Hand has " + fingers.Count
                    //            + " fingers, average finger tip position: " + avgPos);
                }

                // Get the hand's sphere radius and palm position
                //SafeWriteLine("Hand sphere radius: " + hand.SphereRadius.ToString("n2")
                //            + " mm, palm position: " + hand.PalmPosition);

                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;

                // Calculate the hand's pitch, roll, and yaw angles
                //SafeWriteLine("Hand pitch: " + direction.Pitch * 180.0f / (float)Math.PI + " degrees, "
                //            + "roll: " + normal.Roll * 180.0f / (float)Math.PI + " degrees, "
                //            + "yaw: " + direction.Yaw * 180.0f / (float)Math.PI + " degrees");
            }

            // Get gestures


            GestureList gestures = frame.Gestures();
            for (int i = 0; i < gestures.Count; i++)
            {
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPECIRCLE:

                        if (OnCircleGesture == null) break;

                        CircleGesture circle = new CircleGesture(gesture);

                        // Calculate clock direction using the angle between circle normal and pointable
                        String clockwiseness;
                        if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4)
                        {
                            //Clockwise if angle is less than 90 degrees
                            clockwiseness = "clockwise";
                        }
                        else
                        {
                            clockwiseness = "counterclockwise";
                        }

                        float sweptAngle = 0;

                        // Calculate angle swept since last frame
                        if (circle.State != Gesture.GestureState.STATESTART)
                        {
                            CircleGesture previousUpdate = new CircleGesture(controller.Frame(1).Gesture(circle.Id));
                            sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
                        }

                        var cargs = new LeapListenerArgs(circle);
                        
                        cargs.logdata = "Circle id: " + circle.Id
                                       + ", " + circle.State
                                       + ", progress: " + circle.Progress
                                       + ", radius: " + circle.Radius
                                       + ", angle: " + sweptAngle
                                       + ", " + clockwiseness;

                        OnCircleGesture(this, cargs);

                        //SafeWriteLine("Circle id: " + circle.Id
                        //               + ", " + circle.State
                        //               + ", progress: " + circle.Progress
                        //               + ", radius: " + circle.Radius
                        //               + ", angle: " + sweptAngle
                        //               + ", " + clockwiseness);


                        break;
                    case Gesture.GestureType.TYPESWIPE:

                        if (OnSwipeGesture == null) break;

                        SwipeGesture swipe = new SwipeGesture(gesture);

                        var sargs = new LeapListenerArgs(swipe);
                        
                         sargs.logdata = "Swipe id: " + swipe.Id
                                       + ", " + swipe.State
                                       + ", position: " + swipe.Position
                                       + ", direction: " + swipe.Direction
                                       + ", speed: " + swipe.Speed;

                         

                        OnSwipeGesture(this, sargs);

                        //SafeWriteLine("Swipe id: " + swipe.Id
                        //               + ", " + swipe.State
                        //               + ", position: " + swipe.Position
                        //               + ", direction: " + swipe.Direction
                        //               + ", speed: " + swipe.Speed);
                        break;
                    case Gesture.GestureType.TYPEKEYTAP:

                        if (OnKeyTapGesture == null) break;

                        KeyTapGesture keytap = new KeyTapGesture(gesture);

                        var kargs = new LeapListenerArgs(keytap);

                        kargs.logdata = "Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction;

                        OnKeyTapGesture(this, kargs);

                        //SafeWriteLine("Tap id: " + keytap.Id
                        //               + ", " + keytap.State
                        //               + ", position: " + keytap.Position
                        //               + ", direction: " + keytap.Direction);
                        break;
                    case Gesture.GestureType.TYPESCREENTAP:

                        if (OnScreenTapGesture == null) break;

                        ScreenTapGesture screentap = new ScreenTapGesture(gesture);

                        var targs = new LeapListenerArgs(screentap);

                        targs.logdata = "Tap id: " + screentap.Id
                                       + ", " + screentap.State
                                       + ", position: " + screentap.Position
                                       + ", direction: " + screentap.Direction;

                        OnScreenTapGesture(this, targs);

                        //SafeWriteLine("Tap id: " + screentap.Id
                        //               + ", " + screentap.State
                        //               + ", position: " + screentap.Position
                        //               + ", direction: " + screentap.Direction);
                        break;
                    default:
                        //SafeWriteLine("Unknown gesture type.");
                        break;
                }
            }

            if (!frame.Hands.Empty || !frame.Gestures().Empty)
            {
                //SafeWriteLine("");
            }
        }
    }

        #endregion
}
