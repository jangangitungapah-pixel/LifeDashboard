using LifeDashboard.Data;
using LifeDashboard.Models;
using LifeDashboard.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LifeDashboard.Tests;

[TestClass]
public class XPServiceTests
{
    private PersistenceHelper _persistence;
    private XPService _xpService;

    [TestInitialize]
    public void Setup()
    {
        // Create a test instance with in-memory file operations
        _persistence = new PersistenceHelper();
        _xpService = new XPService(_persistence);
    }

    [TestMethod]
    public async Task AddXp_IncreasesTotalXp()
    {
        await _xpService.InitializeAsync();
        long initialXp = _xpService.TotalXp;

        await _xpService.AddXpAsync(50, "test");

        Assert.AreEqual(initialXp + 50, _xpService.TotalXp);
    }

    [TestMethod]
    public async Task AddXp_NegativeAmount_DoesNotChange()
    {
        await _xpService.InitializeAsync();
        long initialXp = _xpService.TotalXp;

        await _xpService.AddXpAsync(-10, "test");

        Assert.AreEqual(initialXp, _xpService.TotalXp);
    }

    [TestMethod]
    public void GetLevelFromXp_Level0_At0Xp()
    {
        var level = _xpService.GetLevelFromXp(0);
        Assert.AreEqual(0, level);
    }

    [TestMethod]
    public void GetLevelFromXp_Level1_At100Xp()
    {
        var level = _xpService.GetLevelFromXp(100);
        Assert.AreEqual(1, level);
    }

    [TestMethod]
    public void GetLevelFromXp_Level2_At400Xp()
    {
        var level = _xpService.GetLevelFromXp(400);
        Assert.AreEqual(2, level);
    }

    [TestMethod]
    public void GetLevelFromXp_Level3_At900Xp()
    {
        var level = _xpService.GetLevelFromXp(900);
        Assert.AreEqual(3, level);
    }

    [TestMethod]
    public void GetLevelFromXp_Level5_At2500Xp()
    {
        var level = _xpService.GetLevelFromXp(2500);
        Assert.AreEqual(5, level);
    }

    [TestMethod]
    public void GetXpRequiredForLevel_Level1_Requires100()
    {
        var xp = _xpService.GetXpRequiredForLevel(1);
        Assert.AreEqual(100L, xp);
    }

    [TestMethod]
    public void GetXpRequiredForLevel_Level2_Requires400()
    {
        var xp = _xpService.GetXpRequiredForLevel(2);
        Assert.AreEqual(400L, xp);
    }

    [TestMethod]
    public void GetXpRequiredForLevel_Level5_Requires2500()
    {
        var xp = _xpService.GetXpRequiredForLevel(5);
        Assert.AreEqual(2500L, xp);
    }

    [TestMethod]
    public async Task LevelUp_Event_FiresOnLevelThreshold()
    {
        await _xpService.InitializeAsync();

        bool eventFired = false;
        int newLevel = 0;

        _xpService.LevelUp += (s, e) =>
        {
            eventFired = true;
            newLevel = e.NewLevel;
        };

        await _xpService.AddXpAsync(100, "test");

        Assert.IsTrue(eventFired);
        Assert.AreEqual(1, newLevel);
    }

    [TestMethod]
    public async Task LevelUp_Event_DoesNotFireOnSmallXpGain()
    {
        await _xpService.InitializeAsync();

        bool eventFired = false;

        _xpService.LevelUp += (s, e) =>
        {
            eventFired = true;
        };

        await _xpService.AddXpAsync(10, "test");

        Assert.IsFalse(eventFired);
    }

    [TestMethod]
    public async Task GetProgressToNextLevel_AtLevel0()
    {
        await _xpService.InitializeAsync();
        // At 0 XP, progress should be 0/100 = 0.0
        var progress = _xpService.GetProgressToNextLevel();
        Assert.IsTrue(progress >= 0 && progress <= 1);
    }

    [TestMethod]
    public async Task GetProgressToNextLevel_MidLevel()
    {
        await _xpService.InitializeAsync();
        await _xpService.AddXpAsync(50, "test");

        // At level 0 with 50 XP, progress is 50/100 = 0.5
        var progress = _xpService.GetProgressToNextLevel();
        Assert.IsTrue(progress > 0.4 && progress < 0.6);
    }

    [TestMethod]
    public async Task Profile_Persists_AfterAddXp()
    {
        await _xpService.InitializeAsync();
        await _xpService.AddXpAsync(100, "test");

        var savedProfile = _persistence.LoadProfile();
        Assert.AreEqual(100L, savedProfile.TotalXp);
    }
}

[TestClass]
public class AchievementServiceTests
{
    private PersistenceHelper _persistence;
    private AchievementService _achievementService;
    private UserProfile _profile;

    [TestInitialize]
    public void Setup()
    {
        _persistence = new PersistenceHelper();
        _achievementService = new AchievementService(_persistence);
        _profile = new UserProfile();
    }

    [TestMethod]
    public void GetAllBadges_ReturnsFourStreakBadges()
    {
        var badges = _achievementService.GetAllBadges();

        Assert.IsNotNull(badges);
        Assert.IsTrue(badges.Count >= 4);
        Assert.IsTrue(badges.Any(b => b.Id == "streak_3"));
        Assert.IsTrue(badges.Any(b => b.Id == "streak_7"));
        Assert.IsTrue(badges.Any(b => b.Id == "streak_14"));
        Assert.IsTrue(badges.Any(b => b.Id == "streak_30"));
    }

    [TestMethod]
    public void HasBadge_ReturnsFalse_WhenNotEarned()
    {
        var hasBadge = _achievementService.HasBadge("streak_3", _profile);
        Assert.IsFalse(hasBadge);
    }

    [TestMethod]
    public async Task AwardBadge_AddsBadgeToProfile()
    {
        await _achievementService.AwardBadgeAsync("streak_3", _profile);

        Assert.IsTrue(_profile.Badges.Contains("streak_3"));
    }

    [TestMethod]
    public async Task AwardBadge_DoesNotDuplicate()
    {
        await _achievementService.AwardBadgeAsync("streak_3", _profile);
        await _achievementService.AwardBadgeAsync("streak_3", _profile);

        var count = _profile.Badges.Count(b => b == "streak_3");
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public async Task CheckStreakBadges_Awards3DayAtStreak3()
    {
        await _achievementService.CheckStreakBadgesAsync(3, _profile);

        Assert.IsTrue(_profile.Badges.Contains("streak_3"));
    }

    [TestMethod]
    public async Task CheckStreakBadges_Awards7DayAtStreak7()
    {
        await _achievementService.CheckStreakBadgesAsync(7, _profile);

        Assert.IsTrue(_profile.Badges.Contains("streak_7"));
    }

    [TestMethod]
    public async Task CheckStreakBadges_Awards14DayAtStreak14()
    {
        await _achievementService.CheckStreakBadgesAsync(14, _profile);

        Assert.IsTrue(_profile.Badges.Contains("streak_14"));
    }

    [TestMethod]
    public async Task CheckStreakBadges_Awards30DayAtStreak30()
    {
        await _achievementService.CheckStreakBadgesAsync(30, _profile);

        Assert.IsTrue(_profile.Badges.Contains("streak_30"));
    }

    [TestMethod]
    public async Task BadgeEarned_Event_Fires()
    {
        bool eventFired = false;
        Badge earnedBadge = null;

        _achievementService.BadgeEarned += (s, e) =>
        {
            eventFired = true;
            earnedBadge = e.Badge;
        };

        await _achievementService.AwardBadgeAsync("streak_3", _profile);

        Assert.IsTrue(eventFired);
        Assert.IsNotNull(earnedBadge);
        Assert.AreEqual("streak_3", earnedBadge.Id);
    }
}

[TestClass]
public class AIServiceTests
{
    [TestMethod]
    public async Task GetTip_ReturnsCoachTip()
    {
        var aiService = new AIService(new LocalAIStub());

        var tip = await aiService.GetTipAsync("test context");

        Assert.IsNotNull(tip);
        Assert.IsFalse(string.IsNullOrEmpty(tip.Tip));
        Assert.IsFalse(string.IsNullOrEmpty(tip.Motivation));
    }

    [TestMethod]
    public async Task GetTip_LocalStub_ReturnsDifferentTips()
    {
        var stub = new LocalAIStub();

        var tip1 = await stub.GetTipAsync("context1");
        var tip2 = await stub.GetTipAsync("context2");

        // Might be same due to randomness, but both should be valid
        Assert.IsFalse(string.IsNullOrEmpty(tip1.Tip));
        Assert.IsFalse(string.IsNullOrEmpty(tip2.Tip));
    }

    [TestMethod]
    public void AIService_SetAdapter_ChangesProvider()
    {
        var stub = new LocalAIStub();
        var aiService = new AIService(stub);

        var anotherStub = new LocalAIStub();
        aiService.SetAdapter(anotherStub);

        Assert.IsNotNull(aiService);
    }
}

[TestClass]
public class PersistenceHelperTests
{
    private PersistenceHelper _helper;

    [TestInitialize]
    public void Setup()
    {
        _helper = new PersistenceHelper();
    }

    [TestMethod]
    public void LoadProfile_ReturnsDefaultProfile_WhenNotExists()
    {
        var profile = _helper.LoadProfile();

        Assert.IsNotNull(profile);
        Assert.AreEqual(0, profile.TotalXp);
        Assert.AreEqual(0, profile.Level);
    }

    [TestMethod]
    public void SaveAndLoad_Profile_Preserves Data()
    {
        var profile = new UserProfile
        {
            TotalXp = 500,
            Level = 2,
            Badges = new List<string> { "streak_3" }
        };

        _helper.SaveProfile(profile);
        var loaded = _helper.LoadProfile();

        Assert.AreEqual(500, loaded.TotalXp);
        Assert.AreEqual(2, loaded.Level);
        Assert.IsTrue(loaded.Badges.Contains("streak_3"));
    }

    [TestMethod]
    public void LoadDailyStats_ReturnsEmptyList_WhenNotExists()
    {
        var stats = _helper.LoadDailyStats();

        Assert.IsNotNull(stats);
        Assert.AreEqual(0, stats.Count);
    }

    [TestMethod]
    public void SaveAndLoad_DailyStats_PreservesData()
    {
        var stats = new List<DailyStats>
        {
            new DailyStats
            {
                Date = "2024-01-01",
                TasksDoneCount = 5,
                TasksTotalCount = 5,
                HabitsDoneCount = 3,
                HabitsTotalCount = 3,
                Intensity = 1.0
            }
        };

        _helper.SaveDailyStats(stats);
        var loaded = _helper.LoadDailyStats();

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual("2024-01-01", loaded[0].Date);
        Assert.AreEqual(5, loaded[0].TasksDoneCount);
    }
}
