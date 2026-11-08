using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly MobileApiService _api = new MobileApiService();
        private List<FacultyOption> _faculties = new List<FacultyOption>();

        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProfileDataAsync();
        }

        private async Task LoadProfileDataAsync()
        {
            _faculties = _api.GetPredefinedFaculties();
            FacultyPicker.ItemsSource = _faculties.Select(f => f.FacultyCode + " - " + f.FacultyName).ToList();

            var profile = StudentSession.Current.Profile;

            FullNameHeaderLabel.Text = profile.FullName;
            MatricHeaderLabel.Text = "Matric No: " + profile.MatricNo;
            FacultyBadgeLabel.Text = (profile.FacultyCode ?? "FSKTM") + " • Student Athlete";

            MatricValueLabel.Text = profile.MatricNo;
            FullNameEntry.Text = profile.FullName;
            EmailEntry.Text = profile.Email;

            // Pre-select faculty
            int index = _faculties.FindIndex(f => f.FacultyId == (profile.FacultyId ?? 1));
            if (index >= 0)
            {
                FacultyPicker.SelectedIndex = index;
            }
        }

        // UPDATE: Edit Profile (Mobile CRUD #1)
        private async void OnSaveProfileClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text) || string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Validation Error", "Full Name and Email cannot be empty.", "OK");
                return;
            }

            int selectedIndex = FacultyPicker.SelectedIndex;
            int? facultyId = selectedIndex >= 0 ? _faculties[selectedIndex].FacultyId : null;
            string? facultyCode = selectedIndex >= 0 ? _faculties[selectedIndex].FacultyCode : null;

            int userId = StudentSession.Current.Profile.UserId;

            bool success = await _api.UpdateProfileAsync(userId, FullNameEntry.Text.Trim(), EmailEntry.Text.Trim(), facultyId, facultyCode);
            if (success)
            {
                await LoadProfileDataAsync();
                await DisplayAlert("Profile Updated", "Your student profile and faculty affiliation have been updated successfully.", "OK");
            }
        }

        // AUTHENTICATION: Login / Switch Student
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string identifier = await DisplayPromptAsync("Student Login", 
                "Enter your Matric No or UTHM Siswa Email:", 
                initialValue: "AI220015", 
                maxLength: 60);

            if (string.IsNullOrWhiteSpace(identifier)) return;

            string password = await DisplayPromptAsync("Student Login", 
                $"Enter password for {identifier.Trim()}:", 
                initialValue: "student123", 
                maxLength: 50, 
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(password)) return;

            var (success, message, profile) = await _api.LoginAsync(identifier.Trim(), password.Trim());
            if (success && profile != null)
            {
                await LoadProfileDataAsync();
                await DisplayAlert("Login Successful", $"Welcome, {profile.FullName}!\nLogged in as {profile.Role} ({profile.FacultyCode}).", "Continue");
            }
            else
            {
                await DisplayAlert("Login Failed", message, "Retry");
            }
        }

        private async void OnQuickLoginFarhanClicked(object sender, EventArgs e)
        {
            var (success, message, profile) = await _api.LoginAsync("AI220015", "student123");
            if (success)
            {
                await LoadProfileDataAsync();
                await DisplayAlert("Account Switched", "Active session switched to Muhammad Farhan (AI220015 - FSKTM).", "OK");
            }
        }

        private async void OnQuickLoginAisyahClicked(object sender, EventArgs e)
        {
            var (success, message, profile) = await _api.LoginAsync("CI220098", "student123");
            if (success)
            {
                await LoadProfileDataAsync();
                await DisplayAlert("Account Switched", "Active session switched to Nur Aisyah (CI220098 - FKEE).", "OK");
            }
        }

        // CREATE: Register New Student Profile (Mobile CRUD #1)
        private async void OnRegisterNewProfileClicked(object sender, EventArgs e)
        {
            string matric = await DisplayPromptAsync("New Registration", "Enter Student Matric Number (e.g. AI220088):", maxLength: 20);
            if (string.IsNullOrWhiteSpace(matric)) return;

            string name = await DisplayPromptAsync("New Registration", "Enter Student Full Name:", maxLength: 50);
            if (string.IsNullOrWhiteSpace(name)) return;

            string email = await DisplayPromptAsync("New Registration", "Enter Student Email:", initialValue: $"{matric.ToLower()}@siswa.uthm.edu.my", maxLength: 60);
            if (string.IsNullOrWhiteSpace(email)) return;

            await _api.RegisterProfileAsync(matric.Trim(), name.Trim(), email.Trim(), 1, "student123");
            await LoadProfileDataAsync();
            await DisplayAlert("Welcome", $"New student profile created for {name} ({matric}).", "OK");
        }

        // DELETE: Deactivate/Delete Account (Mobile CRUD #1)
        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            var profile = StudentSession.Current.Profile;
            bool confirm = await DisplayAlert("Deactivate Account", 
                $"Are you sure you want to permanently delete profile for {profile.FullName} ({profile.MatricNo})?", 
                "Permanently Delete", "Cancel");

            if (confirm)
            {
                await _api.DeleteProfileAsync(profile.UserId);
                await LoadProfileDataAsync();
                await DisplayAlert("Account Deleted", "Student profile has been deactivated and removed from the system.", "OK");
            }
        }
    }
}

