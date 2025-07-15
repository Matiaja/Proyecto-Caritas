# Proyecto Final: Sistema de Gestión de Donaciones para Cáritas

## 1\. Introducción

Este proyecto fue desarrollado como trabajo final para la carrera de Ingeniería en Sistemas de Información. Consiste en una aplicación web completa diseñada para optimizar y digitalizar la gestión de donaciones, inventario y solicitudes de la organización no gubernamental **Cáritas**. La plataforma busca centralizar las operaciones, mejorar la trazabilidad de los productos y facilitar la comunicación entre los donantes, los centros de acopio y los voluntarios.

El sistema se compone de una **API RESTful** desarrollada en .NET y una aplicación de cliente (Single Page Application) desarrollada en **Angular**.

-----

## 2\. Características Principales

  * **Gestión de Usuarios y Roles:** Sistema de autenticación y autorización basado en roles (Administrador, Encargado de Centro, Donante, etc.) utilizando JWT.
  * **Control de Stock:** Administración de inventario de productos por centro de acopio, incluyendo altas, bajas y transferencias.
  * **Gestión de Donaciones:** Registro y seguimiento de donaciones de productos realizadas por los donantes.
  * **Gestión de Solicitudes:** Creación y seguimiento de solicitudes de productos por parte de los centros.
  * **Catálogo de Productos:** Administración centralizada de los productos y categorías que maneja la organización.
  * **Notificaciones en Tiempo Real:** Sistema de notificaciones para eventos clave dentro de la aplicación.

-----

## 3\. Arquitectura y Stack Tecnológico

La solución sigue una arquitectura cliente-servidor desacoplada.

  * **Backend (API):**

      * **Framework:** .NET 8 (ASP.NET Core Web API)
      * **Lenguaje:** C\#
      * **Acceso a Datos:** Entity Framework Core 8
      * **Autenticación:** ASP.NET Core Identity con JSON Web Tokens (JWT)
      * **Base de Datos:** MySQL

  * **Frontend (Cliente Web):**

      * **Framework:** Angular
      * **Lenguaje:** TypeScript
      * **Comunicación:** Consumo de la API RESTful a través de servicios HTTP.
      * **Diseño:** Interfaz de usuario reactiva y modular.

-----

## 4\. Guía de Instalación y Puesta en Marcha

Para ejecutar el proyecto en un entorno de desarrollo local, sigue estos pasos:

### 4.1. Prerrequisitos

Asegúrate de tener instalado el siguiente software en tu sistema:

  * [**Git**](https://git-scm.com/): Para clonar el repositorio.
  * [**.NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0): Para compilar y ejecutar el backend.
  * [**Node.js y npm**](https://nodejs.org/): Se recomienda la versión LTS para ejecutar el frontend.
  * [**Angular CLI**](https://angular.io/cli): Interfaz de línea de comandos de Angular. Instálala globalmente con `npm install -g @angular/cli`.
  * **Un IDE o Editor de Código:**
      * [**Visual Studio 2022**](https://visualstudio.microsoft.com/es/): Recomendado para el desarrollo del backend.
      * [**Visual Studio Code**](https://code.visualstudio.com/): Recomendado para el desarrollo del frontend.
  * **Servidor de Base de Datos:**
      * [**MySql**](https://www.mysql.com/): El motor de la base de datos.
      * [**MySQL Workbench**](https://dev.mysql.com/downloads/workbench/): Herramienta visual para administrar la base de datos.

### 4.2. Pasos de Configuración

**1. Clonar el Repositorio**

```bash
git clone URL_DEL_REPOSITORIO_AQUÍ
cd NOMBRE_DE_LA_CARPETA_DEL_PROYECTO
```

**2. Configuración de la Base de Datos (MySQL)**

1.  Abre MySQL Workbench y crea un nuevo *schema* (base de datos).
    ```sql
    CREATE SCHEMA `caritas_db`;
    ```
2.  Ejecuta el script SQL para crear la estructura de tablas de la aplicación. El script se encuentra en: `Backend/Data/Scripts/script.sql`.
      * En MySQL Workbench, selecciona el schema `caritas_db`, ve a `File > Open SQL Script...`, abre el archivo y ejecútalo.

**3. Configuración del Backend (.NET)**

1.  Abre la solución `ProyectoCaritas.sln` (ubicada en la carpeta `Backend/`) con Visual Studio 2022. Los paquetes NuGet deberían restaurarse automáticamente.

2.  **Crear el archivo de configuración:** En la raíz del proyecto de backend, crea un archivo `appsettings.json` y pega el siguiente contenido.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Port=3306;Database=caritas_db;User=root;Password=TU_PASSWORD_DE_MYSQL;"
      },
      "Jwt": {
        "Key": "ESTA_ES_UNA_CLAVE_SECRETA_MUY_LARGA_Y_COMPLEJA_PARA_PROBAR",
        "Issuer": "http://localhost",
        "Audience": "http://localhost"
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*"
    }
    ```

    > **Importante:** Reemplaza `TU_PASSWORD_DE_MYSQL` con la contraseña de tu instancia local de MySQL.

3.  **Crear las tablas de Identity:** El sistema usa ASP.NET Core Identity. Sus tablas deben ser creadas mediante migraciones.

      * Abre la Consola del Administrador de Paquetes (`Tools > NuGet Package Manager > Package Manager Console`).
      * Asegúrate de que el proyecto por defecto sea el del Backend.
      * Ejecuta los siguientes comandos en orden:
        ```powershell
        Add-Migration InitialIdentitySchema
        Update-Database
        ```

4.  Ejecuta el backend desde Visual Studio (presionando F5 o el botón de Play). La API se iniciará y se abrirá una ventana de Swagger. Anota la URL (ej: `https://localhost:7185`).

**4. Configuración del Frontend (Angular)**

1.  Abre una terminal en la carpeta `Frontend/`.

2.  **Crear los archivos de entorno:** Dentro de la carpeta `Frontend/src/`, crea una nueva carpeta `environments`. Dentro de ella, crea el archivo `environment.development.ts` con el siguiente contenido:

    ```typescript
    export const environment = {
      production: false,
      baseUrl: 'https://localhost:PUERTO_DEL_BACKEND/api/'
    };
    ```

    > **Importante:** Reemplaza `PUERTO_DEL_BACKEND` con el puerto real en el que se está ejecutando tu API (ej: `7185`).

3.  Instala las dependencias del proyecto.

    ```bash
    npm install
    ```

4.  Ejecuta la aplicación de Angular.

    ```bash
    ng serve -o
    ```

    Se abrirá automáticamente una pestaña en tu navegador en `http://localhost:4200`.

-----

## 5\. Troubleshooting (Solución de Problemas Comunes)

  * **Error: `npm install` falla por políticas de ejecución en PowerShell.**

      * **Causa:** La política de seguridad de PowerShell impide ejecutar scripts.
      * **Solución:** Abre PowerShell **como Administrador** y ejecuta `Set-ExecutionPolicy RemoteSigned`.

  * **Error: `500 - Table 'caritas_db.aspnetroles' doesn't exist` al registrar un usuario.**

      * **Causa:** Las tablas del sistema ASP.NET Core Identity no fueron creadas.
      * **Solución:** Asegúrate de haber ejecutado los comandos `Add-Migration` y `Update-Database` como se indica en el paso 4.3.3.

  * **Error: `401 Unauthorized` al intentar acceder a los recursos desde Swagger.**

      * **Causa:** Los endpoints están protegidos. Necesitas un token JWT para acceder.
      * **Solución:**
        1.  Usa el endpoint `POST /api/User/register` para crear un usuario.
        2.  Usa `POST /api/User/login` con esas credenciales para obtener un token.
        3.  Copia el token y pégalo en la sección `Authorize` de Swagger para autenticar todas tus futuras peticiones.

  * **Error de compilación del Backend: `Access to the path 'apphost.exe' is denied`**

      * **Causa:** Un antivirus o una configuración de seguridad de Windows está bloqueando el proceso de compilación.
      * **Solución:**
        1.  Reinicia el equipo.
        2.  Añade la carpeta de tu proyecto a las exclusiones de tu antivirus (Windows Defender, Avast, etc.).
        3.  Ejecuta Visual Studio como Administrador.

-----

## 6. Autores

Un proyecto desarrollado por:

**Alejandro Foresi** [`alejandroforesi15@gmail.com`](mailto:alejandroforesi15@gmail.com) | [LinkedIn](https://www.linkedin.com/in/alejandroforesi/) | [GitHub](https://github.com/chipcasla)

**Alvaro Genetti** [`alvarogenetti@gmail.com`](mailto:alvarogenetti@gmail.com) | [LinkedIn](https://www.linkedin.com/in/alvaro-genetti/) | [GitHub](https://github.com/alvarogenetti)

**Matías Petrich** [`matias.petrich@gmail.com`](mailto:matias.petrich@gmail.com) | [LinkedIn](https://www.linkedin.com/in/matias-petrich-995a27272/) | [GitHub](https://github.com/Matiaja)

