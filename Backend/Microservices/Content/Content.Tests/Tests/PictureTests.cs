
using Content.Core;
using Content.Core.Queries;
using Content.Domain;

using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Content.Tests;

[TestClass]
public class PictureTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Add_Picture()
    {
        // Arrange
        var user = helper.User;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.jpg");

        // Act
        var outcome = helper.PictureService.Upload(user, filePath, string.Empty).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to upload picture");
        Assert.IsNotNull(result, ex?.Message ?? "Failed to upload picture");
        Assert.IsNull(result.errors, "Upload returned errors");
        Assert.IsNotNull(result.pictureId, "Picture returned null");
    }

    [TestMethod]
    public void Should_Update_Picture()
    {
        // Arrange
        var picture = helper.AddTestPicture();
        var name = "Updated Name";
        var tags = new string[] { "Test Tag" };

        // Act
        var outcome = helper.PictureService.Update(picture.PictureId, name, tags.AsEnumerable(), false, UserLevel.Admin).TryRunTask(out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to update picture");
    }

    [TestMethod]
    public void Should_Like_Picture()
    {
        // Arrange
        var user = helper.User;
        var picture = helper.AddTestPicture();

        // Act
        var outcome = helper.PictureService.Like(picture.PictureId, user, true).TryRunTask(out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to update picture");
        Assert.IsNotNull(helper.Context.PictureLikes.FirstOrDefault(p => p.PictureId == picture.PictureId && p.UserId == user.UserId), "Failed to like picture");
    }

    [TestMethod]
    public void Should_Get_Distance_From_Picture()
    {
        // Arrange
        var src = new Coords { Lat = 47.14815278m, Lng = 10.24900278m };
        var dest = new Coords { Lat = 47.137450m, Lng = 10.24923610m };

        // Act
        var within = src.IsWithinDistance<Coords>(50).Compile().Invoke(dest);

        // Assert
        Assert.IsTrue(within, "Distance not within 50km");
    }

    [TestMethod]
    public void Should_Seed_Picture()
    {
        // Arrange
        var picture = new Picture().Seeded();

        // Act
        var valid = picture.IsSeedValid();

        picture.UpdatedDate = DateTime.Now.AddMinutes(1);
        var invalid = picture.IsSeedValid();

        // Assert
        Assert.IsTrue(valid, "Seed is invalid when it should be valid");
        Assert.IsFalse(invalid, "Seed is valid when it should be invalid");
    }

    [TestMethod]
    public void Should_Get_Picture_Matches()
    {
        // Arrange
        helper.AddTestPicture();
        var picture2 = helper.AddTestPicture();

        // Act
        var outcome = helper.PictureQueries.Matches(picture2.PictureId).TryRunTask(out var matches, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to get matches");
        Assert.IsTrue(matches.ToList().Count > 0, "Failed to count matches");
    }

    [TestMethod]
    public void Should_Search_Picture_By_Location()
    {
        // Arrange
        var picture = helper.AddTestPicture();
        var options = new PictureQueryOptions() { Locations = new[] { picture.Location.Name } };
        var request = new PagedListRequest<PictureQueryOptions>(1, 15, OrderByDirection.Desc, options);

        // Act
        var outcome = helper.PictureQueries.Lookup(request).TryRunTask(out var results, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to lookup picture");
        Assert.IsTrue(results.TotalRows > 0, "No pictures returns from lookup");
    }
}