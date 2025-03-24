using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLiteDb.Models;
using SQLiteDb.Repositories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace SQLiteDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(AppDbContext context, UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserDTO userDTO)
        {
            //Create a dictionary of registration fields (username, password, email)
            Dictionary<string, string> fields = new Dictionary<string, string>
            {
                { "username", userDTO.username},
                { "password", userDTO.password},
                { "email", userDTO.email}
            };

            //Validate input fields using a custom validation function
            //If validation fails, return an error message
            string errorMessage = ValidateFields(fields);
            if (errorMessage != "")
            {
                return (BadRequest(errorMessage));
            }

            //Check if a user with the provided email already exists in the database
            if (await _userRepository.GetUserByEmailAsync(fields["email"]) != null)
            {
                return BadRequest("A user with this email already exists");
            }

            //Hash the password using the generated salt
            byte[] salt = GenerateSalt();
            string hashedPassword = HashPassword(fields["password"], salt);
            string base64Salt = Convert.ToBase64String(salt);

            //Create a new User object and save it in the database
            var user = new User(userDTO.username, hashedPassword, userDTO.email, base64Salt, hashedPassword);
            await _userRepository.CreateNewUserAsync(user);

            //Return a "Created" response with the newly created user information
            UserDTO registeredUser = user.UserDTOFromUser();
            return CreatedAtAction(nameof(GetUserAsync), new { email = registeredUser.email }, registeredUser);
        }


        [HttpPost ("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserDTO userDTO)
        {
            //Try to retrieve the registered user by email.
            //Return a BadRequest if the user does not exists or the credentials are not valid.
            var registeredUser = await _userRepository.GetUserByEmailAsync(userDTO.email);
            if (registeredUser == null ||await ValidateUserAsync(userDTO) == false)
            {
                return BadRequest("A user with this email does not exists or the credentials are not correct");
            }

            //Return the UserDTO object of the logged in user
            return Ok(registeredUser.UserDTOFromUser());
        }


        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserAsync(string email)
        {
            //Return the user object as a response with HTTP status 200 OK.
            var user = await _userRepository.GetUserByEmailAsync(email);
            return Ok(user);
        }

        private async Task<bool> ValidateUserAsync(UserDTO userDTO)
        {
            //Retrieve the registered user and its corresponding hashed password.
            User? registeredUser = await _userRepository.GetUserByEmailAsync(userDTO.email);
            string hashedPassword = registeredUser.hashedPassword;
            
            //Compute the hash of the entered password with the same hash
            byte[] salt = Convert.FromBase64String(registeredUser.salt);
            byte[] enteredPassword = Encoding.UTF8.GetBytes(userDTO.password);
            string hashedEnteredPassword = HashPassword(userDTO.password, salt);

            //Compare the two hashes values for verify if the password is correct.
            if (hashedEnteredPassword == hashedPassword)
                return true;
            else
                return false;
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                //Copy the password and salt bytes into a byte array
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                //Compute the sha256 hash value of the salted password
                byte[] hashedPassword = sha256.ComputeHash(saltedPassword);

                //Copy the hashed password bytes and the salt bytes into a byte array
                byte[] hashedPasswordWithSalt = new byte[hashedPassword.Length + salt.Length];
                Buffer.BlockCopy(hashedPassword, 0, hashedPasswordWithSalt, 0, hashedPassword.Length);
                Buffer.BlockCopy(salt, 0, hashedPasswordWithSalt, hashedPassword.Length, salt.Length);

                //Convert and return the final byte array in a Base64 string
                return Convert.ToBase64String(hashedPasswordWithSalt);
            }
        }

        private byte[] GenerateSalt()
        {
            //Generate a random salt of 16 bytes
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private bool ValidateInput(string input, string pattern)
        {
            //Validate an input string against a regular expression pattern
            return Regex.IsMatch(input, pattern);
        }

        private string ValidateFields(Dictionary<string, string> fields)
        {
            //Validate user input string and returns if they are not valid an error message

            string usernamePattern = "^[a-zA-Z0-9_-]{3,16}$";
            string passwordPattern = "^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
            string emailPattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";

            if (!ValidateInput(fields["username"], usernamePattern))
                return ("Error: Username must be 3-16 characters long and contain only letters, " +
                        "numbers, underscores (_), or hyphens (-).");
            if (!ValidateInput(fields["password"], passwordPattern))
                return ("Error: Password must be 8+ characters and include 1 uppercase, 1 lowercase, 1 number, " +
                        "and 1 special character (!@#$%^&*).");
            if (!ValidateInput(fields["email"], emailPattern))
                return ("Error: Invalid email format.");

            return "";
        }
    }
}
