using System.Collections.Generic;
using System.IO;
using System.Linq;

using DominikStiller.VertretungsplanServer.Models;

namespace DominikStiller.VertretungsplanServer.Web.Helper
{
    public class UserRepository
    {
        List<User> UserList;

        public UserRepository()
        {
            UserList = new List<User>();
        }

        public User Authenticate(string username, string password)
        {
            return UserList.FirstOrDefault(user =>
                user.Username == username
                && user.Password == password
            );
        }

        public void LoadUsers(string path, UserType userType)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                // Lines have the format username;password
                var authData = line.Split(";");
                UserList.Add(new User()
                {
                    Type = userType,
                    Username = authData[0],
                    Password = authData[1]
                });
            }
        }
    }
}
