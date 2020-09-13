using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoRecorder
{
    public partial class App : Application
    {
        public static IContainer Container { set; get; }
        public static readonly ContainerBuilder Builder = new ContainerBuilder();
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        public static void BuildContainer()
        {
            Container = Builder.Build();
        }

        protected override void OnStart() { }

        protected override void OnSleep() { }

        protected override void OnResume() { }

    }
}
