using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using SAV_Projekt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV_Projekt.ViewModel
{
    public class EtfDetailViewModel : ViewModelBase
    {
        public Etf Etf { get; set; }
        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }
        public ChartValues<EtfValue> ChartValues { get; set; }
        public EtfDetailViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Model.Etf>>(this, (c) => NotificationMessageReceived(c.Notification, c.Content));
        }
        private void NotificationMessageReceived(string notification, Etf content)
        {
            Etf = new Etf();
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
                            Title = content.Name,
                            
                        });
                        break;
                    }
                default: break;
            }
            RaisePropertyChanged("Formatter");
            RaisePropertyChanged("Series");
        }
    }
}
