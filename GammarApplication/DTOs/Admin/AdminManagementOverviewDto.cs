namespace GammarApplication.DTOs.Admin;

public sealed record AdminManagementOverviewDto(
    DateTime GeneratedAtUtc,
    IReadOnlyList<AdminMetricCardDto> Cards,
    IReadOnlyList<AdminModuleDescriptorDto> Modules);
