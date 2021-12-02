
using Content.Core.Queries;
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Content.Tests;

[TestClass]
public class CollectionTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Create_Collection()
    {
        // Arrange
        var user = helper.User;
        var name = "Test Collection";
        Collection collection = null;

        // Act
        var outcome = helper.CollectionService.Create(user, name).TryRunTask(out var result, out var ex);
        if (outcome)
        {
            collection = helper.CollectionQueries.Get(result).Result;
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to create collection");
        Assert.IsNotNull(collection, "Failed get collection");
        Assert.AreEqual(name, collection.Name, "Failed to set name");
    }

    [TestMethod]
    public void Should_Add_To_Collection()
    {
        // Arrange
        var collection = AddCollection();
        var picture = helper.AddTestPicture();

        // Act
        var outcome = helper.CollectionService.AddOrRemovePicture(collection.CollectionId, picture.PictureId).TryRunTask(out var ex);
        if (outcome)
        {
            collection = helper.Context.Collections.Include(c => c.Pictures).FirstOrDefault(c => c.CollectionId == collection.CollectionId);
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add to collection");
        Assert.IsTrue(collection.Pictures.Count == 1, "Failed to add picture to lookup");
    }

    [TestMethod]
    public void Should_Remove_From_Collection()
    {
        // Arrange
        var collection = AddCollection();
        var picture = helper.AddTestPicture();

        // Act
        var outcome = helper.CollectionService.AddOrRemovePicture(collection.CollectionId, picture.PictureId).TryRunTask(out var ex);
        if (outcome)
        {
            outcome = helper.CollectionService.AddOrRemovePicture(collection.CollectionId, picture.PictureId).TryRunTask(out ex);
            if (outcome)
            {
                collection = helper.Context.Collections.Include(c => c.Pictures).FirstOrDefault(c => c.CollectionId == collection.CollectionId);
            }
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to remove from collection");
        Assert.IsTrue(collection.Pictures.Count == 0, "Failed to remove picture from lookup");
    }

    [TestMethod]
    public void Should_Delete_Collection()
    {
        // Arrange
        var collection = AddCollection();

        // Act
        var outcome = helper.CollectionService.Delete(collection.CollectionId).TryRunTask(out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to create collection");
        Assert.IsNull(helper.Context.Collections.FirstOrDefault(c => c.CollectionId == collection.CollectionId), "Failed to remove collection");
    }

    [TestMethod]
    public void Should_Lookup_Collections()
    {
        // Arrange
        var userId = Guid.NewGuid();
        helper.Context.Collections.Add(new() { UserId = userId, Username = "Tester", Name = "Test" });
        helper.Context.SaveChanges();

        var options = new CollectionQueryOptions { UserId = userId };
        var request = new PagedListRequest<CollectionQueryOptions>(1, 10, OrderByDirection.Desc, options);

        // Act
        var outcome = helper.CollectionQueries.Lookup(request).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to get collections");
        Assert.AreEqual(1, result?.TotalRows, "Failed to return total rows");
    }

    private static Collection AddCollection()
    {
        var index = helper.Context.Collections.Count() + 1;
        var collection = helper.Context.Add(new Collection { UserId = helper.User.UserId, Username = helper.User.Username, Name = "Unit Test Collection " + index }).Entity;
        helper.Context.SaveChanges();

        return collection;
    }
}