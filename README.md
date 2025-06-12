download and install nodejs (LTS) to run frontend in VsCode
for mac:
https://nodejs.org/dist/v22.16.0/node-v22.16.0.pkg
for windows:
https://nodejs.org/en

Open vs code , then open the terminal and run
cd Frontend
npm install
if install fails then run:
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
and then again run:
npm install
npm run dev   -->this will give a link which will launch the frontend

then in another terminal run
cd BackendAPI
dotnet build
dotnet run  
