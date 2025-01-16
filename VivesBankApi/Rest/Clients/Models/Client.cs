﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VivesBankApi.Rest.Clients.Models;
[Table("Clients")]
public class Client
{
    [Key]
    public String Id { get; set; }
    [Required]
    public String UserId { get; set; }
    [Required]
    public String FullName { get; set; }
    [Required]
    public String Adress { get; set; }
    [Required]
    public String Photo { get; set; }
    [Required]
    public String PhotoDni { get; set; }
    [Required]
    public List<String> AccountsIds { get; set; }
    [Required]
    public String role { get; set; }
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
}