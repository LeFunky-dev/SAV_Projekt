using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;

namespace SAV_Projekt.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<EtfDetailViewModel>();
            SimpleIoc.Default.Register<AddEditPortfolioViewModel>();
            SimpleIoc.Default.Register<AddTransactionViewModel>();
        }
        /// <summary>
        /// MainViewModel member for binding MainWindow to this ViewModel
        /// </summary>
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        /// <summary>
        /// EtfDetailViewmodel member for binding EtfDetailWindow to this ViewModel
        /// </summary>
        public EtfDetailViewModel EtfDetail
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EtfDetailViewModel> ();
            }
        }
        /// <summary>
        /// AddEditPortfolioViewModel member for binding AddEditPortfolioWindow to this Viewmodel
        /// </summary>
        public AddEditPortfolioViewModel AddEditPortfolio
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AddEditPortfolioViewModel>();
            }
        }
        /// <summary>
        /// AddEditPortfolioViewModel member for binding AddTransactionWindow to this Viewmodel
        /// </summary>
        public AddTransactionViewModel AddTransaction
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AddTransactionViewModel> ();
            }
        }
    }
}