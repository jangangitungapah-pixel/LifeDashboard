// ============================================
// LIFE DASHBOARD - GAMIFICATION QA TEST PLAN
// ============================================
// 
// This document outlines manual and automated testing for the 7 new gamification features.
// Features: XP, Level Up, Confetti, Badges, Heatmap, AI Coach, Notifications
//
// Test Environment:
// - Target: Windows (Desktop) + Android (Emulator or Device)
// - Build: .NET MAUI, .NET 10
// - Duration: ~30 minutes per platform
//
// ============================================
//
// FEATURE 1: XP SYSTEM & LEVEL PROGRESSION
// ========================================
//
// Acceptance Criteria:
// ? User gains 10 XP when completing a task
// ? User gains 5 XP when completing a habit
// ? XP progress bar updates in real-time
// ? Level calculation follows formula: level = floor(sqrt(totalXp / 100))
// ? Level display updates immediately on level up
// ? XP persists across app restarts
//
// Test Cases:
//
// TC1.1: Complete Task -> XP Added
// Steps:
//   1. Launch app on any device
//   2. Add a task "Test Task"
//   3. Check the task box
//   4. Observe XpLabel should show "10 / 100 XP" (if level 0->1)
//   5. Observe XpProgressBar fills by 10%
// Expected: ? XP increased by 10
//
// TC1.2: Complete Habit -> XP Added
// Steps:
//   1. Add a habit "Morning Exercise"
//   2. Check the habit box
//   3. Observe XpLabel
// Expected: ? XP increased by 5
//
// TC1.3: Level Calculation at 100 XP
// Steps:
//   1. Complete 10 tasks (10 * 10 = 100 XP)
//   2. Observe LevelLabel
// Expected: ? Level shows "Level 1" (floor(sqrt(100/100)) = 1)
//
// TC1.4: Level Calculation at 400 XP
// Steps:
//   1. Continue to 400 XP
// Expected: ? Level shows "Level 2" (floor(sqrt(400/100)) = 2)
//
// TC1.5: XP Persists After Restart
// Steps:
//   1. Achieve 150 XP (complete 15 tasks or mix)
//   2. Close app completely
//   3. Reopen app
//   4. Observe XpLabel
// Expected: ? XP and level match previous session
//
// ============================================
//
// FEATURE 2: LEVEL UP & CONFETTI ANIMATION
// ========================================
//
// Acceptance Criteria:
// ? LevelUpPopup displays when level threshold crossed
// ? Popup shows: "LEVEL UP!", new level, total XP, optional badge
// ? Confetti animation plays (non-blocking)
// ? Popup closes after user taps "Awesome!"
// ? Confetti is visible for 2 seconds
// ? Animations are smooth, no app freeze
//
// Test Cases:
//
// TC2.1: Level Up Popup Appears at 100 XP
// Steps:
//   1. Clear app data (delete user_profile.json)
//   2. Complete exactly 10 tasks (100 XP)
//   3. Observe screen during last task completion
// Expected: 
//   ? LevelUpPopup appears with scale/fade animation
//   ? Shows "Level 1"
//   ? Shows "Total XP: 100"
//
// TC2.2: Confetti Animation Plays
// Steps:
//   1. Trigger level up (same as TC2.1)
// Expected:
//   ? Colorful squares fall from top to bottom
//   ? Duration ~2 seconds
//   ? UI remains responsive (can scroll during confetti)
//
// TC2.3: Popup Closes on Button Click
// Steps:
//   1. After level up popup appears
//   2. Tap "Awesome!" button
// Expected:
//   ? Popup fades out
//   ? Confetti stops
//   ? Popup is hidden
//
// TC2.4: Multiple Level Ups
// Steps:
//   1. Continue adding XP to reach Level 2 (400 XP total)
//   2. Repeat level up process
// Expected:
//   ? Popup appears again for Level 2
//   ? Confetti plays again
//   ? New XP value shown
//
// ============================================
//
// FEATURE 3: STREAK BADGES
// ========================
//
// Acceptance Criteria:
// ? 3-day streak badges awarded at 3-day streak
// ? 7-day streak badges awarded at 7-day streak
// ? 14-day streak badge awarded
// ? 30-day streak badge awarded
// ? Badges display in BadgesContainer
// ? Tapping badge shows description
// ? Bonus XP awarded: 3-day=+10, 7=+30, 14=+70, 30=+200
//
// Test Cases:
//
// TC3.1: 3-Day Streak Badge
// Steps:
//   1. Create a habit "Daily Reading"
//   2. Complete it for 3 consecutive days
//      - Day 1: Check habit, close app
//      - Day 2: Open app next day, check habit
//      - Day 3: Open app next day, check habit
//   3. Observe BadgesContainer on Day 3
// Expected:
//   ? Badge with ?? icon appears
//   ? Badge labeled "3-Day Streak"
//   ? XP increased by 10 on Day 3
//
// TC3.2: Tap Badge to See Description
// Steps:
//   1. After earning 3-day streak badge
//   2. Tap the ?? badge
// Expected:
//   ? Alert dialog shows badge name and description
//   ? Description: "Complete a habit for 3 consecutive days"
//
// TC3.3: 7-Day Streak Badge
// Steps:
//   1. Continue same habit for 7 days total
// Expected:
//   ? Badge with ? icon earned on day 7
//   ? XP increased by 30
//   ? Two badges now visible (3-day + 7-day)
//
// TC3.4: 14-Day Streak Badge
// Steps:
//   1. Continue to day 14
// Expected:
//   ? Badge with ?? icon earned
//   ? XP increased by 70
//
// TC3.5: 30-Day Streak Badge
// Steps:
//   1. Continue to day 30
// Expected:
//   ? Badge with ?? icon earned
//   ? XP increased by 200
//   ? Four badges displayed
//
// ============================================
//
// FEATURE 4: CALENDAR HEATMAP
// ===========================
//
// Acceptance Criteria:
// ? Heatmap renders last 90 days as grid
// ? Days are colored by intensity (0.0 = no activity, 1.0 = full completion)
// ? Color gradient: dark -> cyan -> blue -> darker blue
// ? Today is included in heatmap
// ? Tapping a day shows daily stats popup
// ? Popup displays: date, tasks done/total, habits done/total, percentage
// ? Legend shows intensity scale
//
// Test Cases:
//
// TC4.1: Heatmap Renders
// Steps:
//   1. Scroll to heatmap section on MainPage
// Expected:
//   ? Grid visible with title "?? Last 90 Days"
//   ? Grid contains 13 columns (weeks) x 7 rows (days)
//   ? Legend visible with 5 color samples
//
// TC4.2: Colors Change Based on Completion
// Steps:
//   1. On Day 1: Complete 0/3 tasks, 0/2 habits (0% intensity)
//      - Heatmap cell should be dark (#0f172a)
//   2. On Day 2: Complete 2/3 tasks, 1/2 habits (75% intensity)
//      - Heatmap cell should be cyan-ish
//   3. On Day 3: Complete 3/3 tasks, 2/2 habits (100% intensity)
//      - Heatmap cell should be dark blue
// Expected: ? Colors match intensity
//
// TC4.3: Tap Day to See Stats
// Steps:
//   1. Tap a heatmap cell
// Expected:
//   ? Alert displays date in YYYY-MM-DD format
//   ? Shows "2/3 tasks, 1/2 habits, 83.3%"
//
// TC4.4: Heatmap Updates Daily
// Steps:
//   1. Complete items for today
//   2. Observe today's heatmap cell (rightmost)
// Expected:
//   ? Cell color updates to match today's completion
//   ? Cell is among the most recent visible
//
// ============================================
//
// FEATURE 5: AI COACH
// ==================
//
// Acceptance Criteria:
// ? "?? Coach Tip" section displays in main UI
// ? Tip loads asynchronously on app startup
// ? Tip text ? 140 chars
// ? Tapping "Get Another Tip" fetches a new tip
// ? Tip source is "local" by default
// ? Fallback to LocalAIStub if remote fails
// ? No UI freeze during tip loading
//
// Test Cases:
//
// TC5.1: Initial Tip Loads
// Steps:
//   1. Launch app
//   2. Wait 1-2 seconds
//   3. Observe CoachTipLabel
// Expected:
//   ? Tip appears (e.g., "Try the Pomodoro Technique: 25 min work, 5 min break.")
//   ? Plus motivation line below
//   ? CoachTipLabel is readable (font size 12)
//
// TC5.2: Get Another Tip Button
// Steps:
//   1. Observe initial tip
//   2. Tap "Get Another Tip"
//   3. Wait ~1 second
// Expected:
//   ? New tip appears
//   ? Label shows "Loading..." briefly
//   ? Tip differs from previous one (most likely, due to randomization)
//
// TC5.3: Tip Text Is Reasonable Length
// Steps:
//   1. Get several tips (5-10 taps)
// Expected:
//   ? All tips fit within CoachTipLabel width
//   ? No text cutoff
//   ? Readable line breaks
//
// TC5.4: Async Loading (No Freeze)
// Steps:
//   1. Tap "Get Another Tip" multiple times rapidly
//   2. Simultaneously try to scroll or interact with buttons
// Expected:
//   ? App remains responsive
//   ? Can scroll MainPage while tip is loading
//   ? No "not responding" errors
//
// ============================================
//
// FEATURE 6: NOTIFICATIONS (ANDROID)
// ==================================
//
// Acceptance Criteria (Android):
// ? Notification service initializes on app startup
// ? Can schedule daily notifications
// ? Scheduled notification fires at specified time
// ? Notification includes title, body, and is tappable
// ? Repeated daily until canceled
// ? Notifications are cancelable
// ? Permission: POST_NOTIFICATIONS is granted
// ? Notification channel is created
//
// Test Cases (Requires Android):
//
// TC6.1: Schedule Reminder (Manual until UI implemented)
// Steps (Developer):
//   1. In code, call:
//      await notificationService.ScheduleDailyAsync(
//          "habit_1",
//          "Morning Exercise",
//          "Time to exercise!",
//          8,  // 8 AM
//          30  // 30 minutes
//      );
//   2. Wait until 8:30 AM or set device time forward
//   3. Observe notification tray
// Expected:
//   ? Notification appears in system tray
//   ? Title: "Morning Exercise"
//   ? Body: "Time to exercise!"
//   ? Tappable (opens app on tap)
//
// TC6.2: Cancel Notification
// Steps (Developer):
//   1. Call: await notificationService.CancelAsync("habit_1");
//   2. Check notification tray and logs
// Expected:
//   ? Notification is removed
//   ? Future repeats do not fire
//
// ============================================
//
// FEATURE 7: ACHIEVEMENT (Complete All Tasks + Habits)
// =====================================================
//
// Acceptance Criteria:
// ? When all tasks AND all habits are completed, alert is shown once per day
// ? Alert text: "?? Achievement Unlocked!\nYou completed EVERYTHING today!"
// ? Confetti plays along with alert
// ? Achievement message shown only once per day (even after restart)
// ? Resets at midnight
//
// Test Cases:
//
// TC7.1: All Tasks Completed
// Steps:
//   1. Create 2 tasks: "Task A", "Task B"
//   2. Complete both
//   3. Create 0 habits (or complete all if exist)
// Expected:
//   ? No achievement alert if habits exist and not completed
//
// TC7.2: All Tasks + All Habits Completed
// Steps:
//   1. Create 2 tasks, 1 habit
//   2. Complete all 3 items
//   3. Observe screen
// Expected:
//   ? Alert appears: "?? Achievement Unlocked!..."
//   ? Confetti plays
//   ? "Nice!" button closes alert
//
// TC7.3: Achievement Shown Only Once Per Day
// Steps:
//   1. After TC7.2, tap "Nice!"
//   2. Uncheck all items, then recheck them again
// Expected:
//   ? Alert does NOT appear again (already shown today)
//
// TC7.4: Achievement Resets Next Day
// Steps:
//   1. Complete TC7.2
//   2. Set device time to next day (or wait until midnight)
//   3. Reopen app
//   4. Complete all items again
// Expected:
//   ? Alert appears again
//   ? Flag resets properly
//
// ============================================
//
// CROSS-PLATFORM TESTING
// ====================
//
// Windows Desktop:
//  - Confetti visible and smooth
//  - UI scaling appropriate
//  - Heatmap displays correctly (grid alignment)
//  - AI Coach tip loads quickly
//  - No platform-specific errors
//
// Android:
//  - Dark theme respected
//  - Scrolling smooth
//  - Confetti performance good (no lag)
//  - Notifications integrate with system tray
//  - Permissions (POST_NOTIFICATIONS) requested
//  - App survives configuration changes (orientation, etc.)
//
// ============================================
//
// PERFORMANCE & STABILITY
// =======================
//
// PerfTC1: App Startup Time
// - Measure time from launch to fully loaded
// - Target: < 3 seconds
// - Check: No ANRs (Android), no hangs (Windows)
//
// PerfTC2: XP Update Performance
// - Complete 50 tasks in sequence
// - Monitor: Smooth animations, no jank
// - Expected: All XP bars animate smoothly
//
// PerfTC3: Heatmap Rendering
// - Scroll heatmap repeatedly
// - Expected: No lag, smooth scrolling
//
// PerfTC4: Confetti on Older Devices
// - Test on mid-range Android device (if available)
// - Expected: Confetti plays at 30+ FPS
//
// ============================================
//
// DATA PERSISTENCE
// ================
//
// DataTC1: Profile Saved After Each XP Gain
// - Complete task
// - Force-close app
// - Reopen
// - Verify: XP persists
//
// DataTC2: Habits Persist with Streaks
// - Create habit, complete 3 days
// - Close app
// - Reopen
// - Verify: Streak unchanged
//
// DataTC3: Daily Stats Persisted
// - Check heatmap for past 90 days
// - Close app
// - Reopen
// - Verify: Heatmap colors unchanged
//
// DataTC4: Badges Persist
// - Earn 3-day streak badge
// - Force-close app
// - Reopen
// - Verify: Badge still visible
//
// ============================================
//
// EDGE CASES & ERROR HANDLING
// ==========================
//
// EdgeTC1: No Tasks or Habits
// - Uninstall all tasks and habits
// - Observe: Heatmap displays 0% intensity
// - Observe: Level/XP visible but no progress possible
//
// EdgeTC2: Rapid Completions
// - Check 10 tasks in quick succession
// - Expected: All XP awarded, no missing increments
//
// EdgeTC3: Network Failure (AI Coach)
// - Disable internet
// - Tap "Get Another Tip"
// - Expected: Falls back to LocalAIStub (tip still appears)
//
// EdgeTC4: Very High XP (Stress Test)
// - Manually set TotalXp to 10000 (edit JSON directly)
// - Expected: Level = floor(sqrt(10000/100)) = 10
// - Level UI updates correctly
//
// ============================================
//
// ACCEPTANCE & SIGN-OFF
// ====================
//
// All features pass when:
// ? Unit tests pass (XPServiceTests, AchievementServiceTests, AIServiceTests, PersistenceHelperTests)
// ? Manual QA cases pass on Windows (target minimum)
// ? Manual QA cases pass on Android (primary platform)
// ? No regressions in existing features (tasks, habits, clock, daily reset)
// ? Code builds without warnings
// ? App starts in < 3 seconds
// ? No memory leaks (basic check via task manager)
//
// ============================================
//
// NOTES FOR TESTERS
// =================
//
// - Clear app data between feature tests to ensure clean state
// - Use Settings app (Android) or %AppData% (Windows) to reset JSON files
// - For date/time testing, use device settings to change date (don't wait 90 days!)
// - Notification testing requires Android device or emulator with Google Play services
// - Confetti may vary slightly due to random positioning—verify general effect, not pixel-perfect
// - XP and Level values are deterministic; verify exact numbers in unit tests
//
// ============================================

/**
 * UNIT TEST COMMANDS
 * 
 * Run all unit tests:
 *   dotnet test LifeDashboard.Tests/ -v normal
 * 
 * Run specific test class:
 *   dotnet test LifeDashboard.Tests/XPServiceTests.cs -v normal
 * 
 * Run with code coverage:
 *   dotnet test /p:CollectCoverage=true /p:CoverageFileName=coverage.xml
 */

/**
 * MANUAL TEST CHECKLIST
 * 
 * ? XP System
 * ? Level Up & Confetti
 * ? Streak Badges
 * ? Heatmap
 * ? AI Coach
 * ? Notifications (if supported on test device)
 * ? Achievement All-Complete
 * ? Data Persistence
 * ? Performance
 * ? Cross-platform compatibility
 */
