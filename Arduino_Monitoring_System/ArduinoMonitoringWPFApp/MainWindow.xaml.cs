using ArduinoMonitoringWPFApp.Base;
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;
using ArduinoMonitoringWPFApp.ViewInfo;

namespace ArduinoMonitoringWPFApp
{

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        SerialPort serial;

        string strConnString = "Data Source=localhost;Port=3306;Database=iot_sensordata;uid=root;password=mysql_p@ssw0rd";

        private short xCount = 200;
        private short maxPhotoVal = 1023;
        private int time = 0;

        List<int> Time = new List<int>(); //x
        List<int> vs = new List<int>();  //y

        List<SensorData> photoDatas = new List<SensorData>();
        public bool IsSimulation { get; set; }

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Random rand = new Random();
        public MainWindow()
        {
            InitializeComponent();
            InitialControls();
        }

        private void InitialControls()
        {
            
            foreach (var item in SerialPort.GetPortNames())
            {
                CboSerialPort.Items.Add(item);
            }
            CboSerialPort.Text = "Select Port";

            PgbPhotoRegistor.Minimum = 0;
            PgbPhotoRegistor.Maximum = maxPhotoVal;

             BtnDisconnect.IsEnabled = false;
        }

        private void CboSerialPort_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var portName = CboSerialPort.SelectedItem.ToString();
            serial = new SerialPort(portName);
            serial.DataReceived += Serial_DataReceived;

            BtnConnect.IsEnabled = true;
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string sVal = serial.ReadLine();
            this.BeginInvoke(new Action(delegate { DisplayValue(sVal); }));
        }

        private void DisplayValue(string sVal)
        {
            

            try
            {
                ushort v = ushort.Parse(sVal);
                if (v < 0 || v > maxPhotoVal)
                    return;

                SensorData data = new SensorData(DateTime.Now, v);
                photoDatas.Add(data);
                TxtSensorCount.Text = photoDatas.Count.ToString();
                PgbPhotoRegistor.Value = v;
                LblPhotoRegistor.Text = v.ToString();

                string item = $"{photoDatas.Count} {DateTime.Now.ToString("yy-MM-dd hh:mm:ss")}\t{v}";

               
                RtbLog.AppendText($"{item}\n");
                RtbLog.ScrollToEnd();
                


                vs.Add(v);
                Time.Add(time);

                linegraph.PlotY(vs);


                if (IsSimulation == false)
                {
                    BtnPortValue.Content = $"{serial.PortName}\n{sVal}";
                    InsertDataToDB(data);
                }
                else
                    BtnPortValue.Content = $"{sVal}";
                    

            }
            catch (Exception ex)
            {
                RtbLog.AppendText($"Error : {ex.Message}\n");
                RtbLog.ScrollToEnd();
            }
        }

        private void InsertDataToDB(SensorData data)
        {
            string strQuery = "INSERT INTO sensordatatbl " +
                " (Date, Value) " +
                " VALUES " +
                " (@Date, @Value) ";

            using (MySqlConnection conn = new MySqlConnection(strConnString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(strQuery, conn);
                MySqlParameter paramDate = new MySqlParameter("@Date", MySqlDbType.DateTime)
                {
                    Value = data.Date
                };
                cmd.Parameters.Add(paramDate);
                MySqlParameter paramValue = new MySqlParameter("@Value", MySqlDbType.Int32)
                {
                    Value = data.Value
                };
                cmd.Parameters.Add(paramValue);
                cmd.ExecuteNonQuery();
            }
        }

        private void BtnConnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsSimulation == true)
                System.Windows.Forms.MessageBox.Show("시뮬레이션을 중지 하세요.","알림",MessageBoxButtons.OK,MessageBoxIcon.Information);

            if (serial == null)
                System.Windows.Forms.MessageBox.Show("포트를 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (serial!=null && IsSimulation==false)
            {
                serial.Open();
                ConnTime.Text = $"연결시간 : {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}";
                BtnConnect.IsEnabled = false;
                BtnDisconnect.IsEnabled = true;
            }

        }

        private void BtnDisconnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (serial !=null && serial.IsOpen == true)
            {
                serial.Close();
                BtnConnect.IsEnabled = true;
                BtnDisconnect.IsEnabled = false;
            }
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItemStart_Click(object sender, RoutedEventArgs e)
        {
            IsSimulation = true;
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
            // serial통신 끊기
            BtnDisconnect_Click(sender, e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ushort value = (ushort)rand.Next(1, 1024);
            DisplayValue(value.ToString());
            time++;
        }
        

        private void MenuItemStop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            IsSimulation = false;

            // serial 통신 재시작
            //BtnConnect_Click(sender, e);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ViewInfo.InformationWindow informationWindow = new InformationWindow();
            informationWindow.ShowDialog();
        }
    }
}
