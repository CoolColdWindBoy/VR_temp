/*
THIS CODE IS FOR ESP8266!!!!
only for testing
this code makes reference to HadesVR-main
*/

#include <Wire.h>
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
//change these two libs for your mpu
#include "I2Cdev.h"
#include "MPU6050.h"

//what this this esp used for?
String device = "controllerLeft";

#define debug HIGH//to enable serial

//pinout, diagram at /controller/frizing/controller_bb.png
#define IIC_SDA 8
#define IIC_SCL 9
#define TrackPadX 5
#define TrackPadY 4
#define TrackPadBtn 13
#define JoyX 2
#define JoyY 3
#define JoyBtn 10
#define FingerMiddleBtn 6
#define FingerRingBtn 7
#define FingerPinkyBtn 11
#define Bat 0
#define Trigger 1
#define BBtn 12
#define ABtn 18
#define SystemBtn 19

//parameters
#define mpuAddress 0x68 //there is no point for me to define address
//                        because my mpu6050 library did it for me.
//                        you may have to do this manually.
char* ssid = "CMCC-yy2k";
char* password = "18088696966";
unsigned int UDPPort = 4210;


//ignore this part and scroll down for mpu code
unsigned long previousMillisWiFi = 0;
unsigned long intervalWiFi = 500;
WiFiUDP UDP;
char incomingPacket[255];
String sendingPacket;
boolean UDPConnected = LOW;
IPAddress serverIP;
int serverPort = 0;
unsigned long previousMillisRssi = 0;
unsigned long intervalRssi = 1000;
void UDPSend(String data){
  if(debug){
    Serial.println("UDPSend: "+data);
  }
  if(!UDPConnected){
    return;
  }
  UDP.beginPacket(serverIP, serverPort);
  char datac[data.length()+1];
  data.toCharArray(datac,data.length()+1);
  int i = 0;
  while (datac[i] != 0) UDP.write((uint8_t)datac[i++]);
  UDP.endPacket();
}




int16_t ax,ay,az,gx,gy,gz;//a is for accelleration, g is for rotation
/*---------------------------------------------------------------
this is custom mpu part, please modify setup and loop code here*/
MPU6050 mpu;
void mpuSetup(){
  Wire.begin(IIC_SDA,IIC_SCL);//problematic
  mpu.initialize();
  while(!mpu.testConnection()){
    UDPSend("[mpuerror]");//keep this in your code to inform if there is no mpu dected
    delay(100);
  }
}
void mpuLoop(){
  mpu.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
}
/*custom coding end
-------------------------------------------------------------------
*/








void initWiFi(){
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    if(debug){
      Serial.print('.');
    }
    delay(500);
  }
  if(debug){
    Serial.println('!');
  }
}





void setup(){
  if(debug){
    Serial.begin(115200);
  }
  //firstly establish wifi connection
  initWiFi();
  UDP.begin(UDPPort);
  //mpuSetup();
  /*
  pinMode(TrackPadX,INPUT);
  pinMode(TrackPadY,INPUT);
  pinMode(TrackPadBtn,INPUT_PULLUP);
  pinMode(JoyX,INPUT);
  pinMode(JoyY,INPUT);
  pinMode(JoyBtn,INPUT_PULLUP);
  pinMode(FingerMiddleBtn,INPUT_PULLUP);
  pinMode(FingerRingBtn,INPUT_PULLUP);
  pinMode(FingerPinkyBtn,INPUT_PULLUP);
  pinMode(Bat,INPUT);
  pinMode(Trigger,INPUT);
  pinMode(BBtn,INPUT_PULLUP);
  pinMode(ABtn,INPUT_PULLUP);
  pinMode(SystemBtn,INPUT_PULLUP);
  */
  if(debug){
    Serial.println(WiFi.localIP());
    Serial.println("Looping");
  }
}


void loop(){
  unsigned long currentMillis = millis();
  if ((WiFi.status() != WL_CONNECTED) && (currentMillis - previousMillisWiFi >=intervalWiFi)) {
    WiFi.disconnect();
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
      if(debug){
        Serial.print('.');
      }
      delay(200);
    }
    if(debug){
      Serial.println('.');
    }
    previousMillisWiFi = currentMillis;
  }
  if(!UDPConnected||HIGH){
    int packetSize = UDP.parsePacket();
    if (packetSize)
    {
      int len = UDP.read(incomingPacket, 255);
      if (len > 0)
      {
        incomingPacket[len] = 0;
      }
      if(debug&&!UDPConnected){
        Serial.print("detected computer on ");
        Serial.println(UDP.remoteIP());
      }
      if(String(incomingPacket) == "VR"){
        UDPConnected = HIGH;
        serverIP=UDP.remoteIP();
        serverPort=String(UDP.remotePort()).toInt();
        UDPSend("[device]"+device);
      }
    }
  }
  //mpuLoop();
  //UDPSend("[mpu]"+String(ax)+","+String(ay)+","+String(az)+","+String(gx)+","+String(gy)+","+String(gz));
  if(currentMillis - previousMillisRssi>=intervalRssi){
    UDPSend("[Rssi]"+String(WiFi.RSSI()));
    previousMillisRssi = currentMillis;
  }
  //UDPSend("[digital]"+String(digitalRead(TrackPadBtn))+String(digitalRead(JoyBtn))+String(digitalRead(FingerMiddleBtn))+String(digitalRead(FingerRingBtn))+String(digitalRead(FingerPinkyBtn))+String(digitalRead(BBtn))+String(digitalRead(ABtn))+String(digitalRead(SystemBtn)));
  //UDPSend("[analog]"+String(analogRead(TrackPadX))+","+String(analogRead(TrackPadY))+","+String(analogRead(JoyX))+","+String(analogRead(JoyY))+","+String(analogRead(Bat))+","+String(analogRead(Trigger)));
  delay(100);
}
