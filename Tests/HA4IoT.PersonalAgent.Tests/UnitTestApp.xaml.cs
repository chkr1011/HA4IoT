using Windows.ApplicationModel.Activation;

namespace HA4IoT.PersonalAgent.Tests
{
    sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(e.Arguments);
        }
    }
}
