using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using SAV_Projekt.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SAV_Projekt.ViewModel
{
    public class AddEditPortfolioViewModel : ViewModelBase
    {
        private readonly string percentError = "Die Felder müssen insgesamt 1 ergeben!";
        private readonly string nameError = "Portfolio muss validen Namen haben!";
        public Portfolio PortfolioToCreateEdit { get; set; }
        public string PortfolioName { get; set; }
        public int EditFirstOrSecond { get; set; }
        public string PercentErrorMessage { get; set; }
        public bool SubmitEnabled { get; set; } = false;
        public Etf SelectedEtf { get; set; }
        public SeriesCollection pieSeries { get; set; }
        public ObservableCollection<Etf> AvailableEtfs { get; set; }
        public ObservableCollection<PortfolioEtf> SelectedEtfValues { get; set; }
        public ICommand SubmitPortfolioCommand { get { return new RelayCommand(SubmitPortfolio); } }
        public ICommand SelectionChangedCommand { get { return new RelayCommand(SelectionChanged); } }
        public ICommand AddEtfCommand { get { return new RelayCommand(AddEtf); } }
        public ICommand DeleteEtfCommand { get { return new RelayCommand<PortfolioEtf>(DeleteEtf); } }
        /// <summary>
        /// Method to delete an etf from the portfolio
        /// </summary>
        /// <param name="etf"></param>
        private void DeleteEtf(PortfolioEtf etf)
        {
            SelectedEtfValues.Remove(etf);
            RaisePropertyChanged("SelectedEtfValues");
        }
        /// <summary>
        /// Method to add an etf to the portfolio
        /// </summary>
        private void AddEtf()
        {
            SelectedEtfValues.Add(new PortfolioEtf()
            {
                AvailableEtfs = AvailableEtfs,
                Etf = AvailableEtfs[0],
                PercentageOfPortfolio = 0.3
            });
            LoadPieChart();
            RaisePropertyChanged("SelectedEtfValues");
            SelectionChanged();
        }
        /// <summary>
        /// Method to handle the events, when a selection change appears
        /// </summary>
        private void SelectionChanged()
        {
            double sum = 0;
            foreach(var entry in SelectedEtfValues)
            {
                sum += entry.PercentageOfPortfolio;
            }
            //Check if sum is 100%
            if(sum == 1.0 )
            {
                SubmitEnabled = true;
                PercentErrorMessage = "";
            }
            //If not display error message
            else
            {
                SubmitEnabled = false;
                PercentErrorMessage = percentError;
            }
            //Check if name is not null
            if(PortfolioName == null || PortfolioName == "")
            {
                SubmitEnabled = false;
                PercentErrorMessage += "\n" + nameError;
            }
            RaisePropertyChanged("PercentErrorMessage");
            RaisePropertyChanged("SubmitEnabled");
            LoadPieChart();

        }
        /// <summary>
        /// Method to handle the submit button command
        /// </summary>
        private void SubmitPortfolio()
        {
            PortfolioToCreateEdit.PortfolioEtfs = SelectedEtfValues;
            PortfolioToCreateEdit.Name = PortfolioName;
            Messenger.Default.Send(OperatingCommandsEnum.ClosePortfolioAddEdit);
            Messenger.Default.Send(new NotificationMessage<Portfolio>(PortfolioToCreateEdit, EditFirstOrSecond.ToString()));
        }
        /// <summary>
        /// Constructor of the AddEditPortfolioViewModel
        /// </summary>
        public AddEditPortfolioViewModel()
        {
            //Register to messages
            Messenger.Default.Register<NotificationMessage<ObservableCollection<Etf>>>(this, (c) => ETfsReceived(c.Notification, c.Content));
            Messenger.Default.Register<NotificationMessage<Portfolio>>(this, (c) => PortfolioReceived(c.Notification, c.Content));
        }
        /// <summary>
        /// Method to handle a received Portfolio
        /// </summary>
        /// <param name="notification">Notification message of the input</param>
        /// <param name="content">Portfolio that was sent</param>
        private void PortfolioReceived(string notification, Portfolio content)
        {
            if(notification == OperatingCommandsEnum.OpenPortfolioEditFirst.ToString())
            {
                EditFirstOrSecond = 1;
            }
            else if (notification == OperatingCommandsEnum.OpenPortfolioEditSecond.ToString())
            {
                EditFirstOrSecond = 2;
            }
            else
            {
                EditFirstOrSecond = 0;
            }
            PortfolioToCreateEdit = new Portfolio()
            {
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            PortfolioName = content.Name;
            AvailableEtfs = content.PortfolioEtfs.FirstOrDefault().AvailableEtfs;
            SelectedEtfValues = content.PortfolioEtfs;
            RaisePropertyChanged("PortfolioName");
            RaisePropertyChanged("SelectedEtfValues");
            LoadPieChart();
        }
        /// <summary>
        /// Method to handle the AddPortfolio Button from the MainWindow
        /// </summary>
        /// <param name="message">Message of the received object</param>
        /// <param name="EtfCollection">Etfs to choose from, when creating a new portfolio</param>
        private void ETfsReceived(string message, ObservableCollection<Etf> EtfCollection)
        {
            AvailableEtfs = EtfCollection;
            PortfolioToCreateEdit = new Portfolio()
            {
                PortfolioEtfs = new ObservableCollection<PortfolioEtf>()
            };
            SelectedEtf = EtfCollection[0];
            SelectedEtfValues = new ObservableCollection<PortfolioEtf>();
            SelectedEtfValues.Add(new PortfolioEtf()
            {
                AvailableEtfs = EtfCollection,
                Etf = EtfCollection[0],
                PercentageOfPortfolio = 0.3
            });
            LoadPieChart();
            RaisePropertyChanged("SelectedEtfValues");
        }
        /// <summary>
        /// Method to load the pie chart
        /// </summary>
        private void LoadPieChart()
        {
            pieSeries = new SeriesCollection();
            foreach(var entry in SelectedEtfValues)
            {
                pieSeries.Add(new PieSeries()
                {
                    Title = entry.Etf.Name,
                    Values = new ChartValues<double> { entry.PercentageOfPortfolio * 100 },
                    DataLabels = true,

                });
            }
            RaisePropertyChanged("pieSeries");
        }
    }
}
