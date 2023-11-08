namespace Aardwolf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataHandler dh = new dataHandler();

            dh.loadAllData(false);
            dh.parseLevelData();
        }
    }
}