using Caliburn.Micro;
using System;
using System.Threading;
using System.IO.Ports;
using System.Windows;
using ArduinoMonitoringWPFApp.Base;
using System.Collections.Generic;
using LiveCharts;
using MySql.Data.MySqlClient;
using MvvmDialogs;
using System.Linq;



/*
 3. 그래프 줌 버튼
 */


namespace ArduinoMonitoringWPFApp.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        #region 속성 영역
        readonly IWindowManager windowManager; //팝업창
        readonly IDialogService dialogService;

        string strConnString = "Data Source=localhost;Port=3306;Database=iot_sensordata;uid=root;password=mysql_p@ssw0rd";

        string strQuery = "INSERT INTO sensordatatbl " +
                    " (Date, Photo, Temp, Humi) " +
                    " VALUES " +
                    " (@Date, @Photo, @Temp, @Humi) ";

        bool isSimulation;
        public bool IsSimulation 
        {
            get => isSimulation;
            set 
            {
                isSimulation = value;
                NotifyOfPropertyChange(() => CanAllValue);
                NotifyOfPropertyChange(() => CanZoom);
            }
        }
        SerialPort serial;
        private short maxPhotoVal = 1023;
        List<SensorData> photoDatas = new List<SensorData>();
        public double ValueCount { get; set; }
        public bool IsZoom { get; set; }
        public bool FirstZoomBtn { get; set; }

        bool isConn;
        public bool IsConn
        {
            get => isConn;
            set
            {
                isConn = value;
                NotifyOfPropertyChange(() => CanBtnConnect);
                NotifyOfPropertyChange(() => CanBtnDisconnect);
                NotifyOfPropertyChange(() => CanZoom);
                NotifyOfPropertyChange(() => CanAllValue);
            }
        }

        Timer timer { get; set; }
        Random rand = new Random();
        public BindableCollection<string> CboSerialPort { get; set; }

        string connTime;
        public string ConnTime
        {
            get => connTime;
            set
            {
                connTime = value;
                NotifyOfPropertyChange(() => ConnTime);
            }

        }

        string sensorCount;
        public string SensorCount
        {
            get => sensorCount;
            set
            {
                sensorCount = value;
                NotifyOfPropertyChange(() => SensorCount);
            }
        }

        ushort pgbPhoto;
        public ushort PgbPhoto
        {
            get => pgbPhoto;
            set
            {
                pgbPhoto = value;
                NotifyOfPropertyChange(() => PgbPhoto);
            }
        }

        short pgbTemp;
        public short PgbTemp
        {
            get => pgbTemp;
            set
            {
                pgbTemp = value;
                NotifyOfPropertyChange(() => PgbTemp);
            }
        }

        ushort pgbHumi;
        public ushort PgbHumi
        {
            get => pgbHumi;
            set
            {
                pgbHumi = value;
                NotifyOfPropertyChange(() => PgbHumi);
            }
        }

        bool cboEnable;
        public bool CboEnable
        {
            get => cboEnable;
            set
            {
                cboEnable = value;
                NotifyOfPropertyChange(() => CboEnable);
            }
        }

        string lblPhoto;
        public string LblPhoto
        {
            get => lblPhoto;
            set
            {
                lblPhoto = value;
                NotifyOfPropertyChange(() => LblPhoto);
            }
        }

        string lblHumi;
        public string LblHumi
        {
            get => lblHumi;
            set
            {
                lblHumi = value;
                NotifyOfPropertyChange(() => LblHumi);
            }
        }

        string lblTemp;
        public string LblTemp
        {
            get => lblTemp;
            set
            {
                lblTemp = value;
                NotifyOfPropertyChange(() => LblTemp);
            }
        }

        string rtbLog;
        public string RtbLog
        {
            get => rtbLog;
            set
            {
                rtbLog = value;
                NotifyOfPropertyChange(() => RtbLog);
            }
        }

        string btnPortValue;
        public string BtnPortValue
        {
            get => btnPortValue;
            set
            {
                btnPortValue = value;
                NotifyOfPropertyChange(() => BtnPortValue);
            }
        }

        string selectedPort;
        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                
                selectedPort = value;
                NotifyOfPropertyChange(() => SelectedPort);
                NotifyOfPropertyChange(() => CanBtnConnect);
            }
        }

        ChartValues<double> photoValues;
        public ChartValues<double> PhotoValues
        {
            get => photoValues;
            set
            {
                photoValues = value;
                NotifyOfPropertyChange(() => PhotoValues);
            }
        }

        ChartValues<double> tempValues;
        public ChartValues<double> TempValues
        {
            get => tempValues;
            set
            {
                tempValues = value;
                NotifyOfPropertyChange(() => TempValues);
            }
        }

        ChartValues<double> humiValues;
        public ChartValues<double> HumiValues
        {
            get => humiValues;
            set
            {
                humiValues = value;
                NotifyOfPropertyChange(() => HumiValues);
            }
        }


        double axisXMax;
        public double AxisXMax
        {
            get => axisXMax;
            set
            {
                axisXMax = value;
                NotifyOfPropertyChange(() => AxisXMax);
            }
        }

        double axisXMin;
        public double AxisXMin
        {
            get => axisXMin;
            set
            {
                axisXMin = value;
                NotifyOfPropertyChange(() => AxisXMin);
            }
        }
        #endregion

        #region 생성자 영역

        public MainViewModel(IWindowManager windowManager, IDialogService dialogService)
        {
            this.windowManager = windowManager;
            this.dialogService = dialogService;
            InitComboBox();
            inInitializer();
            
        }

        private void inInitializer()
        {
            ConnTime = "연결시간 :";
            PhotoValues = new ChartValues<double>();
            TempValues = new ChartValues<double>();
            HumiValues = new ChartValues<double>();
            SelectedPort = "선택";
            BtnPortValue = "PORT";
            AxisXMin = 0;
            AxisXMax = 1;
            IsZoom = false;
            FirstZoomBtn = true;
            CboEnable = true;

            PgbPhoto = 50;
            PgbTemp = 50;
            PgbHumi = 50;
        }

        private void InitComboBox()
        {
            CboSerialPort = new BindableCollection<string>();
            foreach (var item in SerialPort.GetPortNames())
            {
                CboSerialPort.Add(item);
            }
        }
        #endregion

        public void ProgramExit()
        {
            Environment.Exit(0);
        }
        

        private void DisplayValue(string sVal)
        {
            try
            {
                
                string[] datas = sVal.Split(new string[] { ":" }, StringSplitOptions.None);

                if (datas.Length < 3)
                    return;

                short temp = short.Parse(datas[0]);
                ushort humi = ushort.Parse(datas[1]);
                ushort photo = ushort.Parse(datas[2]);
                if (temp < -50 || temp > 50 || humi < 0 || humi > 100 || photo < 0 || photo > maxPhotoVal)
                    return;
                                             
                SensorData data = new SensorData(DateTime.Now,photo,temp,humi);
                photoDatas.Add(data);

                SensorCount = photoDatas.Count.ToString();

                PgbPhoto = photo;
                LblPhoto = photo.ToString();

                PgbTemp = temp;
                LblTemp = temp.ToString();

                PgbHumi = humi;
                LblHumi = humi.ToString();

                string item = $"{photoDatas.Count} {DateTime.Now.ToString("yy-MM-dd hh:mm")}\t조도:{photo}, 온도:{temp}℃, Humi:{humi}%";
                RtbLog += $"{item}\n";


                if (IsZoom==false)
                    AxisXMax = photoDatas.Count;
                PhotoValues.Add(photo);
                TempValues.Add(temp);
                HumiValues.Add(humi);



                if (IsSimulation == false )
                {
                        BtnPortValue = $"{serial.PortName}";
                }
                else
                    BtnPortValue = $"Simulation";

                InsertDataToDB(data);

            }
            catch (Exception ex)
            {
                RtbLog+=($"Error : {ex.Message}\n");
            }
        }

        private void InsertDataToDB(SensorData data)
        {
            using (MySqlConnection conn = new MySqlConnection(strConnString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(strQuery, conn);
                MySqlParameter paramDate = new MySqlParameter("@Date", MySqlDbType.DateTime)
                {
                    Value = data.Date
                };
                cmd.Parameters.Add(paramDate);

                MySqlParameter paramPhoto = new MySqlParameter("@Photo", MySqlDbType.Int32)
                {
                    Value = data.Photo
                };
                cmd.Parameters.Add(paramPhoto);

                MySqlParameter paramTemp = new MySqlParameter("@Temp", MySqlDbType.Int32)
                {
                    Value = data.Temp
                };
                cmd.Parameters.Add(paramTemp);

                MySqlParameter paramHumi = new MySqlParameter("@Humi", MySqlDbType.Int32)
                {
                    Value = data.Humi
                };
                cmd.Parameters.Add(paramHumi);

                cmd.ExecuteNonQuery();
            }
        } //DB 데이터 입력


        public bool CanBtnConnect
        {
            get =>!((SelectedPort != "선택") && (IsConn==true));
            set
            {
                NotifyOfPropertyChange(() => CanBtnDisconnect);
                NotifyOfPropertyChange(() => CanZoom);
                NotifyOfPropertyChange(() => CanAllValue);
            }
        }

        public void BtnConnect()
        {
            if (IsSimulation)
            {
                MessageBox.Show("시뮬레이션을 중지하세요.");
                return;
            }
            else if (string.IsNullOrEmpty(SelectedPort) || SelectedPort == "선택")
            {
                MessageBox.Show("포트를 선택하세요.");
                return;
            }
            else 
            {
                IsConn = true;
                CboEnable = false;
                string portNum = SelectedPort;
                serial = new SerialPort(portNum);
                serial.DataReceived += Serial_DataReceived;
                serial.Open();

                ConnTime = $"연결시간 : {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}";
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string sVal = serial.ReadLine();
            DisplayValue(sVal);
        }

        public bool CanBtnDisconnect
        {
            get=> !CanBtnConnect;
        }

        public void BtnDisconnect()
        {
            if (serial != null)
            {
                IsConn = false;
                serial.Close();
                CboEnable = true;
                SelectedPort = CboSerialPort[0];
            }
            else
                MessageBox.Show("포트를 연결하세요.");
        }

        public void SimStart()
        {
            IsSimulation = true;
            timer = new Timer(TickCallBack);
            timer.Change(1000, 1000);
        }

        private void TickCallBack(object state)
        {
            ushort photo = (ushort)rand.Next(1, 1024);
            ushort temp = (ushort)rand.Next(1, 50);
            ushort humi = (ushort)rand.Next(1, 100);
            string sVal = $"{temp}:{humi}:{photo}";
            DisplayValue(sVal);
        }

        public void SimStop()
        {
            if(IsSimulation==true)
            {
                timer.Dispose();
                IsSimulation = false;
            }
        }

        public void Info()
        {
            InfoViewModel dialogVM = new InfoViewModel();
            bool? success = windowManager.ShowDialog(dialogVM);
        }

        public bool CanZoom
        {
            get => (!CanBtnConnect) || IsSimulation;
        }

        public void Zoom()
        {

            if (FirstZoomBtn)
            {
                ValueCount = photoDatas.Count;
                FirstZoomBtn = false;
            }
                
            if((AxisXMin+5) <= AxisXMax)
            {

                AxisXMin +=2;
                AxisXMax = ValueCount - 2;
                ValueCount -=2;
                IsZoom = true;
            }
        }

        public bool CanAllValue
        {
            get => (!CanBtnConnect)|| IsSimulation;
        }
        public void AllValue()
        {
            IsZoom = false;
            AxisXMin = 0;
            AxisXMax = photoDatas.Count;
            FirstZoomBtn = true;
        }
    }
}
