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
        public ObservableCollection<Etf> AvailableEtfs { get; set; }
        public ObservableCollection<PortfolioEtf> SelectedEtfValues { get; set; }

        public ICommand SubmitPortfolioCommand { get { return new RelayCommand(SubmitPortfolio); } }
        public ICommand SelectionChangedCommand { get { return new RelayCommand(SelectionChanged); } }
        public ICommand AddEtfCommand { get { return new RelayCommand(AddEtf); } }
        public ICommand DeleteEtfCommand { get { return new RelayCommand<PortfolioEtf>(DeleteEtf); } }

        private void DeleteEtf(PortfolioEtf etf)
        {
            SelectedEtfValues.Remove(etf);
            RaisePropertyChanged("SelectedEtfValues");
        }

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

        private void SelectionChanged()
        {
            double sum = 0;
            foreach(var entry in SelectedEtfValues)
            {
                
                sum += entry.PercentageOfPortfolio;
            }
            if(sum == 1.0 )
            {
                SubmitEnabled = true;
                PercentErrorMessage = "";
            }
            else
            {
                SubmitEnabled = false;
                PercentErrorMessage = percentError;
            }
            if(PortfolioName == null || PortfolioName == "")
            {
                SubmitEnabled = false;
                PercentErrorMessage += "\n" + nameError;
            }
            RaisePropertyChanged("PercentErrorMessage");
            RaisePropertyChanged("SubmitEnabled");
            LoadPieChart();

        }

        public SeriesCollection pieSeries { get; set; }

        private void SubmitPortfolio()
        {
            PortfolioToCreateEdit.PortfolioEtfs = SelectedEtfValues;
            PortfolioToCreateEdit.Name = PortfolioName;
            Messenger.Default.Send(OperatingCommandsEnum.ClosePortfolioAddEdit);
            Messenger.Default.Send(new NotificationMessage<Portfolio>(PortfolioToCreateEdit, EditFirstOrSecond.ToString()));
        }

        public AddEditPortfolioViewModel()
        {
            Messenger.Default.Register<NotificationMessage<ObservableCollection<Etf>>>(this, (c) => ETfsReceived(c.Notification, c.Content));
            Messenger.Default.Register<NotificationMessage<Portfolio>>(this, (c) => PortfolioReceived(c.Notification, c.Content));

        }

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
            SelectedEtfValues = content.PortfolioEtfs;
            RaisePropertyChanged("PortfolioName");
            RaisePropertyChanged("SelectedEtfValues");
            LoadPieChart();
        }

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
