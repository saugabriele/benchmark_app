using BLL.Enums;
using DAL;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL
{
    public class UserManager
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserManager(IConfiguration configuration, UserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<(UserEnum, UserDTO?)> RegisterAsync(UserDTO userDTO)
        {
            //Create a dictionary of registration fields (username, password, email)
            Dictionary<string, string> fields = CreateDictOfInputFields(userDTO.username, userDTO.password,
                                                                        userDTO.email);

            //Validate input fields using a custom validation function
            //If validation fails, return an error code
            UserEnum errorCode = ValidateFields(fields);
            if (errorCode != UserEnum.none)
            {
                return (errorCode, null);
            }

            //Check if a user with the provided email already exists in the database
            if (await _userRepository.GetUserByEmailAsync(fields["email"]) != null)
            {
                return (UserEnum.userWithSameMailExists, null);
            }

            //Hash the password using the generated salt
            byte[] salt = GenerateSalt();
            string hashedPassword = HashPassword(fields["password"], salt);
            string base64Salt = Convert.ToBase64String(salt);

            //Create a new User object and save it in the database
            await _userRepository.CreateNewUserAsync(userDTO.username, hashedPassword, userDTO.email, base64Salt);

            //Return a "Created" response with the newly created user information
            UserDTO? registeredUser = await _userRepository.GetUserByEmailAsync(userDTO.email);
            return (UserEnum.none, registeredUser);
        }

        public async Task<(UserEnum, UserDTO?, string?)> LoginAsync(UserDTO userDTO)
        {
            //Try to retrieve the registered user by email.
            //Return the corresponding error code if the user does not exists or the credentials are not valid.
            var registeredUser = await _userRepository.GetUserByEmailAndUsernameAsync(userDTO.email, userDTO.username);
            if (registeredUser == null || await ValidateUserAsync(userDTO) == false)
            {
                return (UserEnum.userDoesNotExistsOrCredentialsNotValid, null, null);
            }

            //Create the JWT token
            string jwtToken = CreateJWT(registeredUser);

            //Return the UserDTO object and the JWT token of the logged in user
            return (UserEnum.none, registeredUser, jwtToken);
        }

        public async Task<(UserEnum, UserDTO?)> GetUserDTOAsync(string email)
        {
            //Return the corresponding error code if the user is not found
            UserDTO? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return (UserEnum.userNotFound, null);
            }

            //Return the userDTO object
            return (UserEnum.none, user);
        }

        public async Task<UserEnum> ModifyUserAsych(string email, UserDTO userDTO)
        {
            if (email == null)
            {
                return UserEnum.emailNotValid;
            }

            //Check if a user with the provided email already exists in the database
            if (await _userRepository.GetUserByEmailAsync(email) != null)
            {
                return UserEnum.userWithSameMailExists;
            }

            //Try to retrieve the registered user by email.
            //Return the corresponding error code if the user does not exists or the credentials are not valid.
            var registeredUser = await _userRepository.GetUserByEmailAndUsernameAsync(userDTO.email, userDTO.username);
            if (registeredUser == null || await ValidateUserAsync(userDTO) == false)
            {
                return UserEnum.userDoesNotExistsOrCredentialsNotValid;
            }

            //Create a dictionary of registration fields (username, password, email)
            Dictionary<string, string> fields = CreateDictOfInputFields(userDTO.username, userDTO.password, email);

            //Validate input fields using a custom validation function
            //If validation fails, return an error message
            UserEnum errorCode = ValidateFields(fields);
            if (errorCode != UserEnum.none)
            {
                return errorCode;
            }

            // Update the database with the modified data
            await _userRepository.UpdateDatabaseAsync(userDTO.email, email);
            return UserEnum.none;
        }

        private async Task<bool> ValidateUserAsync(UserDTO userDTO)
        {
            //Retrieve the registered user and its corresponding hashed password.
            UserDTO? registeredUser = await _userRepository.GetUserByEmailAsync(userDTO.email);
            string hashedPassword = registeredUser.password;

            //Compute the hash of the entered password with the same salt
            byte[] salt = Convert.FromBase64String(await _userRepository.GetUserPasswordSalt(userDTO.email));
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

        private UserEnum ValidateFields(Dictionary<string, string> fields)
        {
            //Validate user input string and returns if they are not valid an error code

            string usernamePattern = "^[a-zA-Z0-9_-]{3,16}$";
            string passwordPattern = "^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
            string emailPattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";

            if (!ValidateInput(fields["username"], usernamePattern))
                return UserEnum.usernameNotValid;
            if (!ValidateInput(fields["password"], passwordPattern))
                return UserEnum.passwordNotValid;
            if (!ValidateInput(fields["email"], emailPattern))
                return UserEnum.emailNotValid;

            return UserEnum.none;
        }

        private string CreateJWT(UserDTO userDTO)
        {
            // Retrieve JWT configuration values from app settings
            var issuer = _configuration["JWTConfig:Issuer"];
            var audience = _configuration["JWTConfig:Audience"];
            var Key = Encoding.UTF8.GetBytes(_configuration["JWTConfig:Key"]);

            // Define signing credentials using HMAC SHA-512 algorithm
            var SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key),
                SecurityAlgorithms.HmacSha512Signature);

            // Define claims for the JWT token (user identity information)
            var subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userDTO.username)
            });

            // Define the token descriptor with subject, expiration, issuer, audience, and signing credentials
            var expires = DateTime.UtcNow.AddMinutes(1);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = SigningCredentials
            };

            // Create the token and return it as a string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }

        private Dictionary<string, string> CreateDictOfInputFields(string username, string password, string email)
        {
            // Creates a dictionary containing user input fields
            Dictionary<string, string> fields = new Dictionary<string, string>
            {
                { "username", username},
                { "password", password},
                { "email", email}
            };

            return fields;
        }
    }
}
