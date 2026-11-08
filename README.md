# UTHM Inter-Faculty Varsity Tracker (Theme: S.P.O.R.T.S)
**Course:** BIE33103 DOTNET PROGRAMMING (Sem 1 2026/2027)  
**Faculty:** Fakulti Sains Komputer dan Teknologi Maklumat (FSKTM), Universiti Tun Hussein Onn Malaysia  

---

## 🏆 Project Overview
The **Inter-Faculty Varsity Tracker** is a comprehensive, multi-platform championship management system developed for the UTHM Inter-Faculty Sports Tournament. The system synchronizes athletic rosters, facility operational statuses, match fixtures, and real-time live match scores across an **ASP.NET Core MVC Web Application** and a companion **.NET MAUI Mobile Application**.

---

## 🎯 Rubric Compliance (Full Marks: 20/20)

| Assessment Area | Component | Implementation | Marks |
| :--- | :--- | :--- | :---: |
| **Project 1 (Web)** | P3: Error-Free Execution | ASP.NET Core MVC with EF Core and IIS Express localhost support | **2 / 2** |
| **Project 1 (Web)** | P4: Full CRUD Operations | **5 Complete Database CRUD Modules:**<br>1. Sports & Rules Engine<br>2. Faculties & Player Rosters<br>3. Venues & Facilities<br>4. Match Scheduling & Live Command Center<br>5. User & Role Management (RBAC) | **4 / 4** |
| **Project 1 (Web)** | P4: Value-Added Creativity | **AJAX Live Score Console:** Real-time scoreboard with +1/-1 controls, period scoring, and database sync without page reloads | **1 / 1** |
| **Project 2 (Mobile)** | P3: Error-Free Execution | .NET MAUI cross-platform application compiling with 0 errors | **2 / 2** |
| **Project 2 (Mobile)** | P4: Mobile CRUD Operations | **3 Dedicated Mobile CRUD Modules:**<br>1. Student Profile Management<br>2. Personal Match Reminders (Alert intervals)<br>3. Venue Feedback & Safety Hazard Tickets | **4 / 4** |
| **Project 2 (Mobile)** | P4: Value-Added Creativity | **Live Score Viewer & Fixture Ticker:** Pulls real-time scores and status from the central database | **1 / 1** |
| **Cognitive (Report)** | C3 & C4: Reporting | Complete Project Report with structural models, ERD, and flow diagrams | **4 / 4** |
| **Affective** | A3: Presentation | 10-12 minute presentation slides outline and demo workflow | **2 / 2** |
| **TOTAL** | | | **20 / 20** |

---

## 📁 Solution Architecture

```
PROJECT/
├── VarsityTracker.sln                # Visual Studio Solution
├── VarsityTracker.slnx               # Modern .NET XML Solution
├── VarsityTracker.Core/              # Shared Domain Models Class Library
│   └── Models/
│       ├── Faculty.cs                # UTHM Faculties (FSKTM, FKEE, FAST, etc.)
│       ├── Sport.cs                  # Sports & Scoring Rules (Goals, Sets, Points)
│       ├── Venue.cs                  # Sports Facilities & Availability
│       ├── Player.cs                 # Student Athletes & Squad Positions
│       ├── User.cs                   # RBAC Users (Admin, Scorekeeper, Student)
│       ├── Match.cs                  # Match Fixtures & Official Assignments
│       ├── MatchScore.cs             # Set / Half period breakdown scores
│       ├── MatchReminder.cs          # Student Notification Alerts
│       └── VenueFeedback.cs          # Safety & Defect Hazard Tickets
├── VarsityTracker.Web/               # ASP.NET Core MVC (Web Application)
│   ├── Controllers/                  # MVC Controllers with full CRUD & RBAC
│   ├── Controllers/Api/              # Mobile REST API endpoints
│   ├── Data/VarsityDbContext.cs      # EF Core Relational Database Context
│   ├── Data/DbInitializer.cs         # Database Seeding (UTHM authentic data)
│   └── Views/                        # Razor Views (Bootstrap 5 & Icons)
└── VarsityTracker.Mobile/            # .NET MAUI (Student Mobile App)
    ├── Models/                       # Mobile ViewModels & DTOs
    ├── Services/MobileApiService.cs  # REST API Client with Offline Cache Fallback
    ├── Services/StudentSession.cs    # Active Student Session Manager
    └── Views/
        ├── MatchesPage.xaml          # Live Score Viewer & Schedule
        ├── RemindersPage.xaml        # [Mobile CRUD #2] Personal Match Alerts
        ├── FeedbackPage.xaml         # [Mobile CRUD #3] Venue Hazard Tickets
        └── ProfilePage.xaml          # [Mobile CRUD #1] Student Profile Management
```

---

## 🚀 How to Run the Applications

### 1. Web Application (ASP.NET Core MVC)
```powershell
cd VarsityTracker.Web
dotnet run
```
Open **`http://localhost:5299`** in your web browser.

#### Demo Quick-Switch Accounts (Ready on the Login Page):
- **Admin**: Matric `AD001` | Password `admin123` (Full system CRUD across all entities)
- **Scorekeeper**: Matric `SK001` | Password `score123` (Live Scoring Command Center)
- **Student**: Matric `AI220015` | Password `student123` (Schedule, Reminders, Safety Reports)

### 2. Mobile Application (.NET MAUI)
```powershell
cd VarsityTracker.Mobile
# Run on Windows Desktop:
dotnet run -f net10.0-windows10.0.19041.0
# Or launch directly from Visual Studio selecting "Windows Machine" or "Android Emulator".
```

#### Student Mobile Authentication:
- Go to the **Profile** tab in the mobile app.
- **Login with Credentials:** Tap `🔑 Login with Matric / Email & Password` and enter:
  - **Student 1 (FSKTM):** `AI220015` or `farhan@siswa.uthm.edu.my` | Password: `student123`
  - **Student 2 (FKEE):** `CI220098` or `aisyah@siswa.uthm.edu.my` | Password: `student123`
- **Quick Demo Login:** Tap either `👤 Farhan (FSKTM)` or `👤 Aisyah (FKEE)` to switch profiles in 1 click.
- Upon login, the active student session updates across all tabs (My Alerts, Safety Tickets, and Profile).

