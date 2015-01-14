using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;
using System.Globalization;
using System.Collections.ObjectModel;

namespace myPharma
{
    class MedicineManager
    {
        SQLiteConnection dbConn;
        List<Medicine> myCollection;

        #region CRUD to comment

        /// <summary>
        /// Méthode d'ajout d'un nouveau médicament dans la BDD avec les paramètres reçus.
        /// </summary>
        /// <param name="_medicine_name"></param>
        /// <param name="_medicine_image"></param>
        /// <param name="_medicine_type"></param>
        /// <param name="_medicine_dose"></param>
        /// <param name="weeks"></param>
        /// <param name="_occasional"></param>
        /// <param name="_reminder"></param>
        /// <param name="_medicine_time"></param>
        public void AddMedicine(String _medicine_name, String _medicine_image, String _medicine_type, string _medicine_dose, bool[] weeks, bool _occasional, bool _reminder, TimeSpan _medicine_time)
        {
            // Création d'un nouveau médicament ...
            Medicine md = new Medicine (_medicine_name, _medicine_image,  _medicine_type,  _medicine_dose, weeks, _occasional, _reminder, _medicine_time);

            using (dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    // ... et insertion de celui-çi dans la BDD.
                    dbConn.Insert(md);
                });
            }     
        }

        /// <summary>
        /// Va récupérer un médicament via son ID et changer ses informations avec les paramètres reçus.
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_medicine_name"></param>
        /// <param name="_medicine_image"></param>
        /// <param name="_medicine_type"></param>
        /// <param name="_medicine_dose"></param>
        /// <param name="weeks"></param>
        /// <param name="_occasional"></param>
        /// <param name="_reminder"></param>
        /// <param name="_medicine_time"></param>
        public void UpdateMedicine(int _ID,String _medicine_name, String _medicine_image, String _medicine_type, string _medicine_dose, bool[] weeks, bool _occasional, bool _reminder, TimeSpan _medicine_time)
        {
            using (var dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                Medicine existingMedicine = GetMedicine(_ID);

                if (existingMedicine != null)
                {
                    // On set les nouvelles valeurs :
                    existingMedicine.medicine_name = _medicine_name;
                    existingMedicine.medicine_dose = _medicine_dose;
                    existingMedicine.medicine_image =_medicine_image;
                    existingMedicine.medicine_time = _medicine_time;
                    existingMedicine.medicine_type = _medicine_type;
                    existingMedicine.occasional = _occasional;
                    existingMedicine.reminder = _reminder;
                    existingMedicine.days_monday = weeks[0];
                    existingMedicine.days_tuesday = weeks[1];
                    existingMedicine.days_wednesday = weeks[2];
                    existingMedicine.days_thursday = weeks[3];
                    existingMedicine.days_friday = weeks[4];
                    existingMedicine.days_saturday = weeks[5];
                    existingMedicine.days_sunday = weeks[6];

                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(existingMedicine);
                    });
                }
            } 
        }

        /// <summary>
        /// Supprime un médicament récupéré via son ID
        /// </summary>
        /// <param name="Id"></param>
        public void DeleteMedicine(int Id)
        {
            using (var dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                var existingMedicine = dbConn.Query<Medicine>("select * from Medicine where ID =" + Id).FirstOrDefault();
                if (existingMedicine != null)
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(existingMedicine);
                    });
                }
            }
        }

        /// <summary>
        /// Supprime la table 'Medicine' entrièrement
        /// </summary>
        public void DeleteAllMedicines()
        {
            using (dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                dbConn.DropTable<Medicine>();
                dbConn.CreateTable<Medicine>();
                dbConn.Dispose();
                dbConn.Close();
            }
        }
        
        #endregion

        /// <summary>
        /// Met à jour la date de prise d'un médicmanet spécifié par son ID.
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_date"></param>
        public void updateTakenTime(int _ID,DateTime _date)
        {
            using (var dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                Medicine existingMedicine = GetMedicine(_ID);

                if (existingMedicine != null)
                {
                    existingMedicine.taken_time = _date; // On change la date de dernière prise du médicament par celle reçue en param'.

                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(existingMedicine);
                    });
                }
            }
        }
        
        /// <summary>
        /// Se connecte à la BDD, retrouve tout les médicaments et donne le nombre total.
        /// </summary>
        /// <returns></returns>
        public int CountAllMedicines()
        {
            using (dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                List<Medicine> myCollection = dbConn.Table<Medicine>().ToList<Medicine>();

                return myCollection.Count();
            }
        }

        /// <summary>
        /// Récupère les médicaments du jour.
        /// </summary>
        /// <param name="day"></param>
        private void GetDayMedicine(string day)
        {
            string medicine_day = "";

            // Switch pour définir quel champ utilisé dans la requête pour la BDD.
            switch (day)
	        {
                case "Monday":
                    medicine_day = "days_monday";
                    break;
                case "Tuesday":
                    medicine_day = "days_tuesday";
                    break;
                case "Wednesday":
                    medicine_day = "days_wednesday";
                    break;
                case "Thursday":
                    medicine_day = "days_thursday";
                    break;
                case "Friday":
                    medicine_day = "days_friday";
                    break;
                case "Saturday":
                    medicine_day = "days_saturday";
                    break;
                case "Sunday":
                    medicine_day = "days_sunday";
                    break;
		        default:
                    break;
	        }

            using (dbConn = new SQLiteConnection(MainPage.DB_PATH))
            {
                myCollection = dbConn.Query<Medicine>("select * from Medicine where " + medicine_day + " = 1").ToList<Medicine>();
            }
        }

        /// <summary>
        /// Récupère les médicaments en fonction de la période choisie (définie par minHour et maxHour) et en fonction du jour.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="minHour"></param>
        /// <param name="maxHour"></param>
        /// <returns></returns>
        public ObservableCollection<Medicine> GetSpecificMedicines(string day, int minHour, int maxHour)
        {
            if (myCollection == null)
            {
                GetDayMedicine(day);
            }            

            List<Medicine> specificMedicines = new List<Medicine>(); 

            foreach (Medicine med in myCollection)
            {
                if (med.medicine_time.Hours >= minHour && med.medicine_time.Hours < maxHour) // Si dans la tranche d'heure on ajoute le médicaments à la liste.
                {
                    specificMedicines.Add(med);
                }
            }

            ObservableCollection<Medicine> specificCollectionMedicines = new ObservableCollection<Medicine>(specificMedicines);

            return specificCollectionMedicines;
        }

        /// <summary>
        /// Récupère les médicaments de la période 'Nuit' en fonction de la période choisie (définie par minHour et maxHour) et en fonction du jour.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="minHour"></param>
        /// <param name="maxHour"></param>
        /// <returns></returns>
        public ObservableCollection<Medicine> GetNightMedicines(string day, int minHour, int maxHour)
        {
            if (myCollection == null)
            {
                GetDayMedicine(day);
            }

            List<Medicine> specificMedicines = new List<Medicine>();

            foreach (Medicine med in myCollection)
            {
                // Si plus grand que l'heure min (par exemple 22 heures) OU plus petit que max (exemple 6h) on ajoute le médicament. 
                // Il s'agît du reste des médicaments si on veut bien.
                if (med.medicine_time.Hours >= minHour || med.medicine_time.Hours < maxHour)
                {
                    specificMedicines.Add(med);
                }
            }

            ObservableCollection<Medicine> specificCollectionMedicines = new ObservableCollection<Medicine>(specificMedicines);

            return specificCollectionMedicines;
        }

        /// <summary>
        /// Récupère les médicaments occasionnels.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Medicine> GetOccasionalMedicines()
        {
            using (dbConn = new SQLiteConnection(MainPage.DB_PATH)) // Connexion à la BDD.
            {
                myCollection = dbConn.Query<Medicine>("select * from Medicine where occasional = 1").ToList<Medicine>();

                ObservableCollection<Medicine> OccasionnalMedicines = new ObservableCollection<Medicine>(myCollection);// Conversion vers une ObservableCollection.

                return OccasionnalMedicines;
            }
        }

        /// <summary>
        /// Récupère un médicament spécifique via son ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Medicine GetMedicine(int Id)
        {
            using (var dbConn = new SQLiteConnection(MainPage.DB_PATH)) // Connexion à la BDD.
            {
                Medicine existingMedicine = dbConn.Query<Medicine>("select * from Medicine where ID =" + Id).FirstOrDefault();

                return existingMedicine;

            }
        }

        /// <summary>
        /// Récupère tous les médicaments de la BDD.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Medicine> GetAllMedicines()
        {
            using (dbConn = new SQLiteConnection(MainPage.DB_PATH)) // Connexion à la BDD.
            {
                List<Medicine> myCollection = dbConn.Table<Medicine>().ToList<Medicine>();

                ObservableCollection<Medicine> MedicinesList = new ObservableCollection<Medicine>(myCollection); // Conversion vers une ObservableCollection.

                return MedicinesList;
            }
        }

    }
}
