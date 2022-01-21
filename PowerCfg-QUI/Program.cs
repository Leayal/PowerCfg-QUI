using Microsoft.VisualBasic.ApplicationServices;

namespace Leayal.PowerCfg_QUI
{
    internal static class Program
    {
        public readonly static AppController Controller;

        static Program()
        {
            ApplicationConfiguration.Initialize();
            Controller = new AppController();
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Controller.Run(args);
        }
    }

    class AppController : WindowsFormsApplicationBase
    {
        public AppController() : base()
        {
            this.IsSingleInstance = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            this.MainForm = new MyMainMenu();
            return base.OnStartup(eventArgs);
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            eventArgs.BringToForeground = true;
            base.OnStartupNextInstance(eventArgs);
        }
    }
}