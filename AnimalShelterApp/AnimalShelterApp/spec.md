﻿# Project Roadmap: Pet Medication Tracker

## Tech Stack: Blazor WebAssembly (Frontend) & Google Firebase (Backend)
This roadmap breaks the project into six distinct phases, each with actionable steps. The goal is to build a robust Minimum Viable Product (MVP) efficiently, test it with real users, and prepare for a successful launch.

## Phase 1: Foundation & Setup (Estimated Time: 1-2 Days)
**Goal:** Establish a solid foundation for the project by setting up the development environment, creating the necessary cloud services, and structuring the initial codebase.

### Step 1: Environment Setup
- [x] Ensure you have the latest .NET SDK installed.
- [x] Install Visual Studio 2022 (or your preferred IDE like VS Code with C# extensions).
- [x] Install the Firebase CLI (Command Line Interface) for later deployment.

### Step 2: Firebase Project Creation
- [x] Go to the Firebase Console and create a new project.
- [x] In your new project, navigate to the "Build" section.
- [x] Enable Firestore Database. Start in test mode for now (we'll add security rules later).
- [x] Enable Authentication. For the MVP, enable the "Email/Password" sign-in provider.
- [x] Enable Storage to handle pet photo uploads.

### Step 3: Blazor Project Initialization
- [x] In Visual Studio or using the command line, create a new Blazor WebAssembly App.
- [x] Do not select the "ASP.NET Core Hosted" option, as Firebase will be our host and backend.
- [x] Choose ".NET 9.0" or newer.
- [x] Configure it as a Progressive Web App (PWA). This is a crucial setting that allows users to "install" the app to their device's home screen.

### Step 4: Connect Blazor to Firebase
- [x] In the Firebase Console, go to Project Settings and add a new "Web app".
- [x] Copy the `firebaseConfig` object. You will need these keys.
- [x] Store these keys securely in your Blazor app's `wwwroot/appsettings.json` for local development and use environment-specific files (e.g., `appsettings.Development.json`) for different environments.
- [x] Add NuGet packages to your Blazor project to interact with Firebase. A good starting point is a community-maintained library that wraps the Firebase REST APIs for .NET.

### Step 5: Version Control
- [x] Initialize a Git repository for your project.
- [x] Create an initial commit with your new project structure.
- [x] Push the repository to a service like GitHub or Azure DevOps.

## Phase 2: Data Modeling & Security (Estimated Time: 2-3 Days)
**Goal:** Define the application's data structure and implement the security rules to protect user data.

### Step 1: Define C# Data Models
- [x] Create a `Shared` folder or project for your C# classes that will represent your Firestore documents. These will be used by your Blazor app.
- [x] `Shelter.cs`: Id, Name, Address
- [x] `UserProfile.cs`: Uid, Email, DisplayName, ShelterId
- [x] `Animal.cs`: Id, Name, Species, Breed, DateOfBirth, PhotoUrl, IsActive
- [x] `Medication.cs`: Id, Name, DefaultDosage, Instructions
- [x] `ScheduledDose.cs`: Id, AnimalId, MedicationId, Dosage, TimeOfDay (e.g., "08:00"), Notes
- [x] `DoseLog.cs`: Id, ScheduledDoseId, AnimalId, MedicationName, Dosage, TimeAdministered (Timestamp), AdministeredByUid, WasGiven (bool)

### Step 2: Design Firestore Collection Structure
- [x] Plan how your C# models map to Firestore. A good, scalable structure is:
- [x] `/shelters/{shelterId}` - Top-level collection for each organization.
- [x] `/users/{userId}` - Stores user profile info, linking them to a shelter.
- [x] `/shelters/{shelterId}/animals/{animalId}` - Sub-collection of animals for a specific shelter.
- [x] `/shelters/{shelterId}/medications/{medicationId}` - Sub-collection of custom medications.
- [x] `/shelters/{shelterId}/schedule/{scheduleId}` - All scheduled doses for that shelter.
- [x] `/shelters/{shelterId}/logs/{logId}` - All dose administration logs for that shelter.

### Step 3: Implement Firebase Security Rules
- [x] In the Firebase Console, go to Firestore -> Rules.
- [x] Replace the test rules with rules that enforce your data structure.
- [x] Example Rule: Allow a user to read/write animals only for the shelter they belong to.
- [x] Write similar rules for all your sub-collections.

## Phase 3: Building the MVP Features (Estimated Time: 2-3 Weeks)
**Goal:** Implement the core features that deliver value to the user.

### Step 1: Authentication & User Onboarding
- [x] Build Login, Logout, and Sign-Up pages.
- [x] On sign-up, create a new `Shelter` document and a `UserProfile` document, linking the user to their new shelter.
- [x] Create a central `AuthService` in C# to manage user state across the app.


### Step 2: Animal Management (CRUD)
- [x] Create a page to display a list of all animals in the user's shelter.
- [x] Build a form (as a reusable component) for adding and editing an animal.
- [x] Integrate with Firebase Storage to handle photo uploads.


#### Step 2.1: AnimalDetails View Toggle Functionality
- [x] Add a toggle mechanism to switch between:
    - [x] Current column view
    - [x] New row view

**Row View Specifications**
- [x] Display each animal in a compact row format with the following layout:
    - [x] Small photo of the pet (left-aligned)
    - [x] Animal ID (to be added later)
    - [x] Name
    - [x] Species
    - [x] Breed
    - [x] Date of Birth
- [x] Ensure row items have minimal vertical spacing for a condensed layout.

**Column View Adjustments**
- [x] Organize and allow sorting by the following attributes:
    - [x] Name
    - [x] Species
    - [x] Breed
    - [x] Color
    - [x] Date of Birth
- [x] Maintain the current styling unless otherwise specified.

**Ability to Sort Animals by criteria in Animal View**
- [x] Sort by descending or ascending order of the following: Animal ID, Name, Breed, Color and DOB

**Add autofill option for common breeds and species**
- [x] When user types in breed/species in Animal Editor, text box filters out available options
- [x] If breed/species unavailable, user can continue to type and input data
- [x] Add JSON data of species/breed

**Add HTML/CSS to Animal Details to make animal info available**
- [x] Mark if complete 

**Improve and Expand Medication Schedule**

#### a. Animal Weight Management
- [x] Add Weight field to Animal.cs model (decimal, with units - lbs/kg)
- [x] Update AnimalDetails.razor to display weight with unit toggle
- [x] Update AnimalEditor.razor to include weight input with lbs/kg dropdown
- [x] Add weight validation (reasonable ranges for different species)

#### b. Enhanced Medication Storage and Handling
- [x] Add StorageInstructions field to Medication.cs (text box for "Refrigerate", "Room Temperature", etc.)
- [x] Add HandlingInstructions field to Medication.cs (text box for "Shake Well", "Do Not Crush", etc.)
- [x] Update medication forms to include these new fields
- [x] Display storage/handling info in medication scheduling interface

#### 1. Basic Recurring Patterns
- [x] Update ScheduledDose.cs to include RecurrenceType enum:
  - [x] Daily (every day)
  - [x] EveryXDays (every 2 days, every 3 days, etc.)
  - [x] Weekly (specific days of week: Mon, Wed, Fri)
  - [x] BiWeekly (every 2 weeks)
  - [x] Monthly (once per month on specific date)
  - [x] AsNeeded (PRN - when required, no fixed schedule)
- [x] Add RecurrenceInterval field (int) for "every X days" patterns
- [x] Add DaysOfWeek field (List<DayOfWeek>) for weekly patterns
- [x] Update UI to show recurring pattern selection with intuitive options

#### 2. Enhanced Scheduling Options
- [ ] **Multiple Daily Doses Support**
  - [ ] Add DosesPerDay field (int) to ScheduledDose.cs
  - [ ] Add TimeSlots field (List<string>) to replace single TimeOfDay
  - [ ] Create UI patterns for:
    - [ ] **Fixed Times**: Specific times (8:00 AM, 2:00 PM, 8:00 PM)
    - [ ] **Interval Based**: Every X hours (every 8 hours, every 6 hours)
    - [ ] **Meal-Based**: With breakfast, lunch, dinner options
  - [ ] Add plus button (+) interface near time selection for adding multiple doses
  - [ ] Ensure dashboard handles multiple doses per day correctly

- [ ] **Food Relationship Options**
  - [ ] Add FoodRelationship enum to ScheduledDose.cs:
    - [ ] WithFood
    - [ ] WithoutFood  
    - [ ] BeforeMeal
    - [ ] AfterMeal
    - [ ] DoesNotMatter (default)
  - [ ] Add FoodRelationship field to ScheduledDose.cs
  - [ ] Create dropdown component with clear labels
  - [ ] Display food relationship info in dashboard and reports
  - [ ] Include in medication administration reminders

#### 3. Start Date / End Date AND Indefinite Options
- [ ] Add StartDate field (DateTime) to ScheduledDose.cs
- [ ] Add EndDate field (DateTime?) to ScheduledDose.cs - nullable for indefinite
- [ ] Add IsIndefinite field (bool) to ScheduledDose.cs
- [ ] Add Duration field (int?) for "10 days", "2 weeks" options
- [ ] Create floating calendar date picker component
- [ ] Add UI toggle between "End Date" and "Indefinite" options
- [ ] Update dashboard logic to respect start/end dates

#### 4. Enhanced Time Picker Widget
- [ ] Create TimePickerComponent.razor with:
  - [ ] Hour dropdown (1-12 for 12-hour format)
  - [ ] Minutes dropdown in 15-minute increments (00, 15, 30, 45)
  - [ ] AM/PM toggle buttons
  - [ ] Floating window/modal design
  - [ ] Convert to 24-hour format for storage
  - [ ] Display in user-preferred format

### Step 3: Medication & Scheduling
- [x] Create a simple UI to manage a list of common medications for the shelter.
- [x] On the "Animal Detail" page, build the UI to schedule medications. This will create `ScheduledDose` documents in Firestore.

### Step 4: The Dashboard & Core Logging Workflow
- [x] This is the most critical feature. Design a "Today's Medications" dashboard.
- [x] The page should query all `ScheduledDoses` for the current day.
- [x] Display doses in clear, actionable groups: Overdue, Upcoming, and Completed.
- [x] Each item must have a large, easy-to-tap "Log as Given" button.
- [x] Clicking the button should create a `DoseLog` entry with the current timestamp and user's ID, then visually move the item to the "Completed" section in real-time.
- [x] Create an undo feature for medication given

### Step 5: Reporting
- [x] Create a simple page to generate medical reports.
- [x] Include filters for Animal and a Date Range.
- [x] Fetch the relevant `DoseLog` entries from Firestore.
- [x] Display the results in a clean table.
- [x] Implement a "Download as CSV" button. You can use a library like `CsvHelper` to easily generate the file in C#.

### Step 6: Global Error Handling
- [ ] Implement a global exception handler in your Blazor app.
- [ ] Create a user-friendly error boundary to prevent the app from crashing.
- [ ] Consider logging errors to a service (or a Firestore collection) to help with debugging.

## Phase 4: Testing & Quality Assurance (Estimated Time: 1 Week)
**Goal:** Ensure the application is reliable, bug-free, and works as expected through various levels of testing.

### Step 1: Unit Testing
- [ ] Set up a new xUnit or MSTest project in your solution.
- [ ] Write unit tests for any business logic in your services, such as the `AuthService`.
- [ ] Write unit tests for the CSV generation logic in the reporting feature to ensure data is formatted correctly.
- [ ] Test any complex data transformations or validation logic.

### Step 2: Automated End-to-End (E2E) Testing
- [ ] Set up a new Playwright test project to automate browser testing.
- [ ] Write E2E tests for the most critical user workflows:
    - [ ] User sign-up, login, and logout.
    - [ ] Creating, viewing, updating, and deleting an animal.
    - [ ] Scheduling a new medication for an animal.
    - [ ] Logging a dose from the main dashboard and verifying it moves to "Completed".

### Step 3: User Acceptance Testing (UAT)
- [ ] Perform end-to-end testing of every feature from a user's perspective.
- [ ] Crucially, role-play as a stressed, non-tech-savvy shelter worker. Is it truly easy? Can you log a medication in under 10 seconds?
- [ ] Fix any bugs or confusing workflows you discover.

## Phase 5: Refinement & Deployment (Estimated Time: 1 Week)
**Goal:** Polish the application, test it thoroughly, and deploy it for the first users.

### Step 1: Mobile-First UI/UX Polish
- [ ] Go through every page and ensure it is clean, fast, and intuitive on a mobile device.
- [ ] Use a simple UI framework like Bootstrap (default in Blazor) or Tailwind CSS to ensure responsiveness.
- [ ] Simplify wording, enlarge buttons, and remove any non-essential visual clutter.

### Step 2: Deployment to Firebase Hosting
- [ ] Build your Blazor app in Release configuration.
- [ ] Use the Firebase CLI to deploy. The command is typically `firebase deploy --only hosting`.
- [ ] This will upload your application's static files (from the `wwwroot` folder) to Firebase's global CDN.

## Phase 6: Launch & Feedback Loop (Ongoing)
**Goal:** Onboard your first users, gather critical feedback, and plan the future of the product.

### Step 1: First Customer Strategy
- [ ] As planned, contact 3-5 local, independent shelters.
- [ ] Offer the tool for free, indefinitely, in exchange for being case studies.
- [ ] Personally walk them through the app and help them set up their first few animals.

### Step 2: Establish a Feedback Channel
- [ ] Provide a simple email address or a "Feedback" link within the app (e.g., a `mailto:` link) for users to report issues or suggest features.

### Step 3: Iterate Based on Feedback
- [ ] Prioritize the feedback you receive. The first few weeks of real-world use will be invaluable.
- [ ] Plan for small, frequent updates to address user pain points quickly.

### Step 4: Legal & Compliance
- [ ] Create a Privacy Policy that clearly explains what data you collect and how it's used.
- [ ] Create a Terms of Service document for users to agree to.
- [ ] Add links to these documents in the app's footer.

### Step 5: Plan for Monetization
- [ ] Once the product is stable and validated by your initial users, you can confidently implement the planned pricing model (Free for 5 animals, then $19/month).
- [ ] This will involve integrating a payment provider like Stripe.