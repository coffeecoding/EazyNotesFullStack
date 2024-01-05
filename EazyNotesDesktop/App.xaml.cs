using EazyNotesDesktop.Library.DAO;
using EazyNotesDesktop.Library.Helpers;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.ViewModels;
using EazyNotesDesktop.Views;
using Microsoft.Win32;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using Unity;

namespace EazyNotesDesktop
{
    public enum Theme
    {
        Dark, 
        Light
    }

    public partial class App
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public static IUnityContainer AppContainer { get; set; }

        protected override Window CreateShell()
        {
            Log.Info("Creating Shell");

            Log.Info("Setting working directory to the executables directory");
            Log.Info($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            Log.Info($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            // set the current working dir to the executables dir in case the app was opened via the command line
            // (from Boxcryptor)
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            ShutdownMode = ShutdownMode.OnLastWindowClose;

            SetTheme();
            SetLanguageDictionary();


            Exit += (s, e) => RunExitServices(s, e);

            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Log.Info("\n");
            Log.Info("~~~ RUNNING EAZYNOTES ~~~");
            Log.Info("Registering types");
            //containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            AppContainer = containerRegistry.GetContainer();

            containerRegistry.Register<ICryptoRoutines, CryptoRoutines>();

            containerRegistry.RegisterSingleton<IUserData, UserData>();
            containerRegistry.RegisterSingleton<ENClient>();
            containerRegistry.RegisterSingleton<APIClient>();
            containerRegistry.RegisterSingleton<SQLiteClient>();
            containerRegistry.RegisterSingleton<DataAccess>();

            // Weirdly enough, IRegionManager injection works without registering a singleton here...
            //containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
            containerRegistry.Register<LoginViewModel>();
            containerRegistry.RegisterForNavigation<Login>();
            containerRegistry.Register<SignUpViewModel>();
            containerRegistry.RegisterForNavigation<SignUp>();
        }

        private void RunExitServices(object sender, ExitEventArgs e)
        {
            Log.Info($"Exiting app with code {e.ApplicationExitCode}");
            Log.Info($"Finished running exit services");
        }
        
        public void SetLanguageDictionary()
        {
            EazyNotes.Language.Resources.Culture = (Thread.CurrentThread.CurrentCulture.ToString()) switch
            {
                "de-DE" => new System.Globalization.CultureInfo("de-DE"),
                "en-US" => new System.Globalization.CultureInfo("en-US"),
                //default english because there can be so many different system language, we rather fallback on english in this case.
                _ => new System.Globalization.CultureInfo("en-US"),
            };
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // TODO: adjust this if we need to add modules such as EazyNotesDesktop.Library
            base.ConfigureModuleCatalog(moduleCatalog);
            //moduleCatalog.AddModule<ModuleNameModule>();
        }

        #region Theme Logic
        private static Theme Skin { get; set; } = Theme.Dark;

        private const string registryThemeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string registryThemeKeyName = "AppsUseLightTheme";
        private const string registryUsesAccentKeyPath = @"Software\Microsoft\Windows\DWM";
        private const string registryUsesAccentKeyName = "ColorPrevalence";
        private const string registryAccentColorKey = "ColorizationColor";

        private void SetTheme()
        {
            //ApplyWindowsThemeSettings();
            //WatchTheme();
        }

        private Theme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryThemeKeyPath))
            {
                object registryValueObject = key?.GetValue(registryThemeKeyName);
                if (registryValueObject == null)
                {
                    return Theme.Dark;
                }
                int registryValue = (int)registryValueObject;
                return registryValue > 0 ? Theme.Light : Theme.Dark;
            }
        }

        private bool CheckWindowsUsesAccentColor()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryUsesAccentKeyPath))
            {
                object registryValueObject = key?.GetValue(registryUsesAccentKeyName);
                if (registryValueObject == null)
                {
                    return false;
                }
                int value = (int)registryValueObject;
                return value == 1;
            }
        }

        public void ApplyTheme(Theme theme)
        {
            string themeStr = theme == Theme.Dark ? "Dark" : "Light";
            string accent = CheckWindowsUsesAccentColor() ? "WinAccent" : "";
            string assemblyRelativeResourcePath = $"Views/Colors/Colors{themeStr}{accent}.xaml";
            Resources.MergedDictionaries[0].Source = new Uri(assemblyRelativeResourcePath, UriKind.Relative);
        }

        public void ApplyWindowsThemeSettings()
        {
            Skin = GetWindowsTheme();
            string theme = Skin == Theme.Dark ? "Dark" : "Light";
            string accent = CheckWindowsUsesAccentColor() ? "WinAccent" : "";
            string assemblyRelativeResourcePath = $"Views/Colors/Colors{theme}{accent}.xaml";
            Resources.MergedDictionaries[0].Source = new Uri(assemblyRelativeResourcePath, UriKind.Relative);
        }

        private void WatchTheme()
        {
            var CurrentUser = WindowsIdentity.GetCurrent();
            string queryTheme = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                CurrentUser.User.Value, registryThemeKeyPath.Replace(@"\", @"\\"), registryThemeKeyName);

            string queryUsesAccentColor = string.Format(
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                CurrentUser.User.Value, registryUsesAccentKeyPath.Replace(@"\", @"\\"), registryUsesAccentKeyName);

            string queryAccentColor = string.Format(
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                CurrentUser.User.Value, registryUsesAccentKeyPath.Replace(@"\", @"\\"), registryAccentColorKey);

            try
            {
                var themeWatcher = new ManagementEventWatcher(queryTheme);
                themeWatcher.EventArrived += (s, e) =>
                {
                    (Current as App)?.ApplyWindowsThemeSettings();
                };
                themeWatcher.Start();

                var usesAccentWatcher = new ManagementEventWatcher(queryUsesAccentColor);
                usesAccentWatcher.EventArrived += (s, e) =>
                {
                    (Current as App)?.ApplyWindowsThemeSettings();
                };
                usesAccentWatcher.Start();

                var accentColorWatcher = new ManagementEventWatcher(queryAccentColor);
                accentColorWatcher.EventArrived += (s, e) =>
                {
                    (Current as App)?.ApplyWindowsThemeSettings();
                };
                accentColorWatcher.Start();
            } 
            catch(Exception e)
            {
                // TODO: Log
                Console.WriteLine(e);
            }
        }
        #endregion
    }
}
