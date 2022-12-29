using Content.Domain;
using Content.Domain.Dtos;

using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Content.Tests;

[TestClass]
public class PostTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Create()
    {
        // Arrange
        var data = new PostDto
        {
            Title = "Test Title",
            Body = "This is just a test"
        };

        // Act
        var outcome = helper.PostService.Create(data, helper.User).TryRunTask(out var post, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to create post");
        Assert.AreNotEqual(new Guid(), post.PostId, "Failed to generate post id");
    }

    [TestMethod]
    public void Should_Update()
    {
        // Arrange
        var post = CreatePost();
        var newData = new PostDto
        {
            Title = "Updated Title"
        };

        // Act
        var outcome = helper.PostService.Update(post.PostId, newData).TryRunTask(out var updatedPost, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to create post");
        Assert.AreEqual(newData.Title, updatedPost.Title, "Failed to update title");
    }

    [TestMethod]
    public void Should_Delete()
    {
        // Arrange
        var post = CreatePost();

        // Act
        var outcome = helper.PostService.Delete(post.PostId).TryRunTask(out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to delete post");
    }

    [TestMethod]
    public void Should_Not_Update()
    {
        // Arrange

        // Act
        var ex = Assert.ThrowsExceptionAsync<SiteException>(() => helper.PostService.Update(new Guid(), null)).Result;

        // Assert
        Assert.AreEqual(ErrorCode.MissingEntity, ex.ErrorCode);
    }

    private static Post CreatePost()
    {
        // Create post and add to db
        var post = helper.Context.Posts.Add(new Post(helper.User, new PostDto { Title = "Test Title", Body = "This is just a test" })).Entity;
        helper.Context.SaveChanges();

        return post;
    }
}