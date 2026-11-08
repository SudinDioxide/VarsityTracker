using System;
using VarsityTracker.Mobile.Models;

namespace VarsityTracker.Mobile.Services
{
    public class StudentSession
    {
        private static StudentSession? _instance;
        public static StudentSession Current => _instance ??= new StudentSession();

        public StudentProfile Profile { get; set; } = new StudentProfile();

        public bool IsLoggedIn => Profile.UserId > 0;

        public event Action? OnProfileUpdated;

        public void UpdateProfile(string fullName, string email, int? facultyId, string? facultyCode)
        {
            Profile.FullName = fullName;
            Profile.Email = email;
            if (facultyId.HasValue) Profile.FacultyId = facultyId.Value;
            if (!string.IsNullOrEmpty(facultyCode)) Profile.FacultyCode = facultyCode;

            OnProfileUpdated?.Invoke();
        }

        public void Logout()
        {
            Profile = new StudentProfile { UserId = 0, MatricNo = "", FullName = "Guest", Email = "", Role = "" };
            OnProfileUpdated?.Invoke();
        }
    }
}

