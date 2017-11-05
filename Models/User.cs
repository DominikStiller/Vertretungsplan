using System.ComponentModel.DataAnnotations;

namespace DominikStiller.VertretungsplanServer.Models
{
    public class User
    {
        public UserType Type { get; set; }

        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public enum UserType
    {
        Student, Teacher
    }
}
