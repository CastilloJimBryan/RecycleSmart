using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class TarjeraRFIDDAL
    {
        private readonly string _conectar;
        public TarjeraRFIDDAL(string conectar)
        {
            _conectar = conectar;
        }

        public bool ExisteUID(string uid)
        {
            using (var con = new SqlConnection(_conectar))
            using(var cmd = new SqlCommand("",con))
            {
                cmd.Parameters.AddWithValue("@UID", uid);
                con.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
