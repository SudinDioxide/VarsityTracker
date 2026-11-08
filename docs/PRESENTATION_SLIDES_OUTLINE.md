# BIE33103 .NET PROGRAMMING — PRESENTATION SLIDES & DEMO SCRIPT
## **Inter-Faculty Varsity Tracker (Theme: S.P.O.R.T.S)**
### Duration: 10–12 Minutes &bull; All 5 Group Members Active

---

## ⏱️ Timeline & Presentation Structure

| Slide / Phase | Time | Speaker | Content / Live Demo Focus |
| :---: | :---: | :---: | :--- |
| **Slide 1–2** | 0:00 – 1:30 | **Member 1 (Leader)** | Introduction, Problem Statement, Objectives, S.P.O.R.T.S Theme |
| **Slide 3–4** | 1:30 – 3:30 | **Member 2** | System Architecture, ERD Structural Model, EF Core Seeding |
| **Slide 5–7** | 3:30 – 6:30 | **Member 3** | **Live Web Demo:** Modules 1–5 CRUD & Scorekeeper AJAX Console |
| **Slide 8–10** | 6:30 – 9:00 | **Member 4 & 5** | **Live Mobile Demo:** .NET MAUI App & 3 Student Mobile CRUDs |
| **Slide 11–12** | 9:00 – 10:30 | **All Members** | Conclusion, Rubric Compliance, Value-Added Innovation |
| **Q&A** | 10:30 – 12:00 | **All Members** | Evaluator Q&A and Technical Discussion |

---

## 📊 Slide-by-Slide Details

### Slide 1: Title & Group Introduction
- **Slide Title:** Inter-Faculty Varsity Tracker
- **Subtitle:** Multi-Platform Sports Championship Management System
- **Details:**
  - Course: BIE33103 .NET Programming (Sem 1 2026/2027)
  - Faculty: Fakulti Sains Komputer dan Teknologi Maklumat (FSKTM), UTHM
  - Group: Group 1 &bull; Members: [Names & Matric Numbers]
- **Speaker Cue (Member 1):**
  > "Good morning to our esteemed lecturer and classmates. Today our team presents the Inter-Faculty Varsity Tracker, an integrated solution built in ASP.NET Core MVC and .NET MAUI under the S.P.O.R.T.S theme."

---

### Slide 2: Problem Statement & Objectives
- **Key Points:**
  - Legacy paper-based tournament management produces score lag and delayed standings.
  - Campus sports facility hazards (court surface damage, lighting defects) lack rapid reporting.
  - Students lack mobile match kickoff alerts and live set-by-set progress tracking.
- **Objectives:**
  1. Build an ASP.NET Core MVC web app with 5 complete database CRUD modules.
  2. Implement an AJAX-powered Live Scorekeeper Console for real-time scoring without page refreshes.
  3. Deploy a .NET MAUI mobile app implementing 3 dedicated student CRUD modules.

---

### Slide 3: System Architecture & Technologies
- **Technology Stack:**
  - **Shared Core:** C# .NET Class Library (`VarsityTracker.Core`)
  - **Web MVC:** ASP.NET Core MVC + Entity Framework Core (SQLite / SQL Server) + Bootstrap 5
  - **Mobile:** .NET MAUI cross-platform client with XAML and MVVM pattern
  - **Security:** Cookie-Based Authentication + Role-Based Access Control (Admin, Scorekeeper, Student)
  - **Hosting:** IIS Express & Localhost server ready

---

### Slide 4: Database Model & Entity Relationships (ERD)
- **Entities Showcased:**
  - `User`, `Faculty`, `Sport`, `Venue`, `Player`, `Match`, `MatchScore`, `MatchReminder`, `VenueFeedback`
- **Key Relationships:**
  - One-to-Many: Faculty &rarr; Players, Venue &rarr; Matches, Sport &rarr; Matches
  - Cascade Deletion & Restrict behavior to preserve integrity between home/away teams.
  - Automated seeding with authentic UTHM faculties (FSKTM, FKEE, FKMP, FKAAB, FAST, FPTP).

---

### Slide 5–7: Live Web Application Demo (Member 3)
- **Demo Step 1: Quick Role Switcher (Account Controller)**
  - Show evaluator login screen with one-click demo login buttons (`Admin`, `Scorekeeper`, `Student`).
- **Demo Step 2: Module 1 & 2 CRUD (Sports & Rosters)**
  - Demonstrate creating a new sport discipline with custom scoring rules (Goals, Sets, Points).
  - Demonstrate adding student athletes to faculty rosters with jersey roles.
- **Demo Step 3: Module 3 CRUD (Venues & Facility Maintenance)**
  - Show campus facilities list and demonstrate the one-click `IsAvailable` toggle for safety repairs.
- **Demo Step 4: Module 4 CRUD & Value-Added AJAX Live Score Console**
  - Demonstrate creating a match fixture between FSKTM and FKEE.
  - Open `/LiveScore/Console/{id}`:
    - Click **"+1 FSKTM Goal"** &rarr; Notice instant score update and "Database Synced" badge without page reload.
    - Add **"Next Period"** (Half 2 / Set 2) &rarr; Table dynamically displays period-by-period breakdown.
    - Click **"End Match"** &rarr; Locks scores and dynamically updates the Inter-Faculty Standings on the homepage!

---

### Slide 8–10: Live Mobile Application Demo (Members 4 & 5)
- **Demo Step 5: Mobile CRUD #1 — Student Profile Management & Authentication**
  - Show student profile tab with matric number, faculty badge, and the official UTHM crest avatar.
  - Edit display name and primary faculty &rarr; Save updates in real time.
  - Demonstrate student credential login (`/api/mobile/login`) and the 1-tap quick demo switcher (`Farhan - FSKTM` / `Aisyah - FKEE`).
  - Demonstrate "Register Another Profile" dialog and account deactivation.
- **Demo Step 6: Mobile CRUD #2 — Personal Match Reminders**
  - Browse "Live & Matches" tab &rarr; Tap **"🔔 Set Match Alert"** on upcoming fixture.
  - Select lead time (e.g. 15 or 30 minutes before kickoff).
  - Navigate to "My Alerts" tab &rarr; Demonstrate updating lead time and removing alerts.
- **Demo Step 7: Mobile CRUD #3 — Venue Hazard & Safety Tickets**
  - Tap **"➕ Submit New Hazard Report"**.
  - Select venue (e.g., Tennis Court 3), enter subject "Torn perimeter net", and submit.
  - View ticket with "Pending" status badge.
  - Edit report notes & withdraw ticket.
  - Switch to Web Admin screen to show the ticket received by administrators with the option to close the venue!
- **Demo Step 8: Value-Added Real-Time Live Sync**
  - Highlight the green `● Live Sync (2s)` indicator on the mobile header.
  - Keep the mobile screen open on "Live & Matches" while clicking `+1 Goal` on the Web AJAX console.
  - Demonstrate scores changing automatically in real time without refreshing or leaving the screen.

---

### Slide 11: Value-Added Creativity & Rubric Summary
- **Full Marks Verification (20/20):**
  - **Prj1 Web:** 5 full CRUD modules (exceeds requirement of 4) + AJAX live scorekeeping.
  - **Prj2 Mobile:** 3 dedicated CRUD modules (full 4 marks on mobile rubric) + Live score ticker.
  - **Error-Free:** Both Web MVC and .NET MAUI compile with **0 Errors**.
  - **IIS Localhost:** Configured with IIS Express launch settings and portable SQLite database.

---

### Slide 12: Conclusion & Q&A
- **Closing Statement:**
  > "The Inter-Faculty Varsity Tracker delivers a unified, production-ready solution solving real-world athletic tournament challenges at UTHM. Thank you, and we welcome your questions!"

