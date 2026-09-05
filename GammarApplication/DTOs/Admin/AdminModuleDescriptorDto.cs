namespace GammarApplication.DTOs.Admin;

public sealed record AdminEndpointDescriptorDto(
    string Label,
    string Method,
    string Endpoint,
    string Description);

public sealed record AdminModuleDescriptorDto(
    string Code,
    string Label,
    string Description,
    IReadOnlyList<AdminEndpointDescriptorDto> Endpoints);
