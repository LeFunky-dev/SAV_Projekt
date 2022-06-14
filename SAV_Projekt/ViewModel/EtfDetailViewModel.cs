using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using SAV_Projekt.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace SAV_Projekt.ViewModel
{
    public class EtfDetailViewModel : ViewModelBase
    {
        public Etf Etf { get; set; }
        private DateTime minDate;
        private DateTime maxDate;
        public DateTime MinDate
        {
            get { return minDate; }
            set
            {
                minDate = value;
                MinDateDouble = value.Ticks;
                RaisePropertyChanged("MinDateDouble");
                RaisePropertyChanged("MinDate");
            }
        }
        public DateTime MaxDate 
        {
            get
            {
                return maxDate;
            }
            set
            {
                maxDate = value;
                MaxDateDouble = value.Ticks;
                RaisePropertyChanged("MaxDateDouble"); 
                RaisePropertyChanged("MaxDate");
            }
        }
        public double MinDateDouble { get; set; }
        public double MaxDateDouble { get; set; }
        public ObservableCollection<ValueGrowth> ValueGrowth { get; set; }
        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }
        public ChartValues<EtfValue> ChartValues { get; set; }
        public ICommand ResetEtfDataCommand { get { return new RelayCommand(ResetDate); } }
        /// <summary>
        /// Method to handle the ResetEtfDataCommand
        /// </summary>
        private void ResetDate()
        {
            //Reset the min and max date to default
            MinDate = Etf.Values.FirstOrDefault().Date;
            MaxDate = Etf.Values.LastOrDefault().Date;
        }
        /// <summary>
        /// Constructor of the EtfDetailViewModel
        /// </summary>
        public EtfDetailViewModel()
        {
            //Register to the messenger service
            Messenger.Default.Register<NotificationMessage<Model.Etf>>(this, (c) => EtfNotification(c.Notification, c.Content));
        }
        /// <summary>
        /// Method to handle the incoming message if it is type of Etf
        /// </summary>
        /// <param name="notification">notification message</param>
        /// <param name="content">Etf to display</param>
        private void EtfNotification(string notification, Etf content)
        {
            Etf = new Etf();
            Etf = content;
            ChartValues = new ChartValues<EtfValue>();
            //config of the series
            var dayConfig = Mappers.Xy<EtfValue>()
                           .X(dayModel => dayModel.Date.Ticks)
                           .Y(dayModel => dayModel.Value);
            Series = new SeriesCollection(dayConfig);
            //Formatter to display the date in the wanted way of yyyy-MM
            Formatter = value => new DateTime((long)value).ToString("yyyy-MM");
            //Switch between different OperatingCommands
            switch ((OperatingCommandsEnum)Enum.Parse(typeof(OperatingCommandsEnum), notification))
            {
                case OperatingCommandsEnum.ShowEtfDetail:
                    {
                        //Add for every content a chart value
                        foreach (var value in content.Values)
                        {
                            ChartValues.Add(value);
                        }
                        //Add a series with the ChartValues as Values
                        Series.Add(new LineSeries()
                        {
                            Values = ChartValues,
                            Title = ""
                        });
                        break;
                    }
                default: break;
            }
            //Set min and max date
            MinDate = content.Values.FirstOrDefault().Date;
            MaxDate = content.Values.LastOrDefault().Date;
            //Calc value growth of the etf
            ValueGrowth = CalcValueGrowth(content);
            RaisePropertyChanged("Formatter");
            RaisePropertyChanged("Series");
            RaisePropertyChanged("Etf");
            RaisePropertyChanged("MinDate");
            RaisePropertyChanged("MaxDate");
            RaisePropertyChanged("ValueGrowth");
        }
        /// <summary>
        /// Method to calculate the value growth of the etf
        /// </summary>
        /// <param name="etf">Etf to calc</param>
        /// <returns>Returns an observable collection of ValueGrowth</returns>
        private ObservableCollection<ValueGrowth> CalcValueGrowth(Etf etf)
        {
            ObservableCollection<ValueGrowth> valueGrowths = new ObservableCollection<ValueGrowth>();
            var startValue = etf.Values.LastOrDefault().Date;
            var startPerformance = etf.Values.LastOrDefault().Value;
            var max = etf.Values.FirstOrDefault().Date;
            int[] vals = { 1, 4, 5, 5, 5, 10, 10, 10, 10, 10, 10, 10, 10 };
            int totalVal = 0;
            int i = 0;
            startValue = startValue.AddYears(-vals[i]);
            totalVal += vals[i];
            while (startValue > max)
            {
                foreach(var value in etf.Values)
                {
                    if(value.Date == startValue)
                    {
                        var calcPerf = ( startPerformance / value.Value) * 100 -100;
                        valueGrowths.Add(new ValueGrowth()
                        {
                            Period = totalVal > 1 ? totalVal + " Jahre" : totalVal + " Jahr",
                            Performance = calcPerf.ToString("0.00") + "%",
                            Color = calcPerf > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red),  
                        });
                    }
                }
                i++;
                totalVal += vals[i];
                startValue = startValue.AddYears(-vals[i]);
            }
            return valueGrowths;
        }
    }
}
