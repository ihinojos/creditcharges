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
        public ChildCards childCards { get; set; }
        public Entities entities { get; set; }
        public Classes classes { get; set; }
        public Vehicles vehicles { get; set; }
        public VehicleSelect vehicleSelect { get; set; }
        public static Controller controller { get; } = new Controller();
    }
}