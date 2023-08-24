namespace Covenant.Analysis;

public abstract class Analyzer : ICovenantInitializable
{
    public virtual bool Enabled { get; } = true;
    public abstract string[] Patterns { get; }

    public virtual bool ShouldTraverse(DirectoryPath path)
    {
        return true;
    }

    public virtual void Initialize(ICommandLineAugmentor cli)
    {
    }

    public virtual void BeforeAnalysis(AnalysisSettings settings)
    {
    }

    public virtual void AfterAnalysis(AnalysisSettings settings)
    {
    }

    public abstract bool CanHandle(AnalysisContext context, FilePath path);
    public abstract void Analyze(AnalysisContext context, FilePath path);
}