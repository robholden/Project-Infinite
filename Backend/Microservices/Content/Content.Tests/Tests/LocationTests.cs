using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Content.Tests;

[TestClass]
public class LocationTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Add_By_Coords()
    {
        // Arrange
        var lat = 47.148152777777777777777777778M;
        var lng = 10.249002777777777777777777778M;

        // Act
        var outcome = helper.LocationService.AddWithCoords(lat, lng).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add location");
        Assert.IsNotNull(result, "Location is null");
    }
}