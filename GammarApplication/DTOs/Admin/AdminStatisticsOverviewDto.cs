namespace GammarApplication.DTOs.Admin;

public sealed record AdminStatisticItemDto(
    string Key,
    string Label,
    decimal Value,
    string Unit,
    string Description);

public sealed record AdminStatisticsOverviewDto(
    DateTime GeneratedAtUtc,
    IReadOnlyList<AdminStatisticItemDto> Items);
