using FinanceApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceApp.Helpers
{
    public static class Session
    {
        public static User? UsuarioLogado { get; set; }
    }
}
