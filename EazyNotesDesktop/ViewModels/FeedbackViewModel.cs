using EazyNotes.Models.DTO;
using EazyNotesDesktop.Library.Helpers;
using EazyNotesDesktop.Util;
using MvvmHelpers.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace EazyNotesDesktop.ViewModels
{
    public class FeedbackViewModel : BindableBase
    {
        public FeedbackViewModel()
        {
            PostFeedbackCommand = new AsyncCommand(PostFeedback, (x) => true);
        }

        public event EventHandler RequestShowSpinner;
        public event EventHandler RequestCloseSpinner;
        public event EventHandler RequestShowAlertBox;

        public AsyncCommand PostFeedbackCommand { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _feedbackTitle;
        public string FeedbackTitle
        {
            get { return _feedbackTitle; }
            set { SetProperty(ref _feedbackTitle, value); }
        }

        private string _feedbackBody;
        public string FeedbackBody
        {
            get { return _feedbackBody; }
            set { SetProperty(ref _feedbackBody, value); }
        }

        private bool _isFeatureRequest;
        public bool IsFeatureRequest
        {
            get { return _isFeatureRequest; }
            set 
            { 
                SetProperty(ref _isFeatureRequest, value); 
                if (value)
                {
                    IsBugReport = false;
                    IsOtherCategory = false;
                }
            }
        }

        private bool _isBugReport;
        public bool IsBugReport
        {
            get { return _isBugReport; }
            set 
            { 
                SetProperty(ref _isBugReport, value);
                if (value)
                {
                    IsFeatureRequest = false;
                    IsOtherCategory = false;
                }
            }
        }

        private bool _isOtherCategory;
        public bool IsOtherCategory
        {
            get { return _isOtherCategory; }
            set 
            { 
                SetProperty(ref _isOtherCategory, value);
                if (value)
                {
                    IsBugReport = false;
                    IsFeatureRequest = false;
                }
            }
        }

        private string _postResult;
        public string PostResult
        {
            get { return _postResult; }
            set { SetProperty(ref _postResult, value); }
        }

        private async Task PostFeedback()
        {
            if (string.IsNullOrEmpty(FeedbackTitle))
            {
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("Title required", AlertImage.Information), EventArgs.Empty);
                return;
            }
            if (string.IsNullOrEmpty(FeedbackBody))
            {
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("Body required", AlertImage.Information), EventArgs.Empty);
                return;
            }
            if ((IsBugReport || IsFeatureRequest || IsOtherCategory) == false)
            {
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("Please select a feedback category", AlertImage.Information), EventArgs.Empty);
                return;
            }
            try
            {
                RequestShowSpinner?.Invoke("Posting feedback ...", EventArgs.Empty);
                IsBusy = true;

                Int16 category = 0;
                if (IsFeatureRequest) category = (Int16)FeedbackCategory.FeatureRequest;
                else if (IsBugReport) category = (Int16)FeedbackCategory.Bug;
                else if (IsOtherCategory) category = (Int16)FeedbackCategory.Other;

                string device = Environment.MachineName;
                string platform = Environment.OSVersion.VersionString;
                string appver = $"WPF beta";

                FeedbackDTO feedback = new FeedbackDTO(0, FeedbackTitle, FeedbackBody, category, appver, device, platform, DateTime.UtcNow);

                var result = await APIClient.PostFeedback(feedback);
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("Thanks for your feedback, it is well appreciated!", AlertImage.Information), EventArgs.Empty);
                FeedbackTitle = FeedbackBody = "";
            } catch (Exception e)
            {
                PostResult = $"{e.GetType()} {e.Message}";
            }
            finally
            {
                IsBusy = false;
                RequestCloseSpinner?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
