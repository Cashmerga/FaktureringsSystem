using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Data.Repositories
{
    internal class CustomerRepository
    {
        private readonly IConfiguration _configuration;

        public CustomerRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Connect()
        {
            var connectionString = _configuration.GetConnectionString("SqlConnection");
            // Code to initiate connection using connectionString
        }
    }
}
