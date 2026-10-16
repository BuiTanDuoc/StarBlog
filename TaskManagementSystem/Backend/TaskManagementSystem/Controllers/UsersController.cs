using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Core.Filters;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Exceptions;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        public UsersController(IApplicationService applicationService) :
            base(applicationService)
        {
        }

        // ✅ Get user by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserReadOnlyDTO>> GetUserById(int id)
        {
            if (AppUser?.Id != id && !User.IsInRole("Admin"))
                return Forbid();

            UserReadOnlyDTO userReadOnlyDTO = await applicationService.UserService.GetUserByIdAsync(id)
                ?? throw new EntityNotFoundException("User", "User with id " + id + " not found");

            return Ok(userReadOnlyDTO);
        }

        // ✅ Register user (public)
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserReadOnlyDTO>> RegisterUser([FromBody] UserRegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var registeredUser = await applicationService.UserService.RegisterUserAsync(registerDto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = registeredUser.Id },
                registeredUser
            );
        }

        // ✅ Delete user (admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await applicationService.UserService.DeleteUserAsync(id);
            if (!result) return NotFound();

            return Ok(new { message = $"User with ID {id} was successfully deleted." });
        }

        // ✅ Update user (admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserReadOnlyDTO>> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUser = await applicationService.UserService.UpdateUserAsync(id, dto);

            return Ok(updatedUser);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserReadOnlyDTO>>> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            try
            {

                var results = await applicationService.UserService.GetUserByUsernameAsync(query);


                return Ok(results);
            }
            catch (Exception ex)
            {
                //logger.LogError($"Search error: {ex.Message}");
                return StatusCode(500, new { message = "Search failed", error = ex.Message });
            }
        }

        // ✅ Get paginated filtered users - UNIQUE ROUTE
        [HttpGet("paginated")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResult<UserReadOnlyDTO>>> GetPaginatedUsersFiltered(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? username = null,
            [FromQuery] string? email = null,
            [FromQuery] string? userRole = null)
        {
            var filters = new UserFiltersDTO
            {
                UserName = username,
                Email = email,
                UserRole = userRole
            };

            var result = await applicationService.UserService.GetPaginatedUsersFilteredAsync(pageNumber, pageSize, filters);

            return Ok(result);
        }

        // ✅ Get all users - UNIQUE ROUTE
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResult<UserReadOnlyDTO>>> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await applicationService.UserService.GetAllUsersAsync(pageNumber, pageSize);

            if (result == null || !result.Data.Any())
                return NotFound("No users found.");

            return Ok(result);
        }
    }
}