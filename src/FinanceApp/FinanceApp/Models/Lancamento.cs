using System;

namespace FinanceApp.Models
{
    public class Lancamento
    {
        public int id_lancamento { get; set; }

        public decimal vl_valor { get; set; }

        public string tp_lancamento { get; set; } = string.Empty;

        public string ds_categoria { get; set; } = string.Empty;

        public DateTime dt_lancamento { get; set; } = DateTime.Now;

        public int fk_user { get; set; }
        public User user { get; set; }
    }
}
