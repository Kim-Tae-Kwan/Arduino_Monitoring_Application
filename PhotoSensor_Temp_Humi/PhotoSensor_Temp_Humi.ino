#include<DHT.h>     //DHT.h 라이브러리 추가
DHT dht(A1, DHT11); //DHT 설정 dht(핀, DHT종류)
char buf[50];
int PhotoSensorPin = A0;
int Photo=0;
int Temp=0;
int Humi=0;


void setup() {
  Serial.begin(9600); //시리얼모니터 시작
}

void loop() {
  
  Temp = dht.readTemperature();  //온도 값 정수형 변수 tem에 저장
  Humi = dht.readHumidity();     //습도 값 정수형 변수 hum에 저장
  Photo = analogRead(PhotoSensorPin);
  
  sprintf(buf,"%d:%d:%d",Temp,Humi,Photo);
  Serial.println(buf);
  delay(1000);
}
