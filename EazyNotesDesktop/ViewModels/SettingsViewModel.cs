using EazyNotes.Models.POCO;
using EazyNotesDesktop.Util;
using MvvmHelpers.Commands;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EazyNotesDesktop.Library.DAO;
using System.Collections.Generic;
using System.Text;
using EazyNotesDesktop.Library.Common;
using EazyNotes.Common;
using EazyNotesDesktop.Library.Transport;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EazyNotesDesktop.Library.Helpers;

namespace EazyNotesDesktop.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private readonly DataAccess _dataAccess;
        private IUserData _userData;
        private readonly MainViewModel _mainVM;

        // TODO: Potentially add fallback paths in case of access issues (VERY unlikely though)
        private readonly string PREFERENCES_PATH;
        private readonly string PREFERENCES_FILENAME;

        public SettingsViewModel(DataAccess dataAccess, IUserData userSecrets, MainViewModel mainVM)
        {
            _dataAccess = dataAccess;
            _userData = userSecrets;
            _mainVM = mainVM;

            FeedbackViewModel = new FeedbackViewModel();
            FeedbackViewModel.RequestShowSpinner += (s, e) => RequestShowSpinner?.Invoke(s, e);
            FeedbackViewModel.RequestCloseSpinner += (s, e) => RequestCloseSpinner?.Invoke(s, e);
            FeedbackViewModel.RequestShowAlertBox += (s, e) => RequestShowAlertBox?.Invoke(s, e);

            PREFERENCES_PATH = ENClient.GetUserPath(_userData.User.Username);
            PREFERENCES_FILENAME = $@"prefs_{_userData.User.Username}.json";

            LoadPreferencesFromFile();

            Username = _userData.User.Username;
            DisplayName = _userData.User.DisplayName;
            Email = _userData.User.Email;
            IsReadOnlyDisplayName = IsReadOnlyEmail = IsReadOnlyUsername = true;

            LogoutCommand = new DelegateCommand(Logout, () => true);
            SaveUsernameCommand = new AsyncCommand(SaveUsername, (x) => true);
            SaveDisplayNameCommand = new AsyncCommand(SaveDisplayName, (x) => true);
            SaveEmailCommand = new AsyncCommand(SaveEmail, (x) => true);
            EditUsernameCommand = new DelegateCommand(EditUsername, () => true);
            EditDisplayNameCommand = new DelegateCommand(EditDisplayName, () => true);
            EditEmailCommand = new DelegateCommand(EditEmail, () => true);
            CancelEditUsernameCommand = new DelegateCommand(CancelEditUsername, () => true);
            CancelEditDisplayNameCommand = new DelegateCommand(CancelEditDisplayName, () => true);
            CancelEditEmailCommand = new DelegateCommand(CancelEditEmail, () => true);

            ChangePasswordCommand = new DelegateCommand(ChangePassword, () => true);

            SelectDarkThemeCommand = new DelegateCommand(() => IsDarkThemeSelected = true, () => true);
            SelectLightThemeCommand = new DelegateCommand(() => IsLightThemeSelected = true, () => true);

            SelectImportIntoCurrentCmd = new DelegateCommand(() => SelectedImportIntoCurrentTopic = true, () => true);
            SelectCreateNewTopicCmd = new DelegateCommand(() => SelectedUseExistingOrCreateNewTopic = true, () => true);

            ExportNotesCommand = new DelegateCommand(ExportNotes, () => true);

            IsNotBusy = true;
        }

        public TrashBinViewModel TrashBinViewModel => _mainVM.TrashBinViewModel;
        private FeedbackViewModel _feedbackViewModel;
        public FeedbackViewModel FeedbackViewModel
        {
            get { return _feedbackViewModel; }
            set { SetProperty(ref _feedbackViewModel, value); }
        }

        public event EventHandler RequestShowAlertBox;
        public event EventHandler RequestShowSpinner;
        public event EventHandler RequestCloseSpinner;

        private Preferences Preferences;

        public DelegateCommand LogoutCommand { get; set; }
        public AsyncCommand SaveUsernameCommand { get; set; }
        public AsyncCommand SaveDisplayNameCommand { get; set; }
        public AsyncCommand SaveEmailCommand { get; set; }
        public DelegateCommand EditUsernameCommand { get; set; }
        public DelegateCommand EditDisplayNameCommand { get; set; }
        public DelegateCommand EditEmailCommand { get; set; }
        public DelegateCommand CancelEditUsernameCommand { get; set; }
        public DelegateCommand CancelEditDisplayNameCommand { get; set; }
        public DelegateCommand CancelEditEmailCommand { get; set; }
        public DelegateCommand ChangePasswordCommand { get; set; }

        public DelegateCommand SelectDarkThemeCommand { get; set; }
        public DelegateCommand SelectLightThemeCommand { get; set; }

        public DelegateCommand SelectImportIntoCurrentCmd { get; set; }
        public DelegateCommand SelectCreateNewTopicCmd { get; set; }

        public DelegateCommand ExportNotesCommand { get; set; }

        private bool _isNotBusy;
        public bool IsNotBusy
        {
            get { return _isNotBusy; }
            set { SetProperty(ref _isNotBusy, value); }
        }

        public void RaiseSettingsChanged()
        {
            RaisePropertyChanged("SelectedNoteSortKey");
        }

        #region Data related preferences
        private NoteSortKey _selectedNoteSortKey;
        public NoteSortKey SelectedNoteSortKey
        {
            get { return _selectedNoteSortKey == null ? NoteSortKeys[0] : _selectedNoteSortKey; }
            set 
            { 
                SetProperty(ref _selectedNoteSortKey, value);
                Preferences.NoteSortKey = value;
                _mainVM.TopicViewModels.ForEach(t => t.SortNotes(value));
            }
        }

        public List<NoteSortKey> NoteSortKeys => NoteSortKey.NoteSortKeys;
        #endregion

        #region Appearance Page
        private bool _isLightThemeSelected;
        public bool IsLightThemeSelected
        {
            get { return _isLightThemeSelected; }
            set
            {
                SetProperty(ref _isLightThemeSelected, value);
                if (value == true)
                {
                    IsDarkThemeSelected = false;
                    (App.Current as App).ApplyTheme(Theme.Light);
                }
                Preferences.Theme = Theme.Light;
                ApplyTheme();
            }
        }

        private bool _isDarkThemeSelected;
        public bool IsDarkThemeSelected
        {
            get { return _isDarkThemeSelected; }
            set 
            {
                SetProperty(ref _isDarkThemeSelected, value);
                if (value == true)
                {
                    IsLightThemeSelected = false;
                }
                Preferences.Theme = Theme.Dark;
                ApplyTheme();
            }
        }

        private bool _showTopicTitles;
        public bool ShowTopicTitles
        {
            get { return _showTopicTitles; }
            set { SetProperty(ref _showTopicTitles, value);
                Preferences.ShowTopicTitles = value; }
        }

        public void ApplyTheme()
        {
            try
            {
                (App.Current as App).ApplyTheme(Preferences.Theme);
            } catch (Exception e)
            {
                Log.Info($"Applying changed theme: DarkSelected={IsDarkThemeSelected}, WhiteSelected={IsLightThemeSelected}");
                Log.Error(e);
            }
        }
        #endregion

        #region Profile Page
        private bool _isReadOnlyUsername;
        public bool IsReadOnlyUsername
        {
            get { return _isReadOnlyUsername; }
            set { SetProperty(ref _isReadOnlyUsername, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private bool _isReadOnlyDisplayName;
        public bool IsReadOnlyDisplayName
        {
            get { return _isReadOnlyDisplayName; }
            set { SetProperty(ref _isReadOnlyDisplayName, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private bool _isReadOnlyEmail;
        public bool IsReadOnlyEmail
        {
            get { return _isReadOnlyEmail; }
            set { SetProperty(ref _isReadOnlyEmail, value); }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        private void ChangePassword()
        {
            // TODO: Implement
        }

        private async Task<bool> UpdateUser()
        {
            Log.Info("Updating user in settings");
            try
            {
                UserProfile changedUser = new UserProfile(_userData.User.Id, Username, DisplayName,
                    Email, _userData.User.EmailVerified);

                var result = await _dataAccess.UpdateUser(_userData.User.CloneWith(changedUser));

                _userData.User.Username = Username;
                _userData.User.Email = Email;
                _userData.User.DisplayName = DisplayName;

                // TODO: Verify that this is the correct way to set the updated PW and Key data
                // Maybe move password update into a separate function even
                //_userData.User.PasswordHash = changedUser.PasswordHash;
                //_userData.User.RSAPublicKey = changedUser.RSAPublicKey;
                //_userData.User.RSAPrivateKeyCrypt = changedUser.RSAPrivateKeyCrypt;

                return true;
            }
            // TODO: catch more granular exceptions
            catch (Exception e)
            {
                // TODO Show alert with appropriate message
                Log.Error(e);
                return false;
            }
        }

        private void EditUsername()
        {
            IsReadOnlyUsername = false;
        }

        private void EditDisplayName()
        {
            IsReadOnlyDisplayName = false;
        }

        private void EditEmail()
        {
            IsReadOnlyEmail = false;
        }

        private async Task SaveUsername()
        {
            // Validate first
            if (await UpdateUser() == true)
                IsReadOnlyUsername = true;
        }

        private async Task SaveDisplayName()
        {
            // Validate first
            if (await UpdateUser() == true) 
                IsReadOnlyDisplayName = true;
        }

        private async Task SaveEmail()
        {
            // Validate first
            if (await UpdateUser() == true)
                IsReadOnlyEmail = true;
        }

        private void CancelEditUsername()
        {
            IsReadOnlyUsername = true;
            Username = _userData.User.Username;
        }

        private void CancelEditDisplayName()
        {
            IsReadOnlyDisplayName = true;
            DisplayName = _userData.User.DisplayName;
        }

        private void CancelEditEmail()
        {
            IsReadOnlyEmail = true;
            Email = _userData.User.Email;
        }

        public void Logout()
        {
            _dataAccess.Reset();
            _userData = null;
            SaveSettings();
        }
        #endregion

        #region Import Notes Page
        public async Task ImportNotes(List<string> fileOrFolderPaths)
        {
            Log.Info("Begin importing notes");
            IsNotBusy = false;
            RequestShowSpinner?.Invoke("Importing Notes ...", null);
            if (SelectedImportIntoCurrentTopic)
            {
                if (_mainVM.SelectedTopicVM == null)
                {
                    RequestShowAlertBox(new AlertBoxArgs("Please select a topic first", AlertImage.Information), null);
                    return;
                }
            }
            try
            {
                FileSystemTransporter fst = new FileSystemTransporter(_userData);
                List<Topic> targetTopics = new List<Topic>();
                TopicImportBehaviour tib = TopicImportBehaviour.IfNameExistsUpdateTopic;
                if (SelectedImportIntoCurrentTopic)
                    targetTopics.Add(_mainVM.SelectedTopicVM.Topic);
                else
                {
                    if (SelectedUseExistingOrCreateNewTopic)
                        tib = TopicImportBehaviour.IfNameExistsUpdateTopic;
                    else tib = TopicImportBehaviour.AlwaysCreateNewTopic;
                    targetTopics = _dataAccess.Topics.Values.Where(t => t.DateDeleted == null).ToList();
                }
                ImportResult ir = await Task.Run(() => fst.Import(fileOrFolderPaths, targetTopics, tib));

                ir.ImportedTopics = await _dataAccess.InsertTopics(ir.ImportedTopics);

                // update imported notes with the respective topic ids 
                ir.ImportedNotesByTopicTitle.Keys.ForEach(k =>
                {
                    Guid topicId = ir.ImportedTopics.Single(t => t.Title == k).Id;
                    ir.ImportedNotesByTopicTitle[k].ForEach(n => n.TopicId = topicId);
                });

                // add topic vms
                _mainVM.TopicViewModels = new BindingList<TopicViewModel>(
                    _mainVM.TopicViewModels.AddRange(ir.ImportedTopics
                    .OrderBy(t => t.Position).ToList()
                    .ConvertAll(t => new TopicViewModel(_dataAccess, _mainVM, t))));

                // if user selected to commit notes immediately, do that
                if (SaveImportedNotesInstantly) {
                    // flatten imported notes lists while keeping track of note count per imported topic
                    List<AbstractNote> importedNotes = new List<AbstractNote>();
                    Dictionary<string, int> noteCountByTopicTitle = new Dictionary<string, int>();
                    foreach (string topicTitle in ir.ImportedNotesByTopicTitle.Keys)
                    {
                        importedNotes.AddRange(ir.ImportedNotesByTopicTitle[topicTitle]);
                        noteCountByTopicTitle.Add(topicTitle, ir.ImportedNotesByTopicTitle[topicTitle].Count);
                    }
                    importedNotes = await _dataAccess.InsertNotes(importedNotes);
                    // update the imported note ids with the persisted note ids
                    // this is slightly complex as the notes were flattened to a single list to insert them;
                    // now we need to figure out again which note belongs in which topic but we do that using
                    // the note-count per topic which we saved in the dict "noteCountByTopicTitle"
                    int i = 0;
                    foreach (string topicTitle in noteCountByTopicTitle.Keys)
                    {
                        for (int j = 0; j < noteCountByTopicTitle[topicTitle]; j++)
                            ir.ImportedNotesByTopicTitle[topicTitle][j].Id = importedNotes[j + i].Id;
                        i += noteCountByTopicTitle[topicTitle];
                    }
                }
                
                // in any case, add the new notes as noteviewmodels to the topicviewmodels by topicId, or else by topicName  
                ir.ImportedNotesByTopicTitle.Keys.ForEach(topicTitle =>
                {
                    TopicViewModel tvm = _mainVM.TopicViewModels.Single(tvm => tvm.Title == topicTitle);
                    tvm.AddNoteViewModels(ir.ImportedNotesByTopicTitle[topicTitle]
                        .ConvertAll(n => BaseNoteViewModel.FromNote(n, _dataAccess, _mainVM, tvm)));
                });

            } // TODO: Show appropriate Message to User
            catch (Exception e)
            {
                Log.Error(e);
                RequestShowAlertBox?.Invoke(new AlertBoxArgs(e, AlertImage.Warning), null);
            }
            finally
            {
                IsNotBusy = true;
                RequestCloseSpinner?.Invoke(this, null);
            }
        }

        private bool _saveImportedNotesInstantly;
        public bool SaveImportedNotesInstantly
        {
            get { return _saveImportedNotesInstantly; }
            set 
            { 
                SetProperty(ref _saveImportedNotesInstantly, value); 
                Preferences.SaveImportedNotesInstantly = value; 
            }
        }

        private bool _selectedImportIntoCurrentTopic;
        public bool SelectedImportIntoCurrentTopic
        {
            get { return _selectedImportIntoCurrentTopic; }
            set 
            { 
                SetProperty(ref _selectedImportIntoCurrentTopic, value);
                if (value == true)
                    SelectedUseExistingOrCreateNewTopic = false;
                Preferences.SelectedImportIntoCurrentTopic = value;
            }
        }

        private bool _selectedUseExistingOrCreateNewTopic;
        public bool SelectedUseExistingOrCreateNewTopic
        {
            get { return _selectedUseExistingOrCreateNewTopic; }
            set 
            { 
                SetProperty(ref _selectedUseExistingOrCreateNewTopic, value);
                if (value == true)
                    SelectedImportIntoCurrentTopic = false;
                Preferences.SelectedUseExistingOrCreateNewTopic = value;
            }
        }

        private string _newTopicTitle;
        public string NewTopicTitle
        {
            get { return _newTopicTitle; }
            set { SetProperty(ref _newTopicTitle, value); }
        }
        #endregion

        #region Export Notes Page
        public void SetExportPath(string path)
        {
            ExportPath = path;
        }

        private void ExportNotes()
        {
            if (_mainVM.TopicViewModels.Count == 0)
            {
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("There are no notes to export.", AlertImage.Information), null);
                return;
            }
            Log.Info("Begin exporting notes");
            IsNotBusy = false;
            RequestShowSpinner?.Invoke("Exporting Notes ...", null);
            try
            {
                try
                {
                    FileSystemTransporter fst = new FileSystemTransporter(_userData, ExportPath);
                    _mainVM.TopicViewModels.ForEach(tvm => fst.ExportTopic(tvm.Topic, tvm.NoteViewModels.ToList().ConvertAll(nvm => nvm.Note)));
                    RequestShowAlertBox?.Invoke(new AlertBoxArgs($"Exported files to {Path.GetFullPath(ExportPath)}",
                        AlertImage.Information), null);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e is PathTooLongException)
                        msg = $"The given path is too long";
                    else if (e is UnauthorizedAccessException)
                        msg = $"Access denied to path \"{ExportPath}\"";
                    else if (e is NotFullyQualifiedPathException)
                        msg = $"Incomplete path: \"{ExportPath}\"";
                    else if (e is InvalidPathException)
                        msg = $"Invalid path: \"{ExportPath}\"";
                    else msg = $"Error: {e.GetType()}: {msg}";
                    RequestShowAlertBox?.Invoke(new AlertBoxArgs(msg, AlertImage.Warning), null);
                }
            } // TODO: 1 - catch more granular exceptions, 2 - proper user feedback
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                IsNotBusy = true;
                RequestCloseSpinner?.Invoke(this, null);
            }
        }

        private string _exportPath;
        public string ExportPath
        {
            get { return _exportPath; }
            set { SetProperty(ref _exportPath, value); }
        }
        #endregion

        #region Load and Save Settings
        public void LoadPreferencesFromFile()
        {
            Log.Info("Begin loading user preferences");
            try
            {
                string serialized = "";
                string path = Path.Combine(PREFERENCES_PATH, PREFERENCES_FILENAME);
                using var sr = new StreamReader(path);
                serialized = sr.ReadToEnd();
                Preferences = (Preferences) JsonConvert.DeserializeObject(serialized, typeof(Preferences));

                switch (Preferences.Theme)
                {
                    case Theme.Dark: IsDarkThemeSelected = true; break;
                    case Theme.Light: IsLightThemeSelected = true; break;
                }
                SaveImportedNotesInstantly = Preferences.SaveImportedNotesInstantly;
                SelectedImportIntoCurrentTopic = Preferences.SelectedImportIntoCurrentTopic;
                SelectedUseExistingOrCreateNewTopic = Preferences.SelectedUseExistingOrCreateNewTopic;
                ShowTopicTitles = Preferences.ShowTopicTitles;
                SelectedNoteSortKey = Preferences.NoteSortKey;
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Info("Setting default preferences");
                Preferences = Preferences.GetDefaultSettings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            Log.Info("Begin writing preferences to file");
            string serialized = JsonConvert.SerializeObject(Preferences, Formatting.Indented);
            try
            {
                Directory.CreateDirectory(PREFERENCES_PATH);
                string path = Path.Combine(PREFERENCES_PATH, PREFERENCES_FILENAME);
                using var sw = new StreamWriter(path);
                sw.Write(serialized);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        #endregion
    }
}
