#include <DHT11.h>

int TempHumiPin = A1;
int PhotoSensorPin = A0;
int value = 0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  value = analogRead(PhotoSensorPin);
  Serial.println(value);
  delay(1000);
}
