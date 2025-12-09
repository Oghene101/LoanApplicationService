namespace LoanApplication.Application.Common.Filters;

public record DateRangeFilter(DateTimeOffset? StartDate, DateTimeOffset? EndDate)
{
    /// <summary>
    /// Returns:
    /// - null if both StartDate and EndDate are null
    /// - the single non-null date if one side is null
    /// - the earlier date if both are non-null (swaps if StartDate > EndDate)
    /// </summary>
    public DateTimeOffset? EffectiveStartDate
    {
        get
        {
            if (StartDate is null && EndDate is null)
                return null;
            if (StartDate is null)
                return EndDate;
            if (EndDate is null)
                return StartDate;
            // both non-null: pick the smaller
            return StartDate <= EndDate ? StartDate : EndDate;
        }
    }

    /// <summary>
    /// Returns:
    /// - null if both StartDate and EndDate are null
    /// - the single non-null date if one side is null
    /// - the later date if both are non-null (swaps if EndDate < StartDate)
    /// </summary>
    public DateTimeOffset? EffectiveEndDate
    {
        get
        {
            if (StartDate is null && EndDate is null)
                return null;
            if (EndDate is null)
                return StartDate;
            if (StartDate is null)
                return EndDate;
            // both non-null: pick the larger
            return EndDate >= StartDate ? EndDate : StartDate;
        }
    }
}