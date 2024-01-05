using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using MvvmHelpers.Commands;
using EazyNotesDesktop.Library.Helpers;
using EazyNotesDesktop.Views;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.Library.DAO;
using System.Net.Http;
using EazyNotesDesktop.Util;
using EazyNotes.CryptoServices;

namespace EazyNotesDesktop.ViewModels
{
    public class LoginViewModel : BindableBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private readonly DataAccess _dataAccess;
        private readonly ENClient _enClient;
        private readonly IRegionManager _regionManager;

        public LoginViewModel(DataAccess dataAccess, ENClient enClient, IRegionManager regionManager)
        {
            _dataAccess = dataAccess;
            _enClient = enClient;
            _regionManager = regionManager;

            LoginCommand = new AsyncCommand(Login, (x) => CanLogin);
            NavigateToSignUpCommand = new DelegateCommand(NavigateToSignUp, () => true);

            NavigateToMainCommand = new DelegateCommand(() => {
                Main mainWindow = new Main();
                mainWindow.Show();
            });
        }

        public string Title => "Login";

        public event EventHandler RequestShowAlertBox;
        public event EventHandler RequestParentClose;

        public DelegateCommand NavigateToMainCommand { get; private set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set {
                SetProperty(ref _isBusy, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private DelegateCommand _navigateToSignUpCommand;
        public DelegateCommand NavigateToSignUpCommand
        {
            get { return _navigateToSignUpCommand; }
            set { SetProperty(ref _navigateToSignUpCommand, value); }
        }

        private AsyncCommand _loginCommand;
        public AsyncCommand LoginCommand
        {
            get { return _loginCommand; }
            set { SetProperty(ref _loginCommand, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set 
            { 
                SetProperty(ref _username, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        // TODO: Remove the private backing field: It creates another clear copy of pw in memory
        // TODO: Replace with SecureString: Do it like so:
        //   When Text of PasswordBox changes, append the new character to secure string (or remove it)
        //   When finished, compute the hash and immediately clear the secure string by calling .Dispose() on it!
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _apiLoginResult;
        public string ApiLoginResult
        {
            get { return _apiLoginResult; }
            set { SetProperty(ref _apiLoginResult, value); }
        }

        private string _sqLiteLoginResult;
        public string SQLiteLoginResult
        {
            get { return _sqLiteLoginResult; }
            set { SetProperty(ref _sqLiteLoginResult, value); }
        }

        private string _welcomeMsg;
        public string WelcomeMsg
        {
            get { return _welcomeMsg; }
            set { SetProperty(ref _welcomeMsg, value); }
        }

        public bool CanLogin
        {
            get
            {
                return !IsBusy && Password?.Length > 0 && Username?.Length > 0;
            }
        }

        public async Task Login()
        {
            try
            {
                IsBusy = true;
                ApiLoginResult = "";

                AuthenticationResult apiResult = null;
                Exception ex = null;
                try
                {
                    apiResult = await _enClient.LoginApiClient(Username, Password);
                }
                catch (HttpRequestException e)
                {
                    ex = e;
                    string msg = "Couldn't reach server.";
                    ApiLoginResult = $"Warning: Cloud login failed with following error message: {msg}.\nYour data won't be synced.";
                    Log.Warn(e);
                }
                finally
                {
                    if (apiResult == null || !apiResult.Success)
                    {
                        ApiLoginResult = $"Warning: Cloud login failed with following error message: {(apiResult== null ? ex.Message : apiResult.Message)}.\nYour data won't be synced.";
                        RequestShowAlertBox?.Invoke(new AlertBoxArgs(ApiLoginResult), null);
                    }
                }

                AuthenticationResult sqliteResult = await _enClient.LoginSQLiteClient(Username, Password);

                if (!sqliteResult.Success)
                {
                    SQLiteLoginResult = sqliteResult.Message;
#if DEBUG
                    RequestShowAlertBox?.Invoke(new AlertBoxArgs(sqliteResult.Message), null);
#endif
                    return;
                }

                // Now rehash the client pw-hash as is done in server, then compare if the resulting complete user objects are identical
                sqliteResult.User.PasswordHash = HashingHelper.PerformHash(sqliteResult.User.PasswordHash, HashingHelper.CURRENT_VERSION);

                if (apiResult != null && !apiResult.User.Equals(sqliteResult.User) && apiResult.User != null)
                {
                    SQLiteLoginResult = "Inconsistent user data";
#if DEBUG
                    RequestShowAlertBox?.Invoke(new AlertBoxArgs(SQLiteLoginResult), null);
#endif
                    return;
                }

                // Potentially need to re-resolve cryptoRoutines, to be tested!
                // If yes, then add parameter to "RenewData...Clients" method and pass it again here.
                //CryptoRoutines cr = (CryptoRoutines)App.AppContainer.Resolve(typeof(ICryptoRoutines), null, null);
                _dataAccess.RenewDataSourceClients(_enClient);

                WelcomeMsg = "Welcome " + _enClient.User.DisplayName + "!";

                NavigateToMainCommand.Execute();

                // Login Success => Reset Fields:
                Password = "";
                RequestParentClose(this, new EventArgs());
            }
            catch (Exception e)
            {
                ApiLoginResult = $"An unexpected error occurred.";
#if DEBUG
                RequestShowAlertBox?.Invoke(new AlertBoxArgs(e.ToString()), null);
#endif
                Log.Error(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void NavigateToSignUp()
        {
            _regionManager.RequestNavigate("ContentRegion", new Uri("SignUp", UriKind.Relative));
        }
    }
}
