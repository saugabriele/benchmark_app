using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using DTO;
using BLL.Enums;


namespace SQLiteDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager _userManager;

        public UserController(UserManager userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserDTO userDTO)
        {
            /*
             * Registers a new user.
             * 
             * Parameters:
             *    userDTO (UserDTO): The user data for registration.
             * 
             * Returns:
             *    201 Created - If the user is successfully registered.
             *    400 Bad Request - If the username, password, or email is not valid,
             *                      or if a user with the same email already exists.
             */

            (UserEnum errorCode, UserDTO? registeredUser) = await _userManager.RegisterAsync(userDTO);

            switch (errorCode)
            {
                case (UserEnum.usernameNotValid):
                    return BadRequest("Error: Username must be 3-16 characters long and contain only letters, " +
                        "numbers, underscores (_), or hyphens (-).");
                case (UserEnum.passwordNotValid):
                    return BadRequest("Error: Password must be 8+ characters and include 1 uppercase, 1 lowercase, 1 number, " +
                        "and 1 special character (!@#$%^&*).");
                case (UserEnum.emailNotValid):
                    return BadRequest("Error: Invalid email format.");
                case (UserEnum.userWithSameMailExists):
                    return BadRequest("A user with this email already exists");
                case (UserEnum.none):
                    return CreatedAtAction(nameof(GetUserAsync), new { email = registeredUser.email }, registeredUser);
                default:
                    return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserDTO userDTO)
        {
            /*
             * Authenticates a user and returns a JWT token.
             * 
             * Parameters:
             *    userDTO (UserDTO): The user's login credentials.
             * 
             * Returns:
             *    200 OK - If login is successful, returns user data and JWT token.
             *    400 Bad Request - If the user does not exist or the credentials are invalid.
             */

            (UserEnum errorCode, UserDTO? registeredUser, string? jwtToken) = await _userManager.LoginAsync(userDTO);

            switch (errorCode)
            {
                case UserEnum.userDoesNotExistsOrCredentialsNotValid:
                    return BadRequest("A user with this email does not exists or the credentials are not correct");
                case UserEnum.none:
                    return Ok(new
                    {
                        user = registeredUser,
                        token = jwtToken
                    });
                default:
                    return BadRequest();
            }
        }

        [Authorize]
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserAsync(string email)
        {
            /*
             * Retrieves user information by email. It can be accessed
             * only by authorized users.
             * 
             * Parameters:
             *    email (string): The email of the user to retrieve.
             * 
             * Returns:
             *    200 OK - If the user is found, returns user details.
             *    404 Not Found - If the user is not found.
             */

            (UserEnum errorCode, UserDTO? user) = await _userManager.GetUserDTOAsync(email);

            switch(errorCode)
            {
                case UserEnum.userNotFound:
                    return NotFound("User not found");
                case UserEnum.none:
                    return Ok(user);
                default:
                    return BadRequest();
            }
        }

        [Authorize]
        [HttpPut("modify/{email}")]
        public async Task<IActionResult> ModifyUserAsych(string email, UserDTO userDTO)
        {
            /*
             * Modifies user information. It can be accessed only by
             * authorized users.
             * 
             * Parameters:
             *    email (string): The email of the user to modify.
             *    userDTO (UserDTO): The new user details.
             * 
             * Returns:
             *    204 No Content - If the modification is successful.
             *    400 Bad Request - If the email, username, or password is invalid,
             *                      or if the user does not exist or the 
             *                      credentials are not correct.
             */

            UserEnum errorCode = await _userManager.ModifyUserAsych(email, userDTO);

            switch (errorCode)
            {
                case UserEnum.emailNotValid:
                    return BadRequest("The email is not valid");
                case (UserEnum.userWithSameMailExists):
                    return BadRequest("A user with this email already exists");
                case UserEnum.userDoesNotExistsOrCredentialsNotValid:
                    return BadRequest("A user with this email does not exists or the credentials are not correct");
                case UserEnum.none:
                    return NoContent();
                default:
                    return BadRequest();
            }
        }
    }
}
