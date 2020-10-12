using System;

namespace ArduinoMonitoringWPFApp.Base
{
    public class SensorData
    {
        

        public DateTime Date { get; set; }
        public ushort Photo { get; set; }

        public short Temp { get; set; }

        public ushort Humi { get; set; }

        //public SensorData(DateTime date, ushort value) //생성자
        //{
        //    Date = date;
        //    Value = value;
        //}
        public SensorData(DateTime date, ushort photo, short temp, ushort humi)
        {
            Date = date;
            Photo = photo;
            Temp = temp;
            Humi = humi;
        }

    }
}
