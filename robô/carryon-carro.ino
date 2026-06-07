#include <SPI.h>
#include <MFRC522.h>
#include <IRremote.hpp> 
#include <Stepper.h>

// --- CONFIGURAÇÃO DE PINOS ---
#define RECV_PIN 11
#define LIMIAR 500
#define RST_PIN 48
#define SS_PIN 53
#define SWITCH_PIN 22 
#define TRIG_PIN 41
#define ECHO_PIN 40

// --- PARÂMETROS DO SISTEMA ---
#define DISTANCIA_LIMITE 15 
const int passosPorVolta = 2048;

// --- PARÂMETROS DE RECUPERAÇÃO DE LINHA ---
char ultimoMovimento = 'F'; 
int contadorPerdido = 0;
const int MAX_TENTATIVAS_RECUPERACAO = 150; 

// --- CÓDIGOS DO COMANDO IR (Substituir pelos teus valores) ---
#define BOTAO_MESA1   0x45 
#define BOTAO_MESA2   0x46 
#define BOTAO_BALCAO  0x47 
#define BOTAO_ZERO    0x16 // Substituir pelo código do botão 0

// --- ESTADOS DO SISTEMA ---
enum EstadoRobo { AGUARDAR_COMANDO, IDA_MESA, REGRESSO_BALCAO };
enum Destino { NENHUM, MESA_1, MESA_2, BALCAO };

EstadoRobo estadoAtual = AGUARDAR_COMANDO;
Destino destinoAtual = NENHUM;

// Variáveis de Controlo
int contadorLeituraUltrassom = 0; 

// --- INSTÂNCIAS DOS MÓDULOS ---
Stepper motorEsq(passosPorVolta, 3, 5, 4, 6); 
Stepper motorDir(passosPorVolta, 7, 9, 8, 10);
MFRC522 rfid(SS_PIN, RST_PIN);

// --- UIDs CONFIGURADOS ---
String uidBalcao = "74 38 B7 BB"; 
String uidMesa1  = "65 6D 1E 06"; 
String uidMesa2  = "39 1E C1 A4"; 

void setup() {
  Serial.begin(9600);
  SPI.begin();
  
  rfid.PCD_Init();
  rfid.PCD_SetAntennaGain(rfid.RxGain_max);
  
  IrReceiver.begin(RECV_PIN, ENABLE_LED_FEEDBACK);
  
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);
  pinMode(SWITCH_PIN, INPUT_PULLUP);
  
  motorEsq.setSpeed(15);
  motorDir.setSpeed(15);

  Serial.println("--- SISTEMA CARRY-ON INICIADO ---");
  Serial.println("Estado: AGUARDAR COMANDO");
}

void loop() {
  processarComandoIR();

  // O Microswitch bloqueia o movimento APENAS durante a viagem de ida para a mesa
  // e enquanto não houver peso no chassis
  if (estadoAtual == IDA_MESA && digitalRead(SWITCH_PIN) == HIGH) {
    pararMotores();
    return; 
  }

  // --- MÁQUINA DE ESTADOS ---
  switch (estadoAtual) {
    
    case AGUARDAR_COMANDO:
      pararMotores();
      break;

    case IDA_MESA:
    case REGRESSO_BALCAO:
      executarMovimento();
      break;
  }
}

// --- FUNÇÕES DE CONTROLO PRINCIPAL ---

void processarComandoIR() {
  // A leitura do recetor ocorre em permanência em qualquer estado
  if (IrReceiver.decode()) {
    unsigned long comando = IrReceiver.decodedIRData.command;
    
    // Paragem de Emergência/Forçada - Interrompe o robô em qualquer momento
    if (comando == BOTAO_ZERO) {
      estadoAtual = AGUARDAR_COMANDO;
      destinoAtual = NENHUM;
      pararMotores();
      Serial.println("-> PARAGEM FORCADA ATIVADA (BOTAO 0)");
    }
    // Definição de destino - Apenas válida se estiver no balcão ou sem carga
    else if (estadoAtual == AGUARDAR_COMANDO || (estadoAtual == IDA_MESA && digitalRead(SWITCH_PIN) == HIGH)) {
      if (comando == BOTAO_MESA1) {
        destinoAtual = MESA_1;
        estadoAtual = IDA_MESA;
        Serial.println("-> MODO MESA 1 ATIVADO");
      } 
      else if (comando == BOTAO_MESA2) {
        destinoAtual = MESA_2;
        estadoAtual = IDA_MESA;
        Serial.println("-> MODO MESA 2 ATIVADO");
      }
    }
    
    IrReceiver.resume();
  }
}

void executarMovimento() {
  // 1. Verificação de Segurança Frontal
  contadorLeituraUltrassom++;
  if (contadorLeituraUltrassom >= 10) {
    if (verificarObstaculo()) {
      Serial.println("OBSTACULO DETETADO! Paragem temporaria.");
      pararMotores();
      delay(3000);
      contadorLeituraUltrassom = 0;
      return; 
    }
    contadorLeituraUltrassom = 0;
  }
  
  // 2. Verificação de Localização (RFID)
  verificarDestino();
  
  // Bloqueio
  if (estadoAtual == AGUARDAR_COMANDO) return;

  // 3. Execução de Rota com Memória de Recuperação
  if (verificarPresencaLinha()) {
    contadorPerdido = 0; 
    seguirLinha();
  } else {
    // Tenta regressar à linha baseando-se no último movimento registado
    if (contadorPerdido < MAX_TENTATIVAS_RECUPERACAO) {
      contadorPerdido++;
      if (ultimoMovimento == 'E') esquerda();
      else if (ultimoMovimento == 'D') direita();
      else frente();
    } else {
      pararMotores(); 
    }
  }
}

// --- FUNÇÕES DE SENSORES ---

bool verificarObstaculo() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);
  
  long duracao = pulseIn(ECHO_PIN, HIGH, 3000); 
  if (duracao == 0) return false; 
  
  int distancia = duracao * 0.034 / 2;
  return (distancia > 0 && distancia < DISTANCIA_LIMITE);
}

void verificarDestino() {
  if (!rfid.PICC_IsNewCardPresent() || !rfid.PICC_ReadCardSerial()) return;
  
  String uidLido = "";
  for (byte i = 0; i < rfid.uid.size; i++) {
    uidLido += (rfid.uid.uidByte[i] < 0x10 ? " 0" : " ");
    uidLido += String(rfid.uid.uidByte[i], HEX);
  }
  uidLido.toUpperCase(); uidLido.trim();
  
  // Cenário 1: Chegada à Mesa
  if (estadoAtual == IDA_MESA) {
    if ((uidLido == uidMesa1 && destinoAtual == MESA_1) || 
        (uidLido == uidMesa2 && destinoAtual == MESA_2)) {
      
      pararMotores();
      Serial.println("MESA ALCANCADA. Pode soltar o botao.");
      
      while (digitalRead(SWITCH_PIN) == LOW) {
        delay(50);
      }
      
      Serial.println("BOTAO SOLTO. Contagem de 10 segundos iniciada...");
      delay(10000); 
      
      Serial.println("-> INICIAR REGRESSO AUTONOMO AO BALCAO");
      destinoAtual = BALCAO;
      estadoAtual = REGRESSO_BALCAO;
    }
  } 
  // Cenário 2: Chegada ao Balcão
  else if (estadoAtual == REGRESSO_BALCAO) {
    if (uidLido == uidBalcao) {
      pararMotores();
      Serial.println("BALCAO ALCANCADO. Viagem concluida.");
      destinoAtual = NENHUM;
      estadoAtual = AGUARDAR_COMANDO;
    }
  }
  
  rfid.PICC_HaltA(); 
  rfid.PCD_StopCrypto1();
}

bool verificarPresencaLinha() {
  return (analogRead(A0) < LIMIAR || analogRead(A1) < LIMIAR || 
          analogRead(A2) < LIMIAR || analogRead(A3) < LIMIAR || 
          analogRead(A4) < LIMIAR);
}

// --- FUNÇÕES DE ATUADORES ---

void seguirLinha() {
  int s0 = analogRead(A0);
  int s1 = analogRead(A1);
  int s2 = analogRead(A2); 
  int s3 = analogRead(A3);
  int s4 = analogRead(A4);

  // Regista a última ação para a memória de recuperação
  if (s2 < LIMIAR || (s1 < LIMIAR && s3 < LIMIAR)) { 
    frente(); 
    ultimoMovimento = 'F';
  } 
  else if (s1 < LIMIAR || s0 < LIMIAR) { 
    esquerda(); 
    ultimoMovimento = 'E';
  } 
  else if (s3 < LIMIAR || s4 < LIMIAR) { 
    direita(); 
    ultimoMovimento = 'D';
  }
}

void frente() {
  motorEsq.step(1); 
  motorDir.step(-1); 
}

void esquerda() {
  motorEsq.step(1); 
  motorDir.step(1); 
}

void direita() {
  motorEsq.step(-1);  
  motorDir.step(-1);  
}

void pararMotores() {
  digitalWrite(3, LOW); digitalWrite(4, LOW); digitalWrite(5, LOW); digitalWrite(6, LOW);
  digitalWrite(7, LOW); digitalWrite(8, LOW); digitalWrite(9, LOW); digitalWrite(10, LOW);
}