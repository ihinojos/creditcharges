using creditcharges.Views;

namespace creditcharges.Models
{
    public sealed class Controller
    {
        private Controller()
        {
        }

        public LogIn logIn { get; set; }
        public MainForm mainForm { get; set; }
        public AddTransaction addTransaction { get; set; }
        public EditTransaction editTransaction { get; set; }
        public AddCard addCard { get; set; }
        public DeleteCard delCard { get; set; }
        public AddUser addUser { get; set; }
        public DateRange dateRange { get; set; }
        public Employees employees { get; set; }
        public ProgressBar progressBar { get; set; }
        public static Controller controller { get; } = new Controller();
    }
}