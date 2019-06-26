using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
//using MySql.Data.MySqlClient;
//using System.Configuration;
//using System.Data;
//using System.Text.RegularExpressions;
//using System.Windows;


namespace SzekrenyNyilvantarto
{
    public class LockerModel
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string EmpName { get; set; }
        public int? TeamID { get; set; }
        public string TeamName { get; set; }
        public string State { get; set; }
        public string Roomname { get; set; }
        public int? EmployeeId { get; set; }
        public int? StateId { get; set; }

        public LockerModel() { }

        public LockerModel(MySqlDataReader reader)
        {
            this.Id = Convert.ToInt32(reader["id"]);
            this.Number = reader["number"].ToString();
            this.EmpName = reader["empname"].ToString();
            this.EmployeeId = reader["employee_id"] == DBNull.Value ?
                null : (int?)Convert.ToInt32(reader["employee_id"]);
            this.TeamID = reader["team_id"] == DBNull.Value ?
                null : (int?)Convert.ToInt32(reader["team_id"]);
            this.TeamName = reader["teamname"].ToString();
            this.State = reader["state"].ToString();
            this.StateId = reader["state_id"] == DBNull.Value ?
                null : (int?)Convert.ToInt32(reader["state_id"]);
            this.Roomname = reader["roomname"].ToString();
        }

        public static List<LockerModel> Select(string number, string empname, int? team_id, string roomname, string state)
        {
           

            var list = new List<LockerModel>();
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                
                con.Open();
                var sql = "SELECT" +
                          " lockers.id," +
                           " lockers.state_id," +
                           " lockers.employee_id, "+
                          " states.state," +
                          " employees.number," +
                          " employees.empname," +
                          " teams.teamname," +
                          " employees.team_id," +
                          " lockerrooms.roomname" +
                        " FROM lockers" +
                          " LEFT OUTER JOIN employees" +
                            " ON lockers.employee_id = employees.id" +
                          " LEFT OUTER JOIN lockerrooms" +
                            " ON lockers.lockerroom_id = lockerrooms.id" +
                          " LEFT OUTER JOIN states"+
                            " ON lockers.state_id = states.id"+
                          " LEFT OUTER JOIN teams" +
                            " ON employees.team_id = teams.id"+
                            " WHERE 1 = 1";
                if (!string.IsNullOrEmpty(number))
                    sql += " AND employees.number like @number";
                if (!string.IsNullOrEmpty(empname))
                    sql += " AND employees.empname like @empname";
                if (team_id != null)
                    sql += " AND employees.team_id = @team_id";
                if (!string.IsNullOrEmpty(roomname))
                    sql += " AND lockerrooms.roomname like @roomname";
                if (!string.IsNullOrEmpty(state))
                    sql += " AND states.state like @state";

                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@number", "%" + number + "%");
                    cmd.Parameters.AddWithValue("@empname", "%" + empname + "%");
                    cmd.Parameters.AddWithValue("@team_id", team_id);
                    cmd.Parameters.AddWithValue("@roomname", roomname);
                    cmd.Parameters.AddWithValue("@state", state);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new LockerModel(reader));
                        }
                    }
                    con.Close();
                }
            }

            return list;
        }



        public static void Update(LockerModel model)
        {
            
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                con.Open();
                //megszámolja hogy hány olyan személy van akinek a neve és a száma megegyezik a beírtakkal
                var sameNumberAndEmpname = "SELECT id FROM employees WHERE number=@number AND empname=@empname";

                //beszúrja az adatokat az alkalmazottak táblába
                var insertNewEmployee = "INSERT INTO employees (number, empname, team_id)" +
                    " VALUES(@number, @empname, @team_id)";

                //megkeresi azon dolgozók azonosítóját akik már rendelkeznek ezzel a törzsszámmal
                var numberAlreadyExist = " SELECT id FROM employees WHERE number = @number";

                //frissíti az öltőszekrények táblában a dolgozók azonosítóját
                var updateLockers = " UPDATE lockers " +
                    " SET employee_id = @employee_id," +
                    " state_id = @state_id" +
                    " WHERE id = @locker_id;";

                //frissíti az dolgozók táblában a csapatok azonosítóját
                var updateTeams = "UPDATE employees SET team_id = @team_id WHERE id = @employee_id";

                //megkeresi azon szekrények azonosítóját amihez tartozik olyan dolgozó akik már rendelkeznek ezzel a törzsszámmal és névveel
                var lockersWithsameNumberAndEmpname = " SELECT lockers.id FROM lockers " +
                    " INNER JOIN employees " +
                    " ON lockers.employee_id = employees.id " +
                    " WHERE number=@number AND empname=@empname";


                //var sql7 = "UPDATE lockers SET state_id = 3 WHERE id = @locker_id ";
                var setStateToWaiting = "UPDATE lockers SET state_id = 3 WHERE employee_id = @employee_id ";

                int duplicateId;
                using (var cmd = new MySqlCommand(lockersWithsameNumberAndEmpname, con))
                {
                    cmd.Parameters.AddWithValue("@number", model.Number);
                    cmd.Parameters.AddWithValue("@empname", model.EmpName);
                    duplicateId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int duplicateNumber;
                using (var cmd = new MySqlCommand(numberAlreadyExist, con))
                {
                    cmd.Parameters.AddWithValue("@number", model.Number);
                    duplicateNumber = Convert.ToInt32(cmd.ExecuteScalar());
                }

                bool employeeExists = false;
                int empId = 0;
                using (var cmd = new MySqlCommand(sameNumberAndEmpname, con))
                {
                   

                    cmd.Parameters.AddWithValue("@number", model.Number);
                    cmd.Parameters.AddWithValue("@empname", model.EmpName);
                    object sqlresult=cmd.ExecuteScalar();
                    if (sqlresult!=null)
                    {
                        empId = int.Parse(cmd.ExecuteScalar().ToString());

                        employeeExists = empId != 0 ? true : false;

                    }
                    
                }

                

                if (!employeeExists && duplicateNumber==0)
                {
                    if (!string.IsNullOrWhiteSpace(model.EmpName) && !string.IsNullOrWhiteSpace(model.Number))
                    {
                        using (var cmd = new MySqlCommand(insertNewEmployee, con))
                        {
                            cmd.Parameters.AddWithValue("@team_id", model.TeamID);
                            cmd.Parameters.AddWithValue("@number", model.Number);
                            cmd.Parameters.AddWithValue("@empname", model.EmpName);
                            cmd.ExecuteNonQuery();
                        }
                        int employeeId;

                        using (var cmd = new MySqlCommand(numberAlreadyExist, con))
                        {
                            cmd.Parameters.AddWithValue("@number", model.Number);
                            employeeId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        RefreshLocker(model, con, updateLockers, employeeId);
                    }
                }
                else if(!employeeExists && duplicateNumber!=0)
                {
                        MessageBox.Show("A törzsszám egy másik személyhez tartozik!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                        return;
                }
                else
                {
                    var result = MessageBox.Show("A megadott személy már rendelkezik szekrénnyel! Költöztetni akar?", "Költöztetés", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {



                        int employeeId;

                        using (var cmd = new MySqlCommand(numberAlreadyExist, con))
                        {
                            cmd.Parameters.AddWithValue("@number", model.Number);
                            employeeId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        using (var cmd = new MySqlCommand(setStateToWaiting, con))
                        {
                            cmd.Parameters.AddWithValue("@employee_id", empId);

                            cmd.ExecuteNonQuery();
                        }

                        if (employeeExists)
                        {
                            using (var cmd = new MySqlCommand(updateTeams, con))
                            {
                                cmd.Parameters.AddWithValue("@team_id", model.TeamID);
                                cmd.Parameters.AddWithValue("@employee_id", employeeId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        RefreshLocker(model, con, updateLockers, employeeId);
                    }
                    else
                    {
                        int employeeId;

                        using (var cmd = new MySqlCommand(numberAlreadyExist, con))
                        {
                            cmd.Parameters.AddWithValue("@number", model.Number);
                            employeeId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        RefreshLocker(model, con, updateLockers, employeeId);

                        using (var cmd = new MySqlCommand(updateTeams, con))
                        {
                            cmd.Parameters.AddWithValue("@team_id", model.TeamID);
                            cmd.Parameters.AddWithValue("@employee_id", employeeId);
                            cmd.ExecuteNonQuery();
                        }
                        return;
                    }

                }

            }
             
        }

        private static void RefreshLocker(LockerModel model, MySqlConnection con, string sql4, int employeeId)
        {
            using (var cmd = new MySqlCommand(sql4, con))
            {
                cmd.Parameters.AddWithValue("@locker_id", model.Id);

                if (employeeId != 0)
                {
                    cmd.Parameters.AddWithValue("@employee_id", employeeId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@employee_id", null);
                }

                cmd.Parameters.AddWithValue("@state_id", model.StateId);

                cmd.ExecuteNonQuery();

            }
        }

        public static void ClearFields(int lockerId, int stateId)
        {
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                con.Open();
                var sql = " UPDATE lockers SET employee_id=null, state_id=@state_id WHERE id=@locker_id";
                
                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@locker_id", lockerId);
                    cmd.Parameters.AddWithValue("@state_id", stateId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Remove(int lockerId)
        {
            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                con.Open();
                var sql = " UPDATE lockers SET employee_id=null, state_id=2 WHERE id=@locker_id";

                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@locker_id", lockerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
