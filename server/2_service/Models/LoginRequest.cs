using System.ComponentModel.DataAnnotations;

namespace _2_service.Models;

public record LoginRequest([Required][MinLength(3)]string Username, [Required][MinLength(3)]string Password);