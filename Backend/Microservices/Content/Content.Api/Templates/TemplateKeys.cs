using System.Reflection;

using Markdig;

using Microsoft.Extensions.Caching.Memory;

namespace Content.Api.Templates;

public record TemplateKeyRecord(string Type, string Name);

public static class TemplateKeys
{
    public static TemplateKeyRecord PolicyTerms => new("policy", "terms");

    public static TemplateKeyRecord PolicyPrivacy => new("policy", "privacy");

    private static TemplateKeyRecord[] _keys { get; } = new TemplateKeyRecord[] { PolicyPrivacy, PolicyTerms };

    public static bool ValidKey(string type, string name) => _keys.Any(k => k.Name == name && k.Type == type);

    public static string GetPath(string type, string name)
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates", $"{type}.{name}.md");

    public static async Task CacheTemplates(this IMemoryCache cache)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));
        var tasks = _keys.Select(key => CacheMarkdown(key.Type, key.Name, cache));
        await Task.WhenAll(tasks);
    }

    public static async Task CacheMarkdown(string type, string name, IMemoryCache cache, string md = null)
    {
        var path = GetPath(type, name);
        md ??= await File.ReadAllTextAsync(path);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(md, pipeline);

        cache.Set($"{type}_{name}", html);
    }
}