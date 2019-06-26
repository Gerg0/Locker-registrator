using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
//using MySql.Data.MySqlClient;
//using System.Configuration;

namespace SzekrenyNyilvantarto
{
    class StateModel
    {
        public int Id { get; set; }
        public string State { get; set; }


        public StateModel(MySqlDataReader reader)
        {
            this.Id = Convert.ToInt32(reader["id"]);
            this.State = reader["state"].ToString();
        }

        public StateModel(int id, string state)
        {
            this.Id = id;
            this.State = state;
        }


        public static List<StateModel> Select()
        {
            var list = new List<StateModel>();
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                con.Open();
                var sql = "SELECT id, state FROM states ORDER BY id";
                using (var cmd = new MySqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(new StateModel(reader));
                }
            }
            return list;
        }

        
    }
}
