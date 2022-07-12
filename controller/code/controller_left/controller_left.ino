/*
this code makes reference to HadesVR-main



*/

#include <Wire.h>
#include <SPI.h>
#include <EEPROM.h>
#include "I2Cdev.h"
#include "MPU6050.h"

//pin configuration, diagram at /controller/frizing/controller_bb.png
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
#define mpu6050Address
