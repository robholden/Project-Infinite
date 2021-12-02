using System.Net;

using Markdig;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Content.Tests;

[TestClass]
public class MarkdownTests
{
    [TestMethod]
    public void Should_Generate_Markdown()
    {
        // Arrange
        var md = "This is **Markdown**! ```<script>alert();</script>```";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        // Act
        md = WebUtility.HtmlEncode(md);
        var html = Markdown.ToHtml(md, pipeline);

        // Assert
        Assert.IsNotNull(html);
    }
}