using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Xamarin.Forms;

namespace VideoRecorder
{
    public partial class MainPage : ContentPage
    {
        bool isrecording = false;
        IVideoRecorderService videoRecorderService;
        public MainPage()
        {
            InitializeComponent();
            videoRecorderService = App.Container.Resolve<IVideoRecorderService>();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!isrecording)
            {
                videoRecorderService.StartScreenRecording();
                ((Button)sender).Text = "Stop Recording!";
                isrecording = !isrecording;
            }
            else
            {
                videoRecorderService.StopRecording();
                ((Button)sender).Text = "Start Recording!";
                isrecording = !isrecording;
            }
        }
    }
}
