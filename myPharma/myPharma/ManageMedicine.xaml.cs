using myPharma.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Pour en savoir plus sur le modèle d'élément Page de base, consultez la page http://go.microsoft.com/fwlink/?LinkID=390556

namespace myPharma
{
    /// <summary>
    /// Page d'ajout et de modification de médicaments de la BDD.
    /// </summary>
    public sealed partial class ManageMedicine : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private bool[] weeks = new bool[7];
        private List<string> typeChoice = new List<string>();
        private int medID;

        private Brush accent = Application.Current.Resources["PhoneAccentBrush"] as Brush; // Récupération de la couleur du thème de l'utilisateur.
        private Brush blackBrush;

        public ManageMedicine()
        {
            this.InitializeComponent();

            blackBrush = new SolidColorBrush(Colors.Black);

            typeChoice.Add("Oral");
            typeChoice.Add("Effervescent");
            typeChoice.Add("Drop");
            typeChoice.Add("Injection");
            comboBoxType.ItemsSource = typeChoice; // Ajout des choix de type de médicament.  

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // On reçoit un médicament en paramêtre lors de l'appel de la page, puis on rempli le formulaore avec les informations du médicament.
                Medicine med = (Medicine)e.Parameter;

                medID = med.ID;

                txtTitle.Text = "Update medicine"; 
                txtName.Text = med.medicine_name;

                Uri myUri = new Uri(BaseUri, med.medicine_image);
                BitmapImage bmi = new BitmapImage();
                bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmi.UriSource = myUri;
                imgMedicine.Source = bmi;

                comboBoxType.SelectedItem = med.medicine_type;
                doseValue.Text = med.medicine_dose.Split(null).First();

                weeks[0] = med.days_monday;
                weeks[1] = med.days_tuesday;
                weeks[2] = med.days_wednesday;
                weeks[3] = med.days_thursday;
                weeks[4] = med.days_friday;
                weeks[5] = med.days_saturday;
                weeks[6] = med.days_sunday;

                for (int i = 0; i < 7; i++) // Inversion de la liste, pour faire les contrôles et sélectionner les jours.
                {
                    if (weeks[i] == true)
                        weeks[i] = false;
                    else
                        weeks[i] = true;
                }

                changeSelectedDay(rectMon,0);
                changeSelectedDay(rectTue, 1);
                changeSelectedDay(rectWed, 2);
                changeSelectedDay(rectThu, 3);
                changeSelectedDay(rectFri, 4);
                changeSelectedDay(rectSat, 5);
                changeSelectedDay(rectSun, 6);

                timePicker.Time = med.medicine_time;
                switchReminder.IsOn = med.reminder;

                btnSave.Content = "Update"; // Changement du texte du bouton pour infirmer qu'il s'agît d'une mise à jour. Sert égalemen du contrôle plus tard.
            }
            catch (NullReferenceException) // Si rien n'est reçu lors de l'appel de la page, il s'agît d'un nouveau médicament.
            {

            }
        }

        /// <summary>
        /// Obtient le <see cref="NavigationHelper"/> associé à ce <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Obtient le modèle d'affichage pour ce <see cref="Page"/>.
        /// Cela peut être remplacé par un modèle d'affichage fortement typé.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="sender">
        /// La source de l'événement ; en général <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Données d'événement qui fournissent le paramètre de navigation transmis à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page et
        /// un dictionnaire d'état conservé par cette page durant une session
        /// antérieure.  L'état n'aura pas la valeur Null lors de la première visite de la page.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de
        /// suppression de la page du cache de navigation.  Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">La source de l'événement ; en général <see cref="NavigationHelper"/></param>
        /// <param name="e">Données d'événement qui fournissent un dictionnaire vide à remplir à l'aide de l'
        /// état sérialisable.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// Méthode permettant de changer l'apperçu d'un jour quand sélectionné et changer la value dans le tableau de booléen
        /// pour se souvenir de quels jours sont sélectionnés.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="day"></param>
        private void changeSelectedDay(Rectangle rect, int day)
        {
            if (weeks[day] == false)
            {
                weeks[day] = true;
                rect.Fill = accent;
            }
            else
            {
                weeks[day] = false;
                rect.Fill = blackBrush;
            }

            changeTextBlockFrequency();
        }

        /// <summary>
        /// Récupère les information du formulaire et contrôle si tout est saisi.
        /// Fait une mise à jout du médicament ou ajoute un nouveau selon le context.
        /// </summary>
        private void SaveOrUpdateMedicine()
        {
            bool _occasional, _reminder;
            string _medicine_name, _medicine_type, _medicine_image, _medicine_dose;
            bool IsInformationComplete = true;
            TimeSpan _medicine_time;

            SolidColorBrush errorBrush = new SolidColorBrush(Colors.Red);
            SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);

            // Diférents contrôles pour savoir si toutes les informations sont entrées.
            if (this.txtName.Text == "")
            {
                this.txtName.Background = errorBrush;
                IsInformationComplete = false;
            }
            if (this.comboBoxType.SelectedItem == null)
            {
                this.txtBlockType.Foreground = errorBrush;
                IsInformationComplete = false;
            }
            if (this.doseValue.Text == "")
            {
                this.txtBlockDose.Foreground = errorBrush;
                IsInformationComplete = false;
            }
            if (IsInformationComplete)
            {
                _medicine_name = this.txtName.Text;
                _medicine_type = this.comboBoxType.SelectedItem.ToString();

                _medicine_image = "";

                switch (_medicine_type)
                {
                    case "Oral":
                        _medicine_image = "Assets/pill.png";
                        break;
                    case "Effervescent":
                        _medicine_image = "Assets/effervescent.png";
                        break;
                    case "Drop":
                        _medicine_image = "Assets/drop.png";
                        break;
                    case "Injection":
                        _medicine_image = "Assets/injection.png";
                        break;
                    default:
                        break;
                }

                _medicine_dose = this.doseValue.Text + " " + this.doseName.Text;

                if (weeks.Where(c => c).Count() == 0)
                {
                    _occasional = true;
                }
                else
                {
                    _occasional = false;
                }

                _reminder = switchReminder.IsOn;

                _medicine_time = this.timePicker.Time;

                MedicineManager medManager = new MedicineManager();

                if ((string)btnSave.Content == "Update") // Contrôle s'il s'agît d'une mise à jour ou d'un nouveau médicament.
                {
                    medManager.UpdateMedicine(medID, _medicine_name, _medicine_image, _medicine_type, _medicine_dose, weeks, _occasional, _reminder, _medicine_time);
                }
                else
                {
                    medManager.AddMedicine(_medicine_name, _medicine_image, _medicine_type, _medicine_dose, weeks, _occasional, _reminder, _medicine_time);
                }

                this.Frame.Navigate(typeof(MainPage));
            }
        }

        // Changement dynamique du texte résumant les jours sélectionnés.
        private void changeTextBlockFrequency()
        {
            int daysSelected = weeks.Where(c => c).Count();
            string text;

            if (daysSelected == 0)
            {
                text = "Occasionally";
                timePicker.IsEnabled = false;
                switchReminder.IsOn = false;
                switchReminder.IsEnabled = false;

            }
            else if (daysSelected == 7)
            {
                text = "Everyday";
                timePicker.IsEnabled = true;
                switchReminder.IsEnabled = true;
            }
            else
            {
                text = daysSelected + " day(s) per week";
                timePicker.IsEnabled = true;
                switchReminder.IsEnabled = true;
            }

            txtFrequency.Text = text;
        }

        #region Evenements
        /// <summary>
        /// Changement de l'icône, de la valeur minimale et de l'unité d'un médacment en fonction du choix de type de médication.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uri myUri = new Uri(BaseUri, "Assets/pill.png");

            if(comboBoxType.SelectedItem != null)
            {
                txtBlockType.Foreground = new SolidColorBrush(Colors.White);
            }
            switch ((string)comboBoxType.SelectedItem)
            {
                case "Effervescent":
                    doseName.Text = "pill(s)";
                    doseValue.Text = "1";
                    myUri = new Uri(BaseUri, "Assets/effervescent.png");
                    break;
                case "Drop":
                    doseName.Text = "drop(s)";
                    doseValue.Text = "10";
                    myUri = new Uri(BaseUri, "Assets/drop.png");
                    break;
                case "Injection":
                    myUri = new Uri(BaseUri, "Assets/injection.png");
                    doseName.Text = "mg.";
                    doseValue.Text = "1000";
                    break;
                case "Oral":
                    myUri = new Uri(BaseUri, "Assets/pill.png");
                    doseName.Text = "mg.";
                    doseValue.Text = "1000";
                    break;
                default:
                    break;

            }

            BitmapImage bmi = new BitmapImage();
            bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmi.UriSource = myUri;
            imgMedicine.Source = bmi;
        }

        private void btnSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Appel de méthodes suivant si il s'agît d'un nouveau médicament ou d'une mise à jour d'un.

            SaveOrUpdateMedicine();            

            /*MedecineManager medManager = new MedecineManager();
            medManager.DeleteAllMedicines();*/           
        }

        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remet le fond à blanc si une information est entrée.
            if (txtName.Text != "")
            {
                txtName.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void doseValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remet le texte à blanc si une information est entrée.
            if (doseValue.Text != "")
            {
                txtBlockDose.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        #region Evénements 'tapped' des canvas des jours
        private void canMon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectMon, 0);   
        }

        private void canTue_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectTue, 1);   
        }

        private void canWed_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectWed, 2);   
        }
        private void canThu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectThu, 3); 
        }
        private void canFri_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectFri, 4);   
        }

        private void canSat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectSat, 5);   
        }

        private void canSun_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeSelectedDay(rectSun, 6);
        }
        #endregion
    #endregion
    }
}
