namespace VivesBankApi.Rest.Clients.Dto;

public class ClientPatchRequest
{
    public String? FullName { get; set; }

    public String? Address { get; set; }

    public String? Photo { get; set; }

    public String? PhotoDni { get; set; }
}