namespace GammarApplication.DTOs.Admin;

public sealed record AdminReportRowDto(
    string Label,
    string Value,
    string Description);

public sealed record AdminReportSectionDto(
    string Code,
    string Label,
    string Description,
    IReadOnlyList<AdminReportRowDto> Rows);

public sealed record AdminReportsOverviewDto(
    DateTime GeneratedAtUtc,
    IReadOnlyList<AdminMetricCardDto> Cards,
    IReadOnlyList<AdminReportSectionDto> Sections);
