using System.ComponentModel.DataAnnotations;

namespace Velum.Core.Models;

public class AppSetting
{
    [Key]
    public required string Key { get; set; }
    public required string Value { get; set; }
}
