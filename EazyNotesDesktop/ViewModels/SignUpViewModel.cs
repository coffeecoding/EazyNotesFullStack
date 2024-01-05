using EazyNotesDesktop.Library.Helpers;
using EazyNotes.Models.DTO;
using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using EazyNotesDesktop.Util;
using System.Net.Http;
using EazyNotes.Common;
using System.IO;
using EazyNotesDesktop.Library.DAO;

namespace EazyNotesDesktop.ViewModels
{
    public class SignUpViewModel : BindableBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private readonly IRegionManager _regionManager;

        public SignUpViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateBackCommand = new DelegateCommand(NavigateBack, () => true);
            RegisterCommand = new AsyncCommand(Register, (x) => CanRegister);
        }

        public string Title => "Register EazyNotes Account";

        public event EventHandler RequestShowAlertBox;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                SetProperty(ref _isBusy, value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private void NavigateBack()
        {
            _regionManager.RequestNavigate("ContentRegion", new Uri("Login", UriKind.Relative));
        }

        private DelegateCommand _navigateBackCommand;
        public DelegateCommand NavigateBackCommand
        {
            get { return _navigateBackCommand; }
            set { SetProperty(ref _navigateBackCommand, value); }
        }

        private AsyncCommand _registerCommand;
        public AsyncCommand RegisterCommand
        {
            get { return _registerCommand; }
            set { SetProperty(ref _registerCommand, value); }
        }

        #region bound Properties for fields along with tooltip and validation
        private string _signUpResultMessage;
        public string SignUpResultMessage
        {
            get { return _signUpResultMessage; }
            set { SetProperty(ref _signUpResultMessage, value); }
        }

        private bool _signUpSuccess;
        public bool SignUpSuccess
        {
            get { return _signUpSuccess; }
            set { SetProperty(ref _signUpSuccess, value); }
        }

        private string _fieldUsername;
        public string FieldUsername
        {
            get { return _fieldUsername; }
            set
            {
                if (String.IsNullOrWhiteSpace(@value))
                {
                    IsValidUsername = false;
                    ToolTipUsername = "Username required.";
                }
                else
                {
                    IsValidUsername = true;
                    ToolTipUsername = "OK.";
                }
                SetProperty(ref _fieldUsername, @value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _fieldEmail;
        public string FieldEmail
        {
            get { return _fieldEmail; }
            set
            {
                SignUpResultMessage = "";
                if (String.IsNullOrWhiteSpace(@FieldEmail))
                {
                    IsValidEmail = false;
                    ToolTipEmail = "Email address required.";
                }
                else if (!EmailUtil.IsValidEmail(@FieldEmail))
                {
                    IsValidEmail = false;
                    ToolTipEmail = "Invalid email address.";
                }
                else
                {
                    IsValidEmail = true;
                    ToolTipEmail = "";
                }
                SetProperty(ref _fieldEmail, @value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _fieldDisplayName;
        public string FieldDisplayName
        {
            get { return _fieldDisplayName; }
            set
            {
                if (String.IsNullOrWhiteSpace(@value))
                {
                    IsValidDisplayName = false;
                    ToolTipDisplayName = "DisplayName required.";
                }
                else
                {
                    IsValidDisplayName = true;
                    ToolTipDisplayName = "OK.";
                }
                SetProperty(ref _fieldDisplayName, @value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _fieldPassword;
        public string FieldPassword
        {
            get { return _fieldPassword; }
            set
            {
                if (String.IsNullOrWhiteSpace(@value))
                {
                    IsValidPassword = false;
                    ToolTipPassword = "Password required.";
                }
                else
                {
                    IsValidPassword = true;
                    string strength = PasswordAdvisor.CheckStrength(@value).ToString();
                    ToolTipPassword = $"Your Password is {strength}.";
                }
                if (@value != FieldConfirmPassword)
                {
                    IsValidConfirmPassword = false;
                    ToolTipConfirmPassword = "Confirmed Password must match Password.";
                }
                else
                {
                    IsValidConfirmPassword = true;
                    ToolTipConfirmPassword = "OK.";
                }
                SetProperty(ref _fieldPassword, @value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _fieldConfirmPassword;
        public string FieldConfirmPassword
        {
            get { return _fieldConfirmPassword; }
            set
            {
                if (String.IsNullOrWhiteSpace(@value))
                {
                    IsValidConfirmPassword = false;
                    ToolTipConfirmPassword = "Entered Password must be confirmed.";
                }
                else if (@value != FieldPassword)
                {
                    IsValidConfirmPassword = false;
                    ToolTipConfirmPassword = "Confirmed Password must match Password.";
                }
                else
                {
                    IsValidConfirmPassword = true;
                    ToolTipConfirmPassword = "OK.";
                }
                SetProperty(ref _fieldConfirmPassword, @value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _toolTipUsername;
        public string ToolTipUsername
        {
            get { return _toolTipUsername; }
            set { SetProperty(ref _toolTipUsername, value); }
        }

        private string _toolTipEmail;
        public string ToolTipEmail
        {
            get { return _toolTipEmail; }
            set { SetProperty(ref _toolTipEmail, value); }
        }

        private string _toolTipDisplayName;
        public string ToolTipDisplayName
        {
            get { return _toolTipDisplayName; }
            set { SetProperty(ref _toolTipDisplayName, value); }
        }

        private string _toolTipPassword;
        public string ToolTipPassword
        {
            get { return _toolTipPassword; }
            set { SetProperty(ref _toolTipPassword, value); }
        }

        private string _toolTipConfirmPassword;
        public string ToolTipConfirmPassword
        {
            get { return _toolTipConfirmPassword; }
            set { SetProperty(ref _toolTipConfirmPassword, value); }
        }

        private bool? _isValidUsername;
        public bool? IsValidUsername
        {
            get { return _isValidUsername; }
            set { SetProperty(ref _isValidUsername, value); }
        }

        private bool? _isValidEmail;
        public bool? IsValidEmail
        {
            get { return _isValidEmail; }
            set { SetProperty(ref _isValidEmail, value); }
        }

        private bool? _isValidDisplayName;
        public bool? IsValidDisplayName
        {
            get { return _isValidDisplayName; }
            set { SetProperty(ref _isValidDisplayName, value); }
        }

        private bool? _isValidPassword;
        public bool? IsValidPassword
        {
            get { return _isValidPassword; }
            set { SetProperty(ref _isValidPassword, value); }
        }

        private bool? _isValidConfirmPassword;
        public bool? IsValidConfirmPassword
        {
            get { return _isValidConfirmPassword; }
            set { SetProperty(ref _isValidConfirmPassword, value); }
        }
        #endregion

        public bool CanRegister
        {
            get
            {
                return (IsValidUsername == true && IsValidEmail == true && IsValidDisplayName == true && IsValidPassword == true && IsValidConfirmPassword == true);
            }
        }

        public async Task Register()
        {
            IsBusy = true;
            Log.Info("Begin register user");
            SignUpResultMessage = "";
            try
            {
                // first register new User to API
                UserDTO newUser = UserDTO.CreateUser(FieldUsername, FieldEmail, 
                    FieldDisplayName, FieldPassword);

                (HttpResponseMessage response, string msg) = await APIClient.RegisterUser(newUser);

                if (!response.IsSuccessStatusCode)
                {
                    IsBusy = false;
                    SignUpSuccess = false;
                    SignUpResultMessage = msg;
                    return;
                }

                // if successfully registered User to API, create local user
                string userdir = ENClient.GetUserPath(newUser.Username);

                if (Directory.Exists(userdir))
                    // this should really never be the case but if it is just freakin delete it
                    Directory.Delete(userdir);

                Directory.CreateDirectory(userdir);
                SQLiteClient.Init(newUser);
                SQLiteDataSource.InsertOrUpdateUser(newUser);

                SignUpSuccess = true;
                SignUpResultMessage = "Successfully signed up. You can now log in.";
            }
            // TODO: Catch API validation errors that are data related and display appropriately to user.
            catch (HttpRequestException e)
            {
                SignUpResultMessage = "Couldn't reach server.";
#if DEBUG
                RequestShowAlertBox?.Invoke(new AlertBoxArgs(e.ToString()), null);
#endif
                Log.Warn(e);
            }
            catch (Exception e)
            {
                SignUpSuccess = false;
                SignUpResultMessage = $"An unexpected error occurred.";
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
    }
}
