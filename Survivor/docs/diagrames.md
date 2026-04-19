# Diagrames del Projecte

A continuació es presenten els diagrames requerits per a la documentació del projecte. Utilitzem format *Mermaid* perquè Github els pugui renderitzar automàticament de forma visual.

## 1. Diagrama de Casos d'Ús

Aquest diagrama mostra les accions principals que pot fer el jugador dins del joc.

```mermaid
usecaseDiagram
    actor Jugador

    package "Videojoc Survivor" {
        usecase "Registrar-se / Fer Login" as UC1
        usecase "Crear una Sala" as UC2
        usecase "Unir-se a una Sala" as UC3
        usecase "Moure el personatge" as UC4
        usecase "Atacar" as UC5
        usecase "Guardar Resultats al guanyar" as UC6
    }

    Jugador --> UC1
    Jugador --> UC2
    Jugador --> UC3
    Jugador --> UC4
    Jugador --> UC5
    UC5 --> UC6 : En cas de victòria
```

---

## 2. Diagrama Entitat-Relació (ERD)

Aquest diagrama mostra l'estructura simplificada de la base de dades MongoDB.

```mermaid
erDiagram
    USERS {
        string id PK
        string username
        string password_hash
    }
    
    GAMES {
        string id PK
        string status
        date started_at
    }
    
    SCORES {
        string id PK
        string game_id FK
        string user_id FK
        int survival_time_seconds
        int level_reached
        date created_at
    }

    USERS ||--o{ SCORES : "Aconsegueix"
    GAMES ||--o{ SCORES : "Genera"
```

---

## 3. Diagrama de Seqüència (Sincronització WebSocket)

Aquest diagrama mostra com un jugador s'uneix i comença a interactuar a través de WebSockets, complint amb el requisit de mostrar la comunicació en temps real.

```mermaid
sequenceDiagram
    participant Jugador 1 (Host)
    participant Servidor Node.js
    participant Jugador 2 (Client)

    Jugador 1 (Host)->>Servidor Node.js: HTTP POST /login
    Servidor Node.js-->>Jugador 1 (Host): Token/OK
    
    Jugador 1 (Host)->>Servidor Node.js: WS [CREATE_ROOM]
    Servidor Node.js-->>Jugador 1 (Host): WS [ROOM_CREATED]
    
    Jugador 2 (Client)->>Servidor Node.js: WS [JOIN_ROOM]
    Servidor Node.js-->>Jugador 2 (Client): WS [JOINED_ROOM]
    Servidor Node.js-->>Jugador 1 (Host): WS [PLAYER_JOINED]
    
    Note over Jugador 1 (Host), Jugador 2 (Client): La Partida Comença
    
    Jugador 1 (Host)->>Servidor Node.js: WS [MOVE] (x: 5, y: 0)
    Servidor Node.js-->>Jugador 2 (Client): WS [MOVE] (Host a x:5, y:0)
    
    Jugador 1 (Host)->>Servidor Node.js: WS [ATTACK]
    Servidor Node.js-->>Jugador 2 (Client): WS [ATTACK] (Host ataca)
    
    Note over Jugador 1 (Host), Jugador 2 (Client): El jugador 1 derrota al jugador 2
    
    Jugador 1 (Host)->>Servidor Node.js: WS [GAME_WON]
    Servidor Node.js->>Base de Dades: Repositories (Save Score)
    Servidor Node.js-->>Jugador 1 (Host): (MongoDB Guardat correctament)
```

---

## 4. Arquitectura i Microserveis

Esquema de com està dividit el backend seguint Arquitectura Hexagonal.

```mermaid
graph TD
    Client[Unity WebRequest & WebSockets] --> Controller[Controllers HTTP i SocketServer]
    Controller --> Service[Services Lògica Negoci]
    
    subgraph Lògica Interna
        Service --> UserService
        Service --> GameService
        Service --> ScoreService
    end

    UserService --> Repository[Repositories / Interfícies]
    GameService --> Repository
    ScoreService --> Repository

    Repository --> Mongo[MongoDB Real]
    Repository --> InMemory[InMemory DB per Tests]
```
