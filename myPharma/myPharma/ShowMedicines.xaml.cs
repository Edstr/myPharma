using myPharma.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page de base, consultez la page http://go.microsoft.com/fwlink/?LinkID=390556

namespace myPharma
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ShowMedicines : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private string day;
        private int period, minHour, maxHour;

        private ObservableCollection<Medicine> medicines; // Liste de médicaments de la période sélectionnée.

        public ShowMedicines()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Les méthodes fournies dans cette section sont utilisées simplement pour permettre
        /// NavigationHelper pour répondre aux méthodes de navigation de la page.
        /// <para>
        /// La logique spécifique à la page doit être placée dans les gestionnaires d'événements pour  
        /// <see cref="NavigationHelper.LoadState"/>
        /// et <see cref="NavigationHelper.SaveState"/>.
        /// Le paramètre de navigation est disponible dans la méthode LoadState 
        /// en plus de l'état de page conservé durant une session antérieure.
        /// </para>
        /// </summary>
        /// <param name="e">Fournit des données pour les méthodes de navigation et
        /// les gestionnaires d'événements qui ne peuvent pas annuler la requête de navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            // On récupère les information passées en paramètre lors de l'ouverture de la page.
            ParametersListMedicines param = (ParametersListMedicines)e.Parameter;
            day = param.day;
            period = param.period;
            minHour = param.minHour;
            maxHour = param.maxHour;

            TestMedicines();

            listBoxMedicines.ItemsSource = null;
            listBoxMedicines.Items.Clear();

            if (medicines.Count() > 0)
            {
                for (int i = 0; i < medicines.Count(); i++)
                {
                    Medicine medItem = medicines[i];

                    // Si le jour de prise du médicaments dans la BDD est différent du jour alors on le set à l'an 1 (sorte de reset).
                    // Cela permet de supprimer les prises du médicaments des autres jours.
                    if (medItem.taken_time.DayOfYear != DateTime.Now.DayOfYear) 
                    {
                        medItem.taken_time = new DateTime(1, 1, 1);
                    }

                    listBoxMedicines.Items.Add(medItem); // On rempli la listbox des médicmanets.
                }
            }

            listBoxMedicines.ItemsSource = medicines.OrderByDescending(i => i.medicine_time).ToList(); // Tri par l'heure de prise du médicament.
            nbMedTxtBlock.Text = medicines.Count().ToString() + " medicine(s) to take.";

            // Switch permettant de savoir quelle période est appellée pour changer le titre de la pgae.
            switch (period)
            {
                case 0:
                    titleTextBlock.Text = "Occasional medicines";
                    titleTextBlock.FontSize = 42;
                    nbMedTxtBlock.Text = medicines.Count().ToString() + " record(s).";
                    listBoxMedicines.ItemsSource = medicines.OrderByDescending(i => i.medicine_name).ToList(); // On retri par nom car pas de temps de prise pour ces médicaments.
                    break;
                case 1:
                    titleTextBlock.Text = "Morning medicine(s)";
                    break;
                case 2:
                    titleTextBlock.Text = "Midday medicine(s)";
                    break;
                case 3:
                    titleTextBlock.Text = "Evening medicine(s)";
                    break;
                case 4:
                    titleTextBlock.Text = "Night medicine(s)";
                    break;
                default:
                    break;
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Permet de récupérer les médicaments de la période choisie.
        /// Utile pour recharger la liste une fois un médicament pris.
        /// </summary>
        /// <param name="medManager"></param>
        private void TestMedicines()
        {
            MedicineManager medManager = new MedicineManager();

            if (minHour < maxHour)
            {
                medicines = medManager.GetSpecificMedicines(day, minHour, maxHour);
            }
            else if (minHour > maxHour) // Si min > max --> cas des médicaments de nuits
            {
                medicines = medManager.GetNightMedicines(day, minHour, maxHour);
            }
            else // Médicaments occasionnels
            {
                medicines = medManager.GetOccasionalMedicines();
            }
        }
        
        #region Evenements

        // Affiche le menuFlyout avec l'item 'Taken'
        private void gridMedicine_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void TakeItem_Click(object sender, RoutedEventArgs e)
        {
            int medID = (int)((MenuFlyoutItem)sender).Tag;

            Medicine med = new MedicineManager().GetMedicine(medID);

            if(med.taken_time.Year == 1) // Si l'année est égale à 1 le médicament n'a pas été pris.
            {
                DateTime date = DateTime.Now;

                MedicineManager medManager = new MedicineManager();
                medManager.updateTakenTime(medID, date); // On met à jour la dernière prise du médicament.

                TestMedicines(); // On récupère la liste des médicaments de la période.

                listBoxMedicines.ItemsSource = null;

                listBoxMedicines.ItemsSource = medicines; // On redonne la liste de médicament à la source de la ListBox
            }
            else
            {
                // TODO : faire apparaitre un toast pour indiquer que le médic' a déja été pris ?
            }
        }
        #endregion
    }
}
