using System;
using System.Collections.Generic;

namespace FinanceApp.Models
{
    public class User
    {
        public int id_user { get; set; }

        public string ds_email { get; set; } = string.Empty;

        public string hs_password { get; set; } = string.Empty;

        public DateTime dt_created { get; set; } = DateTime.Now;

        public ICollection<Lancamento> lancamentos { get; set; }
    }
}
