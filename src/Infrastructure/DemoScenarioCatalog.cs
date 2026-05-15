namespace SecureCodingDemo.Infrastructure;

public sealed class DemoScenarioCatalog
{
    private readonly DocumentationCatalog _documentationCatalog;
    private readonly DemoTopicCatalog _topicCatalog;
    private readonly DemoPlaygroundCatalog _playgroundCatalog;

    public DemoScenarioCatalog(
        DocumentationCatalog documentationCatalog,
        DemoTopicCatalog topicCatalog,
        DemoPlaygroundCatalog playgroundCatalog)
    {
        _documentationCatalog = documentationCatalog;
        _topicCatalog = topicCatalog;
        _playgroundCatalog = playgroundCatalog;
    }

    public IReadOnlyList<DemoScenarioSummary> GetScenarios()
    {
        var sections = _documentationCatalog
            .GetSections()
            .ToDictionary(section => section.Slug, StringComparer.OrdinalIgnoreCase);

        var topics = _topicCatalog.GetTopics()
            .ToDictionary(topic => topic.Slug, StringComparer.OrdinalIgnoreCase);

        return _playgroundCatalog
            .GetDefinitions()
            .Where(definition => sections.ContainsKey(definition.Slug) && topics.ContainsKey(definition.Slug))
            .Select(definition =>
            {
                var section = sections[definition.Slug];
                var topic = topics[definition.Slug];

                return new DemoScenarioSummary(
                    section.Slug,
                    section.Title,
                    topic.Category,
                    topic.Summary,
                    $"/docs/{Path.GetFileName(section.Path)}",
                    definition.Unsafe.Request.Path,
                    definition.Safe.Request.Path);
            })
            .ToArray();
    }

    public DemoScenario? GetScenario(string slug)
    {
        var section = _documentationCatalog
            .GetSections()
            .FirstOrDefault(item => item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        if (section is null)
        {
            return null;
        }

        var topic = _topicCatalog.GetTopic(slug);
        var definition = _playgroundCatalog.GetDefinition(slug);
        if (topic is null || definition is null)
        {
            return null;
        }

        return new DemoScenario(
            section.Slug,
            section.Title,
            topic.Category,
            topic.Summary,
            $"/docs/{Path.GetFileName(section.Path)}",
            definition.Notes,
            definition.Unsafe,
            definition.Safe);
    }
}
