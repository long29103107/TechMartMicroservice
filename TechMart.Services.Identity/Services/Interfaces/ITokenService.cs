using TechMart.Services.Identity.Models;

namespace TechMart.Services.Identity.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<bool> ValidateTokenAsync(string token);
}