namespace Framework2.Modules.Services;

public interface IAuditTrail
{
    void Write(string message);

    IReadOnlyList<string> Entries { get; }
}

public sealed class AuditTrail : IAuditTrail
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Write(string message)
    {
        var line = $"[{DateTime.UtcNow:O}] {message}";
        _entries.Add(line);
        Console.WriteLine(line);
    }
}

public interface IValidationService
{
    IReadOnlyList<int> ValidateScores(IEnumerable<int> scores);
}

public sealed class ValidationService : IValidationService
{
    public ValidationService(IAuditTrail auditTrail)
    {
        AuditTrail = auditTrail;
    }

    public IAuditTrail AuditTrail { get; }

    public IReadOnlyList<int> ValidateScores(IEnumerable<int> scores)
    {
        var valid = scores.Where(score => score >= 0 && score <= 100).ToList();
        var invalidCount = scores.Count() - valid.Count;
        AuditTrail.Write($"Validation completed. Valid: {valid.Count}, Invalid: {invalidCount}.");
        return valid;
    }
}

public interface IReportExporter
{
    void Export(IReadOnlyList<int> validatedScores);
}

public sealed class ReportExporter : IReportExporter
{
    private readonly IAuditTrail _auditTrail;

    public ReportExporter(IAuditTrail auditTrail)
    {
        _auditTrail = auditTrail;
    }

    public void Export(IReadOnlyList<int> validatedScores)
    {
        var avg = validatedScores.Count == 0 ? 0 : validatedScores.Average();
        Console.WriteLine("Report: " + string.Join(", ", validatedScores));
        Console.WriteLine($"Average score: {avg:F2}");
        _auditTrail.Write("Report exported to console.");
    }
}
