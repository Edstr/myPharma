using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myPharma
{
    public class Medicine
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        
        public int ID
        {
            get;
            set;
        }

        public string medicine_name
        {
            get;
            set;
        }

        // Chemin pour les images ou icones des médicaments.
        public string medicine_image
        {
            get;
            set;
        }
        public string medicine_type
        {
            get;
            set;
        }
        public string medicine_dose
        {
            get;
            set;
        }

        // Ensemble de booléen indiquant quel jour le médicament est "programmé".
        public bool days_monday
        {
            get;
            set;
        }
        public bool days_tuesday
        {
            get;
            set;
        }
        public bool days_wednesday
        {
            get;
            set;
        }
        public bool days_thursday
        {
            get;
            set;
        }
        public bool days_friday
        {
            get;
            set;
        }
        public bool days_saturday
        {
            get;
            set;
        }
        public bool days_sunday
        {
            get;
            set;
        }
        // Si aucun jour n'est sélectionné alors il s'agît d'un médicament occasionnel.
        public bool occasional
        {
            get;
            set;
        }

        public bool reminder
        {
            get;
            set;
        }

        // TimeSpan indiquant quand emmetre un rappel pour prendre le médicament (inutilisable car pas accès aux reminders et alarms).
        public TimeSpan medicine_time
        {
            get;
            set;
        }

        // Méthode pour afficher l'heure de prise du médicament.(utile dans les apperçu d'item)
        public string medicine_time_string
        {
            get
            {
                if (!this.occasional)
                {
                    int hours = this.medicine_time.Hours;
                    int minutes = this.medicine_time.Minutes;
                    string hoursString, minutesString;

                    if (hours < 10)
                    {
                        hoursString = "0" + hours.ToString();
                    }
                    else
                    {
                        hoursString = hours.ToString();
                    }
                    if (minutes < 10)
                    {
                        minutesString = "0" + minutes.ToString();
                    }
                    else
                    {
                        minutesString = minutes.ToString();
                    }

                    // Ajout d'un 'vu' devant l'heure de prise du médicament.
                    return "\u231A" + hoursString + ":" + minutesString;
                }
                else { return ""; }
            }
        }

        // Date représentant la dernière prise de médicament. 
        public DateTime taken_time
        {
            get;
            set;
        }

        // Méthode pour afficher l'heure de la dernière prise du médicament.(utile dans les apperçu d'item)
        public string taken_time_string
        {
            get
            {
                if (this.taken_time.Year != 1)
                {
                    int hours = this.taken_time.Hour;
                    int minutes = this.taken_time.Minute;
                    string hoursString, minutesString;

                    if (hours < 10)
                    {
                        hoursString = "0" + hours.ToString();
                    }
                    else
                    {
                        hoursString = hours.ToString();
                    }
                    if (minutes < 10)
                    {
                        minutesString = "0" + minutes.ToString();
                    }
                    else
                    {
                        minutesString = minutes.ToString();
                    }

                    // Ajout d'une 'horloge' devant l'heure de dernière prise du médicament.
                    return "\u221A" + hoursString + ":" + minutesString;
                }
                else { return ""; }
            }

        }

        public Medicine()
        { }

        // Constructeur de médicament.
        public Medicine(String _medicine_name, String _medicine_image, String _medicine_type, string _medicine_dose, bool[] weeks, bool _occasional, bool _reminder, TimeSpan _medicine_time)
        {
            this.medicine_name = _medicine_name;
            this.medicine_image = _medicine_image;
            this.medicine_type = _medicine_type;
            this.medicine_dose = _medicine_dose;

            this.days_monday = weeks[0];
            this.days_tuesday = weeks[1];
            this.days_wednesday = weeks[2];
            this.days_thursday = weeks[3];
            this.days_friday = weeks[4];
            this.days_saturday = weeks[5];
            this.days_sunday = weeks[6];

            this.occasional = _occasional;
            this.reminder = _reminder;
            this.medicine_time = _medicine_time;

            this.taken_time = new DateTime(1, 1, 1); // Date de prise du médicament défini au 1.1.1 poru savoir qu'il n'a jamais été pris.
        }
    }
}
