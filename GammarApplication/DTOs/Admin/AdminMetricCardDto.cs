namespace GammarApplication.DTOs.Admin;

public sealed record AdminMetricCardDto(
    string Key,
    string Label,
    string Value,
    string Description);
