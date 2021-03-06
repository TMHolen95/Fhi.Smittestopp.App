using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Text;
using NDB.Covid19.OAuth2;
using NDB.Covid19.ViewModels;
using NDB.Covid19.Droid.Utils;
using NDB.Covid19.Enums;
using NDB.Covid19.Utils;
using Xamarin.Auth;
using static NDB.Covid19.Droid.Utils.StressUtils;

namespace NDB.Covid19.Droid.Views.AuthenticationFlow
{
    [Activity(Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
    public class InformationAndConsentActivity : AppCompatActivity
    {
        ViewGroup _closeButton;
        TextView _header;
        TextView _subtitleText;
        TextView _bodyOneText;
        TextView _bodyTwoText;
        TextView _contentTwoText;
        TextView _contentText;
        Button _idPortenButton;
        InformationAndConsentViewModel _viewModel;

        ProgressBar _progressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.Title = InformationAndConsentViewModel.INFORMATION_CONSENT_HEADER_TEXT;
            SetContentView(Resource.Layout.information_and_consent);
            CustomTabsConfiguration.CustomTabsClosingMessage = null;
            _viewModel = new InformationAndConsentViewModel(OnAuthSuccess, OnAuthError);
            _viewModel.Init();
            InitLayout();
        }

        private void OnAuthError(object sender, AuthErrorType e)
        {
            GoToErrorPage(e);
        }

        private void OnAuthSuccess(object sender, EventArgs e)
        {
            LogUtils.LogMessage(Enums.LogSeverity.INFO, $"Successfully authenticated and verified user. Navigation to {nameof(QuestionnairePageActivity)}");
            GoToQuestionnairePage();
        }

        void InitLayout()
        {
            //Buttons
            _closeButton = FindViewById<ViewGroup>(Resource.Id.close_cross_btn);
            _idPortenButton = FindViewById<Button>(Resource.Id.information_consent_idporten_button);

            //TextViews
            _header = FindViewById<TextView>(Resource.Id.information_consent_header_textView);
            _contentText = FindViewById<TextView>(Resource.Id.information_consent_content_textView);
            _subtitleText = FindViewById<TextView>(Resource.Id.information_consent_subtitle_textView);
            _bodyOneText = FindViewById<TextView>(Resource.Id.information_consent_body_one_textView);
            _bodyTwoText = FindViewById<TextView>(Resource.Id.information_consent_body_two_textView);
            _contentTwoText = FindViewById<TextView>(Resource.Id.information_consent_content_two_textView);

            //Text initialization
            _idPortenButton.Text = InformationAndConsentViewModel.INFORMATION_CONSENT_ID_PORTEN_BUTTON_TEXT;
            _header.Text = InformationAndConsentViewModel.INFORMATION_CONSENT_HEADER_TEXT;
            _contentText.TextFormatted = HtmlCompat.FromHtml($"{InformationAndConsentViewModel.INFOCONSENT_DESCRIPTION}", HtmlCompat.FromHtmlModeLegacy);
            _subtitleText.Text = InformationAndConsentViewModel.INFOCONSENT_TITLE;
            _bodyOneText.Text = InformationAndConsentViewModel.INFOCONSENT_BODY_ONE;
            _bodyTwoText.Text = InformationAndConsentViewModel.INFOCONSENT_BODY_TWO;
            _contentTwoText.Text = InformationAndConsentViewModel.INFOCONSENT_DESCRIPTION_ONE;

            ////Accessibility
            _closeButton.ContentDescription = InformationAndConsentViewModel.CLOSE_BUTTON_ACCESSIBILITY_LABEL;

            //Button click events
            _closeButton.Click += new SingleClick((sender, e) => Finish(), 500).Run;
            _idPortenButton.Click += new SingleClick(LogInWithIDPortenButton_Click, 500).Run;

            //Progress bar
            _progressBar = FindViewById<ProgressBar>(Resource.Id.information_consent_progress_bar);
        }

        private async void LogInWithIDPortenButton_Click(object sender, EventArgs e)
        {
            LogUtils.LogMessage(Enums.LogSeverity.INFO, "Startet login with ID porten");
            Intent browserIntent = AuthenticationState.Authenticator.GetUI(this);
            StartActivity(browserIntent);
        }

        //After calling this method you cannot return by going "Back".
        //OnCreate has to be called again if returning to this page.
        void GoToErrorPage(AuthErrorType error)
        {
            _viewModel.Cleanup();
            RunOnUiThread(() =>
            {
                switch (error)
                {
                    case AuthErrorType.MaxTriesExceeded:
                        AuthErrorUtils.GoToManyTriesError(this, LogSeverity.WARNING,null, "Max number of tries was exceeded");
                        break;
                    case AuthErrorType.NotInfected:
                        AuthErrorUtils.GoToNotInfectedError(this, LogSeverity.WARNING, null, "User is not infected");
                        break;
                    case AuthErrorType.Unknown:
                        AuthErrorUtils.GoToTechnicalError(this, Enums.LogSeverity.WARNING, null, "User sees Technical error page after ID Porten login: Unknown auth error or user press backbtn");
                        break;
                }

            });
        }

        //After calling this method you cannot return by going "Back".
        //OnCreate has to be called again if returning to this page.
        void GoToQuestionnairePage()
        {
            _viewModel.Cleanup();
            RunOnUiThread(() =>
            {
                Intent intent = new Intent(this, typeof(QuestionnairePageActivity));
                StartActivity(intent);
            });
           
        }

        private void ShowSpinner(bool show)
        {
            if (show)
            {
                _idPortenButton.Enabled = false;
                _idPortenButton.Visibility = ViewStates.Invisible;
                _progressBar.Visibility = ViewStates.Visible;
            }
            else
            {
                _idPortenButton.Enabled = true;
                _idPortenButton.Visibility = ViewStates.Visible;
                _progressBar.Visibility = ViewStates.Gone;
            }
        }
    }
}