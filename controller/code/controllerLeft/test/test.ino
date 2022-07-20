#include <Wire.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

Adafruit_MPU6050 mpu;

float pitch=0;
float z=0;

void setup(){
  Serial.begin(115200);
  while(!mpu.begin()){
    delay(100);
    Serial.println("[mpuerror]");
  }
  mpu.setAccelerometerRange(MPU6050_RANGE_2_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_5_HZ);
  delay(100);
}

void loop(){
  sensors_event_t a,g,temp;
  mpu.getEvent(&a,&g,&temp);
  float ax,ay,az;
  ax=a.acceleration.x;
  ay=a.acceleration.y;
  az=a.acceleration.z;
  pitch=atan2(sqrt(ax*ax+ay*ay),az)*180.0/M_PI;
  z=-(atan2(ay,ax)*180.0/M_PI)+180;




  Serial.print(ax);
  Serial.print("\t");
  Serial.print(ay);
  Serial.print("\t");
  Serial.print(az);
  Serial.print("\t");
  Serial.print(pitch);
  Serial.print("\t");
  Serial.print(z);

  Serial.println("");
  delay(200);




}
