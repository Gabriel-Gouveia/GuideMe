#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/

#define SERVICE_UUID "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"
#define CHARACTERISTIC_UUID_MOTOR "30305726-ec78-11ed-a05b-0242ac120003"

#define LIGAR_MOTOR "1"
#define VIBRACOES_MAXIMA_INICIALIZAR 3
#define MOTOR 23
#define TEMPO_MOTOR 300


bool executandoMotor=false;
char contagemVibracao=0;
bool motorLigado=false;
long tempoMotor=0;
bool vibrarAutomatico=false;
int quantidadeVibracoes=0;


/* BLEServer *pServer = BLEDevice::createServer();
BLEService *pService = pServer->createService(SERVICE_UUID);
BLECharacteristic *pCharacteristic = pService->createCharacteristic(
                                         CHARACTERISTIC_UUID,
                                         BLECharacteristic::PROPERTY_READ |
                                         BLECharacteristic::PROPERTY_WRITE
                                       ); */

BLEServer *pServer;
BLEService *pService;
BLECharacteristic *pCharacteristic;
BLECharacteristic *pCharacteristicMotor;
String frame = "";
String frame_semEspaco = "";
byte readFrame[] = { 0xBB, 0X00, 0X22, 0X00, 0X00, 0X22, 0X7E };
bool FrameEnviado=false;
long timestampEnvi=0;

void HandleMotor();

class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      pServer->startAdvertising(); // restart advertising
    };

    void onDisconnect(BLEServer* pServer) {
      pServer->startAdvertising(); // restart advertising
    }
    
};



void setup() {
  Serial.begin(115200);
  Serial.println("Starting BLE Server!");
  Serial2.begin(115200);

  BLEDevice::init("TCC_Bengala_01");
  //BLEDevice::setEncryptionLevel(ESP_BLE_SEC_ENCRYPT);

  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks()); //set the callback function
  pService = pServer->createService(SERVICE_UUID);
  pCharacteristic = pService->createCharacteristic(
    CHARACTERISTIC_UUID,
    BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_WRITE);

  pCharacteristic->setAccessPermissions(ESP_GATT_PERM_READ_ENCRYPTED | ESP_GATT_PERM_WRITE_ENCRYPTED);
  pCharacteristicMotor = pService->createCharacteristic(
    CHARACTERISTIC_UUID_MOTOR,
    BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_WRITE);

  pCharacteristicMotor->setAccessPermissions(ESP_GATT_PERM_READ_ENCRYPTED | ESP_GATT_PERM_WRITE_ENCRYPTED);
  /* BLEServer *pServer = BLEDevice::createServer();
  BLEService *pService = pServer->createService(SERVICE_UUID);
  BLECharacteristic *pCharacteristic = pService->createCharacteristic(
                                         CHARACTERISTIC_UUID,
                                         BLECharacteristic::PROPERTY_READ |
                                         BLECharacteristic::PROPERTY_WRITE
                                       );*/

  pCharacteristic->setValue("Hello, World!");
  pService->start();
  //BLEAdvertising *pAdvertising = pServer->getAdvertising();
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();

  BLESecurity *pSecurity = new BLESecurity();
  pSecurity->setStaticPIN(123456); 

  //pAdvertising->start();
  Serial.println("Characteristic defined! Now you can read it in the Client!");
  pinMode(13,OUTPUT);
  pinMode(MOTOR,OUTPUT);
  digitalWrite(13,HIGH);
  quantidadeVibracoes=VIBRACOES_MAXIMA_INICIALIZAR;
  vibrarAutomatico=true;
  HandleMotor();
}

void loop() {
   HandleMotor();
   long atual=millis();

   if (Serial2.available() ==0 && (FrameEnviado==false || atual-timestampEnvi>1000 ))
   {
        FrameEnviado=true;
        timestampEnvi=atual;
         //Serial.println("Enviando pedido...");
          for (int n = 0; n < sizeof(readFrame); n++) {
            Serial2.write(readFrame[n]);
            delay(1);
          }
          //Serial.println("Pedido enviado...");
   }

   while (Serial2.available() > 0) {
      char c = Serial2.read();
      int cint = (int)c;
      //Serial.println(String(cint,HEX));
      if (cint == 187)  //0xbb
      {
        frame = String(cint, HEX);
        frame_semEspaco = String(cint, HEX);
      } 
      else if (cint == 126)  //0x7e
      {
        FrameEnviado=false;
        frame += " " + String(cint, HEX);
        frame_semEspaco += String(cint, HEX);
        if(frame[3]=='2')
        {
          Serial.println("Frame recebido da antena: " + frame);
          unsigned long tempo = millis();
          char bufMillis[50];
          ltoa(tempo, bufMillis, 10);  // 10 is the base value not the size - look up ltoa for avr
          frame += "-" + String(bufMillis);
          Serial.println("Caracteristica Armazenada " + frame);
          char buffer[frame.length()];
          frame.toCharArray(buffer, frame.length()+1);
          pCharacteristic->setValue(buffer);
          delay(200);
        }
        
        //ParseFrame(frame_semEspaco);
      } 
      else {
        frame += " " + String(cint, HEX);
        frame_semEspaco += String(cint, HEX);
      }
  } 
}

void HandleMotor()
{
  if(!vibrarAutomatico && !executandoMotor)
  {
      std::string value = pCharacteristicMotor->getValue();
      ParseStringMotor(value);
      if(quantidadeVibracoes>0)
      {
        String executando="Executando";
        char buffer[executando.length()];
          executando.toCharArray(buffer, executando.length());
          pCharacteristicMotor->setValue(buffer);
        vibrarAutomatico=true;
      }
      
  }
  else if(vibrarAutomatico && !executandoMotor)
  {
    Serial.println("Inicializando execucao do motor");
    executandoMotor=true;
    contagemVibracao=0;
    tempoMotor=millis();
    motorLigado=true;
    digitalWrite(MOTOR,HIGH);
    vibrarAutomatico=false;
  }
  else if(executandoMotor)
  {
    if(contagemVibracao<=quantidadeVibracoes+1)
    {
      if((millis()-tempoMotor)>=TEMPO_MOTOR)
      {
        if(motorLigado)
        {
          Serial.println("Motor desligado");
          digitalWrite(MOTOR,LOW);
          motorLigado=false;
        }
        else
        {
          Serial.println("Motor ligado");
          digitalWrite(MOTOR,HIGH);
          motorLigado=true;          
        }
        contagemVibracao++;
        tempoMotor=millis();
      }

    }
    else
    {
      Serial.println("Termino da vibração");
      digitalWrite(MOTOR,LOW);
      executandoMotor=false;
      contagemVibracao=0;
      tempoMotor=0;
      motorLigado=false;
      quantidadeVibracoes=0;
    }

  }

}
void ParseStringMotor(std::string valor)
{
  if(valor[0]=='|')
  {
    Serial.println("Valor encontrado no motor");
    String leitura=valor.c_str();
    Serial.println(leitura);
    String quantidade = leitura.substring(1,leitura.length());
    Serial.println(quantidade);
    quantidadeVibracoes=quantidade.toInt();
    
  }
}
