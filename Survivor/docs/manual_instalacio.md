# Manual d'Instal·lació i Configuració

Aquest document explica els passos necessaris per posar en marxa el projecte sencer, tant el backend (servidor Node.js + MongoDB) com el client (Unity).

## 1. Requisits Previs
- **Docker Desktop** instal·lat al teu ordinador (per executar el servidor i la base de dades fàcilment).
- **Unity Hub** i l'Editor d'Unity (versió 2022 o superior recomanada).

## 2. Aixecar el Servidor (Backend)
Tota l'arquitectura del servidor i la base de dades MongoDB està dockeritzada.

1. Obre el terminal de comandes (CMD o PowerShell).
2. Navega fins a la carpeta del servidor on es troba el fitxer `docker-compose.yml`:
   ```bash
   cd ruta/del/teu/projecte/Survivor/server
   ```
3. Executa la següent comanda per construir i aixecar el servidor i la base de dades:
   ```bash
   docker compose up --build
   ```
4. Veuràs al terminal que el servidor s'engega i indica:
   - *Servidor HTTP escoltant a http://localhost:3000*
   - *Servidor WebSocket escoltant a ws://localhost:3000*

*(Si en el futur només vols aixecar-ho sense reconstruir, pots fer servir només `docker compose up`)*.

## 3. Obrir el Joc (Client Unity)
1. Obre Unity Hub.
2. Fes clic a **Add** o **Open** i selecciona la carpeta `Survivor/client/Survivor Proyecte`.
3. Un cop s'obri l'Editor d'Unity, ves a la pestanya de Projecte i obre l'escena inicial (`LoginScene` o la que tinguis configurada com a primera).
4. Prem el botó de **Play** a la part superior de l'editor.
5. Pots compilar el joc sencer anant a `File > Build Settings`, afegint les escenes necessàries i prement `Build` (per exemple per a Windows o WebGL).

## 4. Estructura del Projecte
- `/server`: Conté tota l'aplicació Node.js amb Arquitectura Hexagonal.
- `/client`: Conté l'aplicació feta amb Unity.
- `/docs`: Conté la documentació i diagrames.
