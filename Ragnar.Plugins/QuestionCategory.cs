namespace Ragnar.Plugins;

/// <summary>Represents categories for plugin questions.</summary>
public enum QuestionCategory
{
    /// <summary>Relates to performance optimization issues.</summary>
    Performance,

    /// <summary>Relates to security vulnerabilities or practices.</summary>
    Security,

    /// <summary>Relates to code refactoring suggestions.</summary>
    Refactor,

    /// <summary>General-purpose category for undefined topics.</summary>
    Other,

    /// <summary>General, non-specific plugin-related questions.</summary>
    General,

    /// <summary>Relates to logging configuration or practices.</summary>
    Logging,

    /// <summary>Relates to editor settings or plugins.</summary>
    Editor,

    /// <summary>Relates to testing strategies or frameworks.</summary>
    Testing,

    /// <summary>Relates to XML documentation or processing.</summary>
    XML,

    /// <summary>Relates to modernizing legacy code patterns.</summary>
    Modernization,
    Plugin,
}