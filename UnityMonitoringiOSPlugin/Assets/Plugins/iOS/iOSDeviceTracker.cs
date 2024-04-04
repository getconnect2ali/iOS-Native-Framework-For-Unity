using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Plugins.iOS.TrackingUsage
{
    public class iOSDeviceTracker : MonoBehaviour
    {
        #region Singleton
        private static iOSDeviceTracker _instance;
        public static iOSDeviceTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<iOSDeviceTracker>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("iOSDeviceTracker").AddComponent<iOSDeviceTracker>();
                    }
                }
                return _instance;
            }
        }
        #endregion

#if UNITY_IOS
        
        /// Native Method - Start Tracking the Device
        [ DllImport( "__Internal" ) ]
        private static extern void startTracking();

        /// Native Method - Stop Tracking the Device

        [DllImport("__Internal")]
        private static extern string stopTracking();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Native_OnStatReceived([MarshalAs(UnmanagedType.LPStr), In] string stat );

        [ MonoPInvokeCallback( typeof( Native_OnStatReceived ) ) ]
        private static void BrdigeToNative_OnStatReceived( string message )
        {
            Instance.Bridge_OnStatReceived( JsonUtility.FromJson< Stat >( message ) );
        }

        /// Native Method - Start Tracking the Device with Interval

        /// <param name="onStatReceived">Pointer to callback handler</param>
        [ DllImport( "__Internal" ) ]
        private static extern void startTrackingWithInterval(
            [ MarshalAs( UnmanagedType.FunctionPtr ) ] Native_OnStatReceived onStatReceived );
        
        /// Action to handle the received stat from native
        public Action<Stat> Bridge_OnStatReceived;
        

        /// Native Method - Stop Tracking the Device
        [DllImport("__Internal")]
        private static extern void stopTrackingWithInterval();

        /// Current status of tracking
        private bool isTracking = false;

        /// Start tracking the device
        public bool StartTracking()
        {
            if ( isTracking ) return false;
            startTracking();
            isTracking = true;
            return true;
        }

        /// Stop tracking the device
        public Stat StopTracking()
        {
            if ( !isTracking ) return null;
            var stat = JsonUtility.FromJson< Stat >( stopTracking() );
            isTracking = false;
            return stat;
        }
        
        /// Start tracking the device with interval mode
        public bool StartTrackingWithInterval( Action< Stat > onStatReceived )
        {
            if ( isTracking ) return false;
            startTrackingWithInterval( BrdigeToNative_OnStatReceived );
            Bridge_OnStatReceived += onStatReceived;
            isTracking = true;
            return true;
        }
       

        /// Stop tracking the device with interval mode
        public void StopTrackingWithInterval()
        {
            if ( !isTracking ) return;
            stopTrackingWithInterval();
            Bridge_OnStatReceived = null;
            isTracking = false;
        }
        
        /// Check the current device is under tracking or not
        public bool IsNowTracking() => isTracking;

#endif
    }
}
