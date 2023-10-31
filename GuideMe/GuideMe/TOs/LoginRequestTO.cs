using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.TOs
{
    public class LoginRequestTO
    {
        public LoginRequestTO()
        {
            UserName = string.Empty;
            Password = string.Empty;
        }
        public LoginRequestTO(string login, string senha)
        {
            UserName = login;
            Password = senha;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
    }
}
