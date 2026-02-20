using System.ComponentModel.DataAnnotations;

namespace DefaultNamespace;

public sealed class AppOptions
{
    [Required] [MinLength(20)] public string DbConnectionString { get; set; } = string.Empty!;
    [Required] public string Redis { get; set; } = string.Empty!;
}