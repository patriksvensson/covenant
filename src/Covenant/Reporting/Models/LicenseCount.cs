namespace Covenant.Reporting;

public class LicenseCount
{
    public int Count { get; private set; }
    public int Distinct { get; private set; }

    public LicenseCount()
        : this(0, 0)
    {
    }

    public LicenseCount(int count, int distinct)
    {
        Count = count;
        Distinct = distinct;
    }

    protected void AddExisting()
    {
        Count++;
    }

    protected void AddDistinct()
    {
        Count++;
        Distinct++;
    }
}
