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
using DJI.WindowsSDK;
using DJIVideoParser;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DJISDKDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DJIVideoParser.Parser videoParser;
        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("ba7e2142664cf62b2a46275e");

        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                System.Diagnostics.Debug.WriteLine("Register app successfully.");

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    //Raw data and decoded data listener
                    if (videoParser == null)
                    {
                        System.Diagnostics.Debug.WriteLine("videoParser null.");
                        videoParser = new Parser();
                        videoParser.Initialize(delegate (byte[] data)
                        {
                            //Note: This function must be called because we need DJI Windows SDK to help us to parse frame data.
                            return DJISDKManager.Instance.VideoFeeder.ParseAssitantDecodingInfo(0, data);
                        });
                        //Set the swapChainPanel to display and set the decoded data callback.
                        videoParser.SetSurfaceAndVideoCallback(0, 0, swapChainPanel, ReceiveDecodedData);
                        DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated += OnVideoPush;
                    }
                    //get the camera type and observe the CameraTypeChanged event.
                    System.Diagnostics.Debug.WriteLine("working.");
                    DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).CameraTypeChanged += OnCameraTypeChanged;
                    var type = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetCameraTypeAsync();
                    OnCameraTypeChanged(this, type.value);
                });

                /*
                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    System.Diagnostics.Debug.WriteLine("awaiting answer");
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        System.Diagnostics.Debug.WriteLine("evaluating");
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                            //You can load/display your pages according to the aircraft connection state here.
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is disconnected now.");
                            //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                        }
                    });
                };

                //If you want to get the latest product connection state manually, you can use the following code
                var productType = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
                if (productType != null && productType?.value != ProductType.UNRECOGNIZED)
                {
                    System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                    //You can load/display your pages according to the aircraft connection state here.
                }
                */
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Register SDK failed, the error is: ");
                System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            }
        }

        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        //Decode data. Do nothing here. This function would return a bytes array with image data in RGBA format.
        async void ReceiveDecodedData(byte[] data, int width, int height)
        {
        }

        //We need to set the camera type of the aircraft to the DJIVideoParser. After setting camera type, DJIVideoParser would correct the distortion of the video automatically.
        private void OnCameraTypeChanged(object sender, CameraTypeMsg? value)
        {
            if (value != null)
            {
                switch (value.Value.value)
                {
                    case CameraType.MAVIC_2_ZOOM:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Zoom);
                        System.Diagnostics.Debug.WriteLine("other camera.");
                        break;
                    case CameraType.MAVIC_2_PRO:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Pro);
                        break;
                    default:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Others);
                        break;
                }

            }
        }
    }
}
