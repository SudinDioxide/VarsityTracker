# BIE33103 .NET PROGRAMMING — DEMO VIDEO RECORDING SCRIPT
## **Inter-Faculty Varsity Tracker (Theme: S.P.O.R.T.S)**
### Video Length: 10–12 Minutes &bull; All 5 Group Members Speaking &bull; Target: 20/20 Marks

---

## 🎬 Pre-Recording Checklist
1. **Screen Resolution:** 1920x1080 (1080p).
2. **Web Application Running:** Start `VarsityTracker.Web` at `http://localhost:5299`.
3. **Browser Prepared:** Open Chrome or Edge in full screen at `http://localhost:5299`.
4. **Mobile App Prepared:** Launch `VarsityTracker.Mobile` on Windows Desktop or Android Emulator side-by-side with browser.
5. **Clean Data State:** Verify sample fixtures and standings are displayed on the home dashboard.

---

## ⏱️ Video Timeline Breakdown

```mermaid
timeline
    title 10-12 Minute Presentation & Demonstration Video
    00:00 - 01:30 : Member 1 : Introduction, Problem Statement, Project Objectives
    01:30 - 03:00 : Member 2 : Solution Architecture, ERD Models & Data Seeding
    03:00 - 05:30 : Member 3 : Web CRUD Live Demo (Modules 1, 2, 3)
    05:30 - 07:30 : Member 4 : Value-Added Live Scoring Console (Module 4 AJAX)
    07:30 - 10:00 : Member 5 : .NET MAUI Mobile App Live Demo (Mobile CRUD 1, 2, 3)
    10:00 - 11:30 : All Members : Q&A Preparation, Summary & Rubric Verification
```

---

### **Scene 1: Introduction, Problem Statement & Objectives (00:00 – 01:30)**
- **Speaker:** **Member 1 (Project Leader)**
- **Screen:** Show Slide 1 & Slide 2 (or [PROJECT_REPORT.md](file:///c:/Users/hasur/OneDrive/Desktop/DOTNET/PROJECT/docs/PROJECT_REPORT.md)).
- **Voiceover Script:**
  > "Assalamualaikum and a very good day to our lecturer, Dr. Radzi, and our fellow coursemates. We are Group 1, and today we present our semester project for BIE33103 .NET Programming: the **Inter-Faculty Varsity Tracker**, built under the designated theme S.P.O.R.T.S.
  >
  > At UTHM, our annual inter-faculty sporting championships involve hundreds of student athletes across faculties like FSKTM, FKEE, and FKMP. However, traditional sports coordination suffers from significant challenges: delayed score calculations, lack of immediate hazard reporting when campus facilities are damaged, and zero mobile notifications for students wishing to support their faculty teams.
  >
  > To solve this, our team engineered an end-to-end multi-platform system: a central ASP.NET Core MVC web application connected to a SQL/SQLite database via EF Core, paired with a companion .NET MAUI mobile application for student athletes and fans."

---

### **Scene 2: Architecture & Structural Data Model (01:30 – 03:00)**
- **Speaker:** **Member 2 (Database & Architecture Specialist)**
- **Screen:** Show Visual Studio Solution Explorer (`VarsityTracker.sln`) and the Entity-Relationship Diagram from the report.
- **Voiceover Script:**
  > "Hi everyone, I am Member 2. Our solution is structured into three clean projects adhering to clean architecture:
  >
  > 1. `VarsityTracker.Core`: Contains our shared domain entities: `User`, `Faculty`, `Sport`, `Venue`, `Player`, `Match`, `MatchScore`, `MatchReminder`, and `VenueFeedback`.
  > 2. `VarsityTracker.Web`: Our ASP.NET Core MVC web application with five full CRUD modules, Cookie Authentication for Role-Based Access Control, and a RESTful API powering our mobile app.
  > 3. `VarsityTracker.Mobile`: A cross-platform .NET MAUI application delivering three dedicated mobile CRUD modules.
  >
  > On initial startup, our `DbInitializer` automatically provisions authentic UTHM faculties (such as FSKTM, FKEE, FAST), sports disciplines, and campus venues, ensuring the system runs immediately on IIS Express localhost without complex configuration."

---

### **Scene 3: Web Modules 1, 2 & 3 Live Demonstration (03:00 – 05:30)**
- **Speaker:** **Member 3 (Web Modules Lead)**
- **Screen:** Web Browser at `http://localhost:5299`.
- **Live Demo Steps:**
  1. **Quick Role Switcher:**
     - Point to the top login bar: "We have built a demonstration quick-switcher allowing evaluators to immediately log in as Admin, Scorekeeper, or Student."
     - Click **Admin (AD001)** &rarr; Show that Admin navigation dropdown appears.
  2. **Module 1 (Sports & Rules Engine CRUD):**
     - Navigate to *Sports Rules*.
     - Click **Register New Sport** &rarr; Enter `Sepak Takraw`, Scoring: `Sets`, Max Periods: `3`.
     - Click **Save** &rarr; Show the new sport in the table.
     - Click **Edit** &rarr; Change sets to 5.
  3. **Module 2 (Faculties & Athlete Rosters CRUD):**
     - Navigate to *Rosters & Teams* &rarr; *Athlete Rosters*.
     - Filter by faculty: Select `FSKTM`.
     - Click **Register New Athlete** &rarr; Enter Name: `Muhammad Akmal`, Matric: `AI220300`, Role: `Captain / Striker`.
     - Click **Save** &rarr; Show the new athlete in the squad list.
  4. **Module 3 (Venues & Facilities CRUD):**
     - Navigate to *Venues*.
     - Show campus venues (`Main Stadium`, `Court A`, `Tennis Court 3`).
     - Point to the **Maintenance Status Toggle**: Click the toggle button to demonstrate marking a facility under maintenance, preventing accidental match bookings.

---

### **Scene 4: Value-Added Live Scoring Command Center (05:30 – 07:30)**
- **Speaker:** **Member 4 (Real-Time Engine Lead)**
- **Screen:** Web Browser at `http://localhost:5299/Matches` and `/LiveScore/Console/1`.
- **Live Demo Steps:**
  1. **Fixture Creation (Module 4 CRUD):**
     - Navigate to *Fixtures* &rarr; Click **Schedule New Fixture**.
     - Select Home: `FSKTM`, Away: `FKEE`, Sport: `Futsal`, Venue: `Futsal Court 1`, Kickoff: Tomorrow 4:00 PM, Official: `Amirul Hakim (Scorekeeper)`.
     - Click **Confirm & Publish Fixture**.
  2. **Scorekeeper Command Console (Value-Added Technical Innovation):**
     - Switch user to **Scorekeeper (SK001)**.
     - Navigate to *Live Center* &rarr; Click **Launch Scorekeeper Console**.
     - Click **Set Match Live** &rarr; Status changes to LIVE with red pulsing badge.
     - Click **"+1 Futsal Goal"** for FSKTM: Notice the score changes from 0 to 1, and the green **"Database Synced"** badge flashes.
     - **Emphasize to Evaluator:**
       > "Notice that the browser tab did NOT reload. Our live scoring console makes asynchronous AJAX calls to the backend controller, updating Entity Framework Core and recalculating period totals in real-time."
     - Click **Next Period** &rarr; Second half scores tracked separately.
     - Click **End Match & Lock Standings** &rarr; Show that the match status updates to Completed and the final score locks.
  3. **Standings Automation:**
     - Return to *Dashboard (Home)*.
     - Show the **Inter-Faculty Championship Standings Table**: FSKTM is awarded 3 championship points, and goals for/against and differentials are dynamically recalculated!

---

### **Scene 5: .NET MAUI Mobile App Live Demonstration (07:30 – 10:00)**
- **Speaker:** **Member 5 (Mobile Development Lead)**
- **Screen:** Show the `.NET MAUI Mobile Application` running on screen.
- **Live Demo Steps:**
  1. **Live Score Viewer (Value-Added Mobile Feature):**
     - Open the *Live & Matches* tab.
     - Show the live match cards with colored badges, venue pins, and updated scores synced from the web application.
  2. **Mobile CRUD #2 — Personal Match Reminders:**
     - Tap **"🔔 Set Match Alert"** on the upcoming Badminton match (FKAAB vs FAST).
     - Select `30 minutes before kickoff`.
     - Switch to the *My Alerts* tab &rarr; Show the scheduled alert card.
     - Tap **"✏️ Change Time"** &rarr; Change from 30m to `60 minutes`.
     - Tap **"🗑️ Remove Alert"** &rarr; Demonstrate deleting the reminder.
  3. **Mobile CRUD #3 — Venue Hazard & Safety Defect Tickets:**
     - Switch to the *Safety Tickets* tab.
     - Tap **"➕ Submit New Hazard Report"**.
     - Select Venue: `Main Stadium UTHM`.
     - Enter Subject: `Loose tartan track rubber near lane 1`.
     - Enter Details: `Trip hazard for sprinters during morning heats`.
     - Tap **Submit** &rarr; Notice ticket appears with "Pending" status badge.
     - Tap **"✏️ Edit Details"** &rarr; Amend text & save.
     - Tap **"🗑️ Withdraw"** &rarr; Show confirmation dialog and cancellation.
  4. **Mobile CRUD #1 — Student Profile Management:**
     - Switch to the *Profile* tab.
     - Show current student credentials (`Muhammad Farhan`, `AI220015`, `FSKTM`).
     - Edit Full Name to include middle name & change faculty preference to `FKEE`.
     - Tap **"💾 Save Profile Changes"** &rarr; Show instant profile badge update.
     - Point to **"➕ Register Another Profile"** and **"🗑️ Deactivate Account"** buttons.

---

### **Scene 6: Conclusion & Rubric Compliance (10:00 – 11:30)**
- **Speaker:** **All 5 Members (Closing Statements)**
- **Screen:** Show the Project Assessment Rubric Matrix from [PROJECT_REPORT.md](file:///c:/Users/hasur/OneDrive/Desktop/DOTNET/PROJECT/docs/PROJECT_REPORT.md).
- **Voiceover Script:**
  - **Member 1:** "To summarize our compliance with the course rubric: for Project 1 Web, we developed five full database CRUD modules, exceeding the required four, running cleanly on IIS localhost with an AJAX live scoring console."
  - **Member 4:** "For Project 2 Mobile, we built three dedicated CRUD modules in .NET MAUI along with a live match score ticker, earning the full 4 marks for mobile functionality."
  - **Member 2:** "Both projects compile with zero errors, supported by a formal academic report, structural ERD models, and comprehensive documentation."
  - **Member 3:** "Thank you very much for your time and guidance throughout this semester. We are now ready for questions!"

