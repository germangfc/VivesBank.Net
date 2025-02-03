using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Backup;

public class BackUpRequest
{
    [Required]
    public String FilePath { get; set; }
}