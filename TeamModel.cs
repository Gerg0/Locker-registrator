using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace SzekrenyNyilvantarto
{
    class TeamModel
    {
        public int? Id { get; set; }
        public string TeamName { get; set; }

        public TeamModel(MySqlDataReader reader)
        {
            this.Id = Convert.ToInt32(reader["id"]);
            this.TeamName = reader["teamname"].ToString();
        }

        public TeamModel(int? id, string teamname)
        {
            this.Id = id;
            this.TeamName = teamname;
        }

        public static List<TeamModel> Select()
        {
            var list = new List<TeamModel>();
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                con.Open();
                var sql = "select id, teamname from teams order by id";
                using (var cmd = new MySqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(new TeamModel(reader));
                }
            }
            return list;
        }
    }
}
