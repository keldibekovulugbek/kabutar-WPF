using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kabutar_WPF.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Expiration { get; set; } = string.Empty;
    }
}
