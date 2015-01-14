using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

using SQLite;
using System.Globalization;
using System.Collections.ObjectModel;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=391641

namespace myPharma
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int nbMedicineAll = 0;

        private string day;

        // Listes des médicaments par période de la journée et liste des médicaments occasionnels.
        private ObservableCollection<Medicine> morningMedicines;
        private ObservableCollection<Medicine> middayMedicines;
        private ObservableCollection<Medicine> eveningMedicines;
        private ObservableCollection<Medicine> nightMedicines;
        private ObservableCollection<Medicine> occasionalMedicines;

        // Tranches d'heures pour définir les périodes.
        private int minMorning = 6;
        private int minMidday = 11;
        private int minEvening = 18;
        private int minNight = 22;

        SQLiteConnection dbConn;
        public static string DB_PATH = Path.Combine(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MedicinesManager.sqlite")); // Nom de la base de donnée de l'application

        public MainPage()
        {
            this.InitializeComponent();

            // Contrôle si le fichier de la BDD existe., si non le créé.
            if (!CheckFileExists(DB_PATH).Result)
            {
                using (dbConn = new SQLiteConnection(DB_PATH))
                {
                    dbConn.CreateTable<Medicine>();
                }
            }

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
       
        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d’événement décrivant la manière dont l’utilisateur a accédé à cette page.
        /// Ce paramètre est généralement utilisé pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitMedicinesList();
        }
        
        /// <summary>
        /// Initialisation de la liste des médicaments et la listBox contenant ces derniers.
        /// </summary>
        private void InitMedicinesList()
        {
            ObservableCollection<Medicine> medicinesList;

            // On efface la listebox de ses items et on set la source à null car les listBox se metttent à jour quand l'ItemSource a cahngé.
            listBoxMedicines.ItemsSource = null;
            listBoxMedicines.Items.Clear();

            MedicineManager medManager = new MedicineManager();
            medicinesList = medManager.GetAllMedicines(); // Récupère tous les médicaments de la BDD via le MedicineManager.

            if (medicinesList.Count() > 0)
            {
                for (int i = 0; i < medicinesList.Count(); i++)
                {
                    Medicine medItem = medicinesList[i];

                    // Les items vont remplir les champs de l'ItemTemplate automatiquement grâce aux différents binding.
                    listBoxMedicines.Items.Add(medItem);
                }

                listBoxMedicines.ItemsSource = medicinesList.OrderByDescending(i => i.medicine_name).ToList(); // Ajout de la source triée par nom des médicaments.
            }

            this.nbMedicineAll = medManager.CountAllMedicines();

            txtNbAllMedicines.Text = nbMedicineAll.ToString() + " medicines";

            day = DateTime.Now.DayOfWeek.ToString(); // Récupère le jour de la semaine en string (en anglais).

            setNbMedicines(medManager);
        }

        /// <summary>
        /// Contrôle si un fichier existe dans le dossier local de l'application.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns> un booléén</returns>
        private async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Va remplir les listes de médicaments du jour et set les informations de l'écran.
        /// </summary>
        /// <param name="medManager"></param>
        private void setNbMedicines(MedicineManager medManager)
        {
            int nbMorning, nbMidday, nbEvening, nbNight;

            // Récupère les médicaments dans la tranche horaire spécifiée.
            morningMedicines = medManager.GetSpecificMedicines(day, minMorning, minMidday);
            middayMedicines = medManager.GetSpecificMedicines(day, minMidday, minEvening);
            eveningMedicines = medManager.GetSpecificMedicines(day, minEvening, minNight);
            nightMedicines = medManager.GetNightMedicines(day, minNight, minMorning);
            occasionalMedicines = medManager.GetOccasionalMedicines();

            nbMorning = morningMedicines.Count();
            nbEvening = eveningMedicines.Count();
            nbMidday = middayMedicines.Count();
            nbNight = nightMedicines.Count();

            txtNbMedMorning1.Text = nbMorning.ToString();
            txtNbMedMorning2.Text = nbMorning.ToString();

            txtNbMedMidday1.Text = nbMidday.ToString();
            txtNbMedMidday2.Text = nbMidday.ToString();

            txtNbMedEvening1.Text = nbEvening.ToString();
            txtNbMedEvening2.Text = nbEvening.ToString();

            txtNbMedNight1.Text = nbNight.ToString();
            txtNbMedNight2.Text = nbNight.ToString();

            txtBlockDate.Text = day + " " + DateTime.Now.Day.ToString() + " - " + (nbNight + nbMorning + nbMidday + nbEvening).ToString() + " medicines to take"; // Ecris le jour et le nombre de mécaments à prendre.

            txtNbAllMedicines.Text = medManager.CountAllMedicines().ToString() + " medicine(s)";
        }

        #region Evenements
        // Lance la page (frame) de gestion de médicament.
        private void btn_addMedicine_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ManageMedicine));
        }

        // Affiche le menuFlyout lorsqu'on clique sur un élément de la grille d'un item.
        // Idéalement, par "convention", on devrait utiliser _Holding, mais des crash survenait.
        private void gridMedicine_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement); // Affiche le menuFlyout sour l'item
        }

        // Surpprime le médicament cliqué.
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            int medID = (int)((MenuFlyoutItem)sender).Tag; // Récupère l'ID du médicament via le binding d'un tag.

            try
            {
                List<Medicine> fulllist = new List<Medicine>();
                fulllist = (List<Medicine>)listBoxMedicines.ItemsSource; // Cast de l'ObservableCollection en List.

                ObservableCollection<Medicine> fullCollection = new ObservableCollection<Medicine>(fulllist);
                fullCollection.Remove(listBoxMedicines.SelectedItem as Medicine);

                listBoxMedicines.ItemsSource = fullCollection;
            }
            catch (InvalidCastException) // Lors du premier appel l'itemSource de la listBox n'est pas de type ObservableCollection du coup il faut attraper l'exception et générer le bon type de collection.
            {
                ObservableCollection<Medicine> fullCollection = new ObservableCollection<Medicine>();

                fullCollection = (ObservableCollection<Medicine>)listBoxMedicines.ItemsSource;
                fullCollection.Remove(listBoxMedicines.SelectedItem as Medicine);  // Supprime le médicament de la collection.

                listBoxMedicines.ItemsSource = fullCollection; // On redéfini la collection et de ce fait la liste va de mettre à jour d'elle même.
            }

            MedicineManager medManager = new MedicineManager();
            medManager.DeleteMedicine(medID);

            setNbMedicines(medManager);
        }

        // Appelle la page de gestion de médicament en passant le médicament de l'item cliqué.
        private void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            int medID = (int)((MenuFlyoutItem)sender).Tag; // Récupère l'ID du médicament via le binding d'un tag.

            Medicine med = new MedicineManager().GetMedicine(medID);

            this.Frame.Navigate(typeof(ManageMedicine), med);
        }

        // Supprie tout les médicaments de la liste
        private void btnDeleteAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MedicineManager medManager = new MedicineManager();
            medManager.DeleteAllMedicines();

            setNbMedicines(medManager);

            listBoxMedicines.ItemsSource = null;
            listBoxMedicines.Items.Clear();
        }

        #region Evenements des canvas
        /// <summary>
        /// Evenements 'tapped' des 5 différents canvas appelant la page d'affichage des médicaments de la période.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 

        private void canvasMorning_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ParametersListMedicines param = new ParametersListMedicines();
            param.day = day;
            param.period = 1;
            param.minHour = minMorning;
            param.maxHour = minMidday;
            this.Frame.Navigate(typeof(ShowMedicines), param);
        }

        private void canvasMidday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ParametersListMedicines param = new ParametersListMedicines();
            param.day = day;
            param.period = 2;
            param.minHour = minMidday;
            param.maxHour = minEvening;
            this.Frame.Navigate(typeof(ShowMedicines), param);
        }

        private void canvasEvening_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ParametersListMedicines param = new ParametersListMedicines();
            param.day = day;
            param.period = 3;
            param.minHour = minEvening;
            param.maxHour = minNight;
            this.Frame.Navigate(typeof(ShowMedicines), param);
        }


        private void canvasNight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ParametersListMedicines param = new ParametersListMedicines();
            param.day = day;
            param.period = 4;
            param.minHour = minNight;
            param.maxHour = minMorning;
            this.Frame.Navigate(typeof(ShowMedicines), param);
        }

        private void canvasOccasional_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ParametersListMedicines param = new ParametersListMedicines();
            param.day = day;
            param.period = 0;
            param.minHour = 0;
            param.maxHour = 0;
            this.Frame.Navigate(typeof(ShowMedicines), param);
        }

        #endregion

#endregion
    }

    /// <summary>
    /// Classe passer en paramètre pour l'affichage des médicaments dans la page 'ShowMedicine'
    /// </summary>
    public class ParametersListMedicines
    {
        public string day { get; set; }
        // int qui sera tester pour définir la période (Matin, midi, soir, nuit ou occasionnel)
        public int period { get; set; }
        public int minHour { get; set; }
        public int maxHour { get; set; }
    }
}
