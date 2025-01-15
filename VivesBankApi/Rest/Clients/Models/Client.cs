using System.ComponentModel.DataAnnotations.Schema;

namespace VivesBankApi.Rest.Clients.Models;
[Table("Clients")]
public class Client
{
    public String Id { get; set; }
    public String UserId { get; set; }
    public String FullName { get; set; }
    public String Photo { get; set; }
    public String PhotoDni { get; set; }
    public List<String> cuentasIds { get; set; }
    public String role { get; set; }
    
}