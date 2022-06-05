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

        private void ResetDate()
        {
            MinDate = Etf.Values.FirstOrDefault().Date;
            MaxDate = Etf.Values.LastOrDefault().Date;
        }

        public EtfDetailViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Model.Etf>>(this, (c) => NotificationMessageReceived(c.Notification, c.Content));
        }
        private void NotificationMessageReceived(string notification, Etf content)
        {
            Etf = new Etf();
            Etf = content;
            ChartValues = new ChartValues<EtfValue>();
            var dayConfig = Mappers.Xy<EtfValue>()
                           .X(dayModel => dayModel.Date.Ticks)
                           .Y(dayModel => dayModel.Value);
            Series = new SeriesCollection(dayConfig);
            Formatter = value => new DateTime((long)value).ToString("yyyy-MM");
            switch ((OperatingCommandsEnum)Enum.Parse(typeof(OperatingCommandsEnum), notification))
            {
                case OperatingCommandsEnum.ShowEtfDetail:
                    {
                        foreach (var value in content.Values)
                        {
                            ChartValues.Add(value);
                        }
                        Series.Add(new LineSeries()
                        {
                            Values = ChartValues,
                            Title = ""
                        });
                        break;
                    }
                default: break;
            }
            MinDate = content.Values.FirstOrDefault().Date;
            MaxDate = content.Values.LastOrDefault().Date;
            ValueGrowth = CalcValueGrowth(content);
            RaisePropertyChanged("Formatter");
            RaisePropertyChanged("Series");
            RaisePropertyChanged("Etf");
            RaisePropertyChanged("MinDate");
            RaisePropertyChanged("MaxDate");
            RaisePropertyChanged("ValueGrowth");
        }
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
                        var calcPerf = (startPerformance - value.Value);
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
            //var calcPerformance = (etf.Values.LastOrDefault().Value / startPerformance * 100) - 100;
            //valueGrowths.Add(new ValueGrowth()
            //{
            //    Period = (max.Year - etf.Values.FirstOrDefault().Date.Year).ToString() + " Jahre ",
            //    Performance = calcPerformance.ToString("0.00") + "%",
            //    Color = calcPerformance > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
            //});

            return valueGrowths;
        }
    }
}
