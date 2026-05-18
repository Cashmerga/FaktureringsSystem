using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Dtos.CompanyProfile;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompanyProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompanyProfileResponseDto>> GetCompanyProfile()
        {
            var profile = await _context.CompanyProfiles
                .OrderBy(p => p.Id)
                .Select(p => new CompanyProfileResponseDto
                {
                    Id = p.Id,
                    CompanyName = p.CompanyName,
                    OrganizationNumber = p.OrganizationNumber,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Street = p.Street,
                    PostalCode = p.PostalCode,
                    City = p.City,
                    Country = p.Country,
                    BankAccountNumber = p.BankAccountNumber,
                    ClearingNumber = p.ClearingNumber,
                    IBAN = p.IBAN,
                    SWIFT = p.SWIFT,
                    DefaultOCRPrefix = p.DefaultOCRPrefix,
                    DefaultVATPercentage = p.DefaultVATPercentage,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound(new { message = "Company profile not found. Please create one first." });
            }

            return Ok(profile);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CompanyProfileResponseDto>> CreateCompanyProfile(CreateCompanyProfileDto dto)
        {
            var existingProfile = await _context.CompanyProfiles.FirstOrDefaultAsync();

            if (existingProfile != null)
            {
                return Conflict(new { message = "A company profile already exists. Use PUT to update it instead." });
            }

            var newProfile = new Faktureringsys.Models.CompanyProfile
            {
                CompanyName = dto.CompanyName,
                OrganizationNumber = dto.OrganizationNumber,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Street = dto.Street,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Country = dto.Country,
                BankAccountNumber = dto.BankAccountNumber,
                ClearingNumber = dto.ClearingNumber,
                IBAN = dto.IBAN,
                SWIFT = dto.SWIFT,
                DefaultOCRPrefix = dto.DefaultOCRPrefix,
                DefaultVATPercentage = dto.DefaultVATPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.CompanyProfiles.Add(newProfile);
            await _context.SaveChangesAsync();

            var response = MapToResponseDto(newProfile);
            return CreatedAtAction(nameof(GetCompanyProfile), response);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CompanyProfileResponseDto>> UpdateOrCreateCompanyProfile(UpdateCompanyProfileDto dto)
        {
            var existingProfile = await _context.CompanyProfiles.FirstOrDefaultAsync();

            if (existingProfile == null)
            {
                var newProfile = new Faktureringsys.Models.CompanyProfile
                {
                    CompanyName = dto.CompanyName,
                    OrganizationNumber = dto.OrganizationNumber,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Street = dto.Street,
                    PostalCode = dto.PostalCode,
                    City = dto.City,
                    Country = dto.Country,
                    BankAccountNumber = dto.BankAccountNumber,
                    ClearingNumber = dto.ClearingNumber,
                    IBAN = dto.IBAN,
                    SWIFT = dto.SWIFT,
                    DefaultOCRPrefix = dto.DefaultOCRPrefix,
                    DefaultVATPercentage = dto.DefaultVATPercentage,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CompanyProfiles.Add(newProfile);
                await _context.SaveChangesAsync();

                var response = MapToResponseDto(newProfile);
                return CreatedAtAction(nameof(GetCompanyProfile), response);
            }

            existingProfile.CompanyName = dto.CompanyName;
            existingProfile.OrganizationNumber = dto.OrganizationNumber;
            existingProfile.Email = dto.Email;
            existingProfile.PhoneNumber = dto.PhoneNumber;
            existingProfile.Street = dto.Street;
            existingProfile.PostalCode = dto.PostalCode;
            existingProfile.City = dto.City;
            existingProfile.Country = dto.Country;
            existingProfile.BankAccountNumber = dto.BankAccountNumber;
            existingProfile.ClearingNumber = dto.ClearingNumber;
            existingProfile.IBAN = dto.IBAN;
            existingProfile.SWIFT = dto.SWIFT;
            existingProfile.DefaultOCRPrefix = dto.DefaultOCRPrefix;
            existingProfile.DefaultVATPercentage = dto.DefaultVATPercentage;
            existingProfile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedResponse = MapToResponseDto(existingProfile);
            return Ok(updatedResponse);
        }

        private static CompanyProfileResponseDto MapToResponseDto(Faktureringsys.Models.CompanyProfile profile)
        {
            return new CompanyProfileResponseDto
            {
                Id = profile.Id,
                CompanyName = profile.CompanyName,
                OrganizationNumber = profile.OrganizationNumber,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                Street = profile.Street,
                PostalCode = profile.PostalCode,
                City = profile.City,
                Country = profile.Country,
                BankAccountNumber = profile.BankAccountNumber,
                ClearingNumber = profile.ClearingNumber,
                IBAN = profile.IBAN,
                SWIFT = profile.SWIFT,
                DefaultOCRPrefix = profile.DefaultOCRPrefix,
                DefaultVATPercentage = profile.DefaultVATPercentage,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}
