namespace BLL.Enums
{
    public enum UserEnum
    {
        none = 0,
        usernameNotValid = 1,
        passwordNotValid = 2,
        emailNotValid = 3,
        userWithSameMailExists = 4,
        userDoesNotExistsOrCredentialsNotValid = 5,
        userNotFound = 6
    }
}
