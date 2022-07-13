progress:








editing (don't edit the directory/file while others doing):
  Maverick:
    /controller/code/controller_left









things to do:
1. /.../controller_left.ino
  add sendBadMpuConnection()

for desktop server:
  scan 192.168.1.x with https://stackoverflow.com/questions/13492134/find-all-ip-address-in-a-network
  then send packet with each one.




underdeveloped:
1. TinyML for mpu precision
2. ForceFeedBack








controller data order:
  digital:
    TrackPadBtn
    JoyBtn
    FingerMiddleBtn
    FingerRingBtn
    FingerPinkyBtn
    BBtn
    ABtn
    SystemBtn
  analog:
    TrackPadX
    TrackPadY
    JoyX
    JoyY
    Bat
    Trigger
