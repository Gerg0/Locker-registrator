using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Xml.Linq;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
//using System.Xml.Linq;
//using System.ComponentModel;
//using MySql.Data.MySqlClient;

namespace SzekrenyNyilvantarto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        
        List<LockerModel> locker = new List<LockerModel>();
        List<LockerModel> lockerA = new List<LockerModel>();
        List<LockerModel> lockerB = new List<LockerModel>();
        List<LockerModel> lockerC = new List<LockerModel>();
        List<TeamModel> teams = new List<TeamModel>();
        List<StateModel> states = new List<StateModel>();
        public LockerModel Updatelocker { get; set; }

        string searcsNumber;
        string searcsEmpname;
        int? searcsTeamname;
        string searcsRoomname;
        string searcsState;
        UTF8Encoding utf8 = new UTF8Encoding();

        

        public MainWindow()
        {
            InitializeComponent();

            //minden szekrény listázása
            locker = LockerModel.Select(null, "", null, "", "");
            lockerA = LockerModel.Select(null, "", null, "A", "");
            lockerB = LockerModel.Select(null, "", null, "B", "");
            lockerC = LockerModel.Select(null, "", null, "C", "");
            DG_LockerList.ItemsSource = locker;
            DG_LockerListA.ItemsSource = lockerA;
            DG_LockerListB.ItemsSource = lockerB;
            DG_LockerListC.ItemsSource = lockerC;

            teams = TeamModel.Select();
            teams.Insert(0, new TeamModel(null, ""));//legördülő listában első opció üres
            CB_TeamSearch.ItemsSource = teams;
            CB_TeamSelect.ItemsSource = teams;
            states = StateModel.Select();
            CB_StateSelect.ItemsSource = states;
            

        }


        //keresés
        private void BTN_Search_Click(object sender, RoutedEventArgs e)
        {
            //hibaellenőrzés
            Regex regexnum = new Regex(@"[^0-9]");
            Regex regexchar = new Regex(@"[^a-zA-Zá-űÁ-Ű\s*]");
            if (regexnum.IsMatch(TB_number.Text))
            {
                MessageBox.Show("A törzsszám nem tartalmazhat betűket vagy különleges karaktert!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (regexchar.IsMatch(TB_empname.Text))
            {
                MessageBox.Show("A dolgozó neve nem tartalmazhat számokat vagy különleges karaktert!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TB_number.Text.Count() > 5)
            {
                MessageBox.Show("A dolgozó törzsszáma maximum 5 számjegyből állhat!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            searcsEmpname = TB_empname.Text;
            searcsNumber = TB_number.Text;
            searcsTeamname = (int?)CB_TeamSearch.SelectedValue;
            searcsRoomname = "";
            
            Refresh();
        }

        private void Refresh()
        {
            locker = LockerModel.Select(searcsNumber, searcsEmpname, searcsTeamname, "", searcsState);
            DG_LockerList.ItemsSource = locker;
            lockerA = LockerModel.Select(searcsNumber, searcsEmpname, searcsTeamname, "A", searcsState);
            DG_LockerListA.ItemsSource = lockerA;
            lockerB = LockerModel.Select(searcsNumber, searcsEmpname, searcsTeamname, "B", searcsState);
            DG_LockerListB.ItemsSource = lockerB;
            lockerC = LockerModel.Select(searcsNumber, searcsEmpname, searcsTeamname, "C", searcsState);
            DG_LockerListC.ItemsSource = lockerC;
        }

        //kiválasztott szekrény adatainak megjelenítése
        private void DG_LockerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectLockers(DG_LockerList, locker);
        }

        private void DG_LockerListA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectLockers(DG_LockerListA, lockerA);
        }

        private void DG_LockerListB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectLockers(DG_LockerListB, lockerB);

        }

        private void DG_LockerListC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectLockers(DG_LockerListC, lockerC);

        }

        //szekrény kiválasztás
        private void SelectLockers(DataGrid lockerRoom, List<LockerModel> locker)
        {

            int chosenLocker = lockerRoom.SelectedIndex;
            if (chosenLocker != -1)
            {
                LB_LockerNumber.Content = locker[chosenLocker].Id.ToString();
                TB_EmployeeName.Text = locker[chosenLocker].EmpName.ToString();
                TB_EmployeeNumber.Text = locker[chosenLocker].Number;
                CB_TeamSelect.SelectedValue = locker[chosenLocker].TeamID;
                CB_StateSelect.SelectedValue = locker[chosenLocker].StateId;

            }
        }
        


        //adatok módosítása
        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            Regex regexnum = new Regex(@"[^0-9]");
            Regex regexchar = new Regex(@"[^a-zA-Zá-űÁ-ŰŐ\s*]");
            


            //Ha minden mező üres akkor törölje a személyt
            if (TB_EmployeeName.Text == "" && TB_EmployeeNumber.Text == "" && (int?)CB_TeamSelect.SelectedValue == null)
            {
                LockerModel.ClearFields(int.Parse(LB_LockerNumber.Content.ToString()), (int)CB_StateSelect.SelectedValue);

                Refresh();
                return;
            }

            //Új személy felvételénél minden adat szükséges
            else if (TB_EmployeeName.Text == "" || TB_EmployeeNumber.Text == "" || (int?)CB_TeamSelect.SelectedValue == null)
            {
                MessageBox.Show("Új személy felvételéhez minden adatot meg kell adnia!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Névben nincsenek számok
            else if (regexchar.IsMatch(TB_EmployeeName.Text))
            {
                MessageBox.Show("A dolgozó neve nem tartalmazhat számokat vagy különleges karaktert!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Törzsszámban nincsenek betűk
            else if (regexnum.IsMatch(TB_EmployeeNumber.Text))
            {
                MessageBox.Show("A törzsszám nem tartalmazhat betűket vagy különleges karaktert!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Törzsszám min 5 számjegy
            else if (TB_EmployeeNumber.Text.Count() != 5)
            {
                MessageBox.Show("A dolgozó törzsszámának 5 számjegyből kell állnia!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            else
            {
                this.Updatelocker = new LockerModel()
                {
                    Id = int.Parse(LB_LockerNumber.Content.ToString()),
                    EmpName = TB_EmployeeName.Text,
                    Number = TB_EmployeeNumber.Text,
                    TeamID = (int?)CB_TeamSelect.SelectedValue,
                    StateId = (int)CB_StateSelect.SelectedValue
                };
                LockerModel.Update(this.Updatelocker);
                Refresh();
            }

            

        }

        //Adatok törlése
        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            var resoult = MessageBox.Show("Biztos törölni kívánja ezt a személyt?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resoult == MessageBoxResult.Yes)
            {
                LockerModel.Remove(int.Parse(LB_LockerNumber.Content.ToString()));
                Refresh();
            }
            else
            {
                return;
            }
        }

        //Listázás állapot szerint
        private void LB_empty_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectState("Szabad");
        }


        private void LB_occupied_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectState("Foglalt");

        }

        private void LB_waiting_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectState("Várakozó");

        }

        private void LB_damaged_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectState("Sérült");

        }

        //Mind gomb
        private void BTN_All_Click(object sender, RoutedEventArgs e)
        {
            SelectState("");
        }

        private void SelectState(string state)
        {
            searcsState = state;
            Refresh();
        } 
    }
}
