using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SessionLogin.Models;

namespace SessionLogin.Factories
{
    public class UserFactory : AutoFactory<User>
    {
        public User UserLogin(string username, string password)
        {
            /* Finder den bruger der skal logges ind, hvor kriterierne matcher username og password */
            User usertoLogin = GetAll().Find(x => x.Username == username && x.Password == password);
            return usertoLogin;
        }
    }
}