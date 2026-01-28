# Actividad3MultijugadorPong

## 1. Visión general

Este proyecto consiste en un videojuego de Pong 3D multijugador en tiempo real, con soporte para partidas online utilizando un modelo server-authoritative. El objetivo es reproducir las mecánicas esenciales de Pong en un entorno 3D multijugador.

El juego está diseñado para partidas uno contra uno, con posibilidad de ampliación futura a dobles.

## 2. Mecánicas de juego
2.1 Estados del partido

El flujo del partido se organiza mediante una máquina de estados claramente definida:

WaitingForAllPlayersConnected: los jugadores se conectan a la partida.

WaitingForAllPlayersReady: los jugadores confirman que están listos.

PlayingServe: preparación y ejecución del saque.

PlayingRally: intercambio activo de golpes.

Finished: finalización del partido.

Estos estados gobiernan el comportamiento de jugadores, pelota y cámaras.


## 2.2 Movimiento del jugador

El jugador se desplaza libremente por su mitad de pista durante el rally.

El movimiento está orientado a la cámara activa, garantizando controles intuitivos independientemente del lado de la pista.

Durante el saque, el movimiento está bloqueado para evitar comportamientos inconsistentes.

El movimiento se simula en el servidor y se replica a los clientes.


## 2.3 Golpeo de la pelota

El jugador puede golpear la pelota cuando se encuentra dentro de un rango determinado.

En estado de saque, solo el jugador servidor puede golpear.

El cliente solicita el golpe, pero el servidor es el único que:

* valida la acción

* aplica la fuerza

* actualiza la física real de la pelota

Esto evita trampas y mantiene coherencia entre clientes.


## 2.4 Saque

El saque se produce desde el estado PlayingServe.

El servidor decide quién saca.

La pelota se reposiciona automáticamente antes del saque.

Tras un saque válido, el estado cambia a PlayingRally.


## 2.5 Sistema de puntos

El punto se decide exclusivamente en el servidor.

La lógica de resolución de puntos depende de la posición de la pelota.

Al resolverse un punto:

* se asigna el punto al jugador correspondiente

* se reinicia el estado a PlayingServe

 * se prepara el siguiente saque


## 3. Dinámicas de juego

Las dinámicas emergentes principales son:

* Posicionamiento: anticiparse a la trayectoria de la pelota.

* Control espacial: dominar la mitad de pista propia.

* Ritmo de juego: alternancia entre preparación (saque) y acción (rally).

* Lectura del rival: elegir zonas de golpeo según la posición del oponente.

El diseño busca un equilibrio entre simulación y accesibilidad.


## 4. Arquitectura del sistema
### 4.1 Enfoque general

Se utiliza una arquitectura server-authoritative, donde:

* El servidor es la única fuente de verdad.

* Los clientes envían intenciones (input, solicitudes).

* El servidor valida, simula y sincroniza el estado.

Este enfoque es fundamental para un juego competitivo.

## 4.2 Componentes principales
PlayerControllerNB

* Gestiona el input local.

* Envía movimiento y solicitudes de golpe al servidor.

* Aplica predicción local para suavizar la experiencia.

BallControllerNB

* Simula la física real de la pelota en el servidor.

* Detecta botes, colisiones y condiciones de punto.

MatchManagerSO

* Gestiona reglas del partido.

* Controla el estado del juego.

* Decide saque, puntos y transiciones.

GameManagerNB

* Propaga cambios de estado globales.

* Coordina inicio y final de partidas.

CameraManager

* Gestiona cámaras virtuales según estado y jugador local.

* Garantiza la correcta orientación del input.

## 4.3 Sincronización en red

NetworkVariables para estados persistentes (jugador, listo, visual).

ServerRPCs para acciones críticas:

* movimiento

* golpeo

* confirmación de listo

ClientRPCs / eventos para notificaciones visuales.

## 5. Protocolos de red

El sistema de red se basa en:

* Comunicación cliente → servidor mediante RPCs confiables.

* Replicación automática de estado mediante NetworkVariable.

* Sincronización de transformaciones con NetworkTransform y AnticipatedNetworkTransform.

Se evita cualquier lógica crítica en cliente.

## 6. Motor y tecnologías
### 6.1 Motor

Unity 6

Motivos:

* Motor maduro para multijugador.

* Excelente soporte para físicas y animación.

* Integración directa con Cinemachine.

### 6.2 Librerías y herramientas

* Netcode for GameObjects (NGO)

* Gestión de red y sincronización.

* Cinemachine 3

* Cámaras virtuales y blends.

* CharacterController

* Movimiento controlado y predecible.

* Anticipated Network Transform

* Reducción de latencia percibida.


## 7. Justificación de decisiones técnicas

Server-authoritative: evita trampas y desincronizaciones.

Separación clara de responsabilidades: facilita mantenimiento y escalado.

Cinemachine para orientación de input: experiencia consistente para cada jugador.

Estados de juego explícitos: reduce errores lógicos y condiciones inválidas.


## 8. Escalabilidad futura

El diseño permite extender fácilmente:

Partidas de dobles.

Ranking online.

Repeticiones.

Spectator mode.

Matchmaking automático.

## 9. Conclusión

Este proyecto prioriza robustez, claridad arquitectónica y juego justo, estableciendo una base sólida para un videojuego multijugador competitivo de pádel, preparado tanto para entorno académico como para evolución hacia un producto comercial.