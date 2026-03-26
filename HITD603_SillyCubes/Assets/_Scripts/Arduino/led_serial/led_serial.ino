int ledPin = 13;

void setup() {
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW); // start OFF
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    
    String message = Serial.readStringUntil('\n');
    message.trim();

    if (message == "BLINK") {
      digitalWrite(ledPin, HIGH);
      delay(200);
      digitalWrite(ledPin, LOW);
    }
  }
}