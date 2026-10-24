# Monolegal — recordatorios de facturas

Aplicación web hecha en Visual Studio con ASP.NET Core MVC y C#. Se conecta a MongoDB, muestra las facturas de tres clientes y, según el estado, envía un correo y después actualiza ese estado.

## Qué hace el programa

1. Lee las facturas guardadas en MongoDB.
2. Revisa el campo `estado`.
3. Solo actúa si el estado es `primerrecordatorio` o `segundorecordatorio`.
4. Envía un correo al cliente:
   - Si el estado es `primerrecordatorio`, informa que la factura ha pasado a segundo recordatorio.
   - Si el estado es `segundorecordatorio`, informa que la factura va a ser desactivada.
5. Después de enviar el correo, actualiza la base de datos:
   - `primerrecordatorio` pasa a `segundorecordatorio`.
   - `segundorecordatorio` pasa a `desactivado`.

Las facturas en otro estado, por ejemplo `pagada`, se ven en pantalla y no se modifican.

## Dónde está cada parte

| Parte | Archivo | Función |
| --- | --- | --- |
| Modelo | `src/Monolegal.Web/Models/Factura.cs` y `Cliente.cs` | Datos de la factura y del cliente |
| Vista | `src/Monolegal.Web/Views/Facturas/Index.cshtml` | Pantalla del resumen |
| Controlador | `src/Monolegal.Web/Controllers/FacturasController.cs` | Recibe los clics y devuelve la pantalla |
| Base de datos | `src/Monolegal.Web/Data/MongoDbContext.cs` | Conexión a MongoDB |
| Regla del estado | `src/Monolegal.Web/Dominio/TransicionEstado.cs` | Decide el correo y el estado nuevo |
| Envío del correo | `src/Monolegal.Web/Servicios/RecordatorioService.cs` | Envía el correo y luego actualiza el estado |
| Datos de prueba | `database/monolegal.js` | Script para crear la base |

La cadena de conexión está en `src/Monolegal.Web/appsettings.json`:

- Base de datos: `mongodb://localhost:27017`, base `monolegal`
- Correo de prueba: `127.0.0.1`, puerto `1025`

## Requisitos en el computador

1. [Visual Studio 2022](https://visualstudio.microsoft.com/) con la carga de trabajo **Desarrollo de ASP.NET y web**. Esa carga incluye .NET 8.
2. [MongoDB Community](https://www.mongodb.com/try/download/community) instalado y en ejecución. También sirve [MongoDB Compass](https://www.mongodb.com/try/download/compass) para ver los datos.
3. Para ver los correos sin usar un correo real, [Mailpit](https://mailpit.axllent.org/). Si el equipo tiene Laragon, Mailpit ya viene incluido.

## Paso a paso para ejecutarlo

### 1. Descargar el proyecto

En la página del repositorio pulse **Code** y copie la dirección. En una terminal:

```powershell
git clone DIRECCION-DEL-REPOSITORIO
cd Prueba-Monolegal
```

También puede pulsar **Code** y luego **Download ZIP**, descomprimir la carpeta y abrirla.

### 2. Encender MongoDB

MongoDB tiene que estar escuchando en `mongodb://localhost:27017` antes de abrir la aplicación.

Si MongoDB se instaló como servicio de Windows, basta con comprobar que el servicio **MongoDB** esté iniciado.

Para comprobar la conexión, abra MongoDB Compass y use:

```text
mongodb://localhost:27017
```

Debe aparecer la conexión. La base `monolegal` se crea al ejecutar la aplicación o al correr el script del paso 3.

### 3. Cargar los datos de demostración

Hay dos formas. Con una sola basta.

**Opción A. Dejar que la aplicación los cree.** Si la base `monolegal` está vacía, al presionar F5 se insertan solos los tres clientes y sus facturas.

**Opción B. Ejecutar el script.** Con MongoDB encendido, en la carpeta del proyecto:

```powershell
mongosh "mongodb://localhost:27017" database/monolegal.js
```

Si no tiene `mongosh` en la terminal, abra Compass, conéctese a `mongodb://localhost:27017`, entre a la pestaña **Mongosh** y pegue el contenido de `database/monolegal.js`.

El script borra las colecciones `clientes`, `facturas` y `notificaciones` y las vuelve a crear con este ejemplo:

| Cliente | Correo | Facturas iniciales |
| --- | --- | --- |
| Ana María López | duvansuspe17@gmail.com | FAC-1001 y FAC-1002 en `primerrecordatorio`. FAC-1003 en `pagada` |
| Carlos Gómez Ruiz | carlos.gomez@cliente.test | FAC-2001 en `segundorecordatorio`. FAC-2002 en `primerrecordatorio`. FAC-2003 en `desactivado` |
| Inversiones Delta S.A.S. | cartera@inversionesdelta.test | FAC-3001 y FAC-3002 en `segundorecordatorio`. FAC-3003 en `primerrecordatorio` |

### 4. Encender el visor de correos

El proyecto, tal como está en `appsettings.json`, envía los correos a Mailpit en este equipo. No salen a Gmail.

Si usa Laragon, inicie Mailpit desde allí. También puede ejecutarlo así, si esa es la ruta de su instalación:

```powershell
C:\laragon\bin\mailpit\1.22.3\mailpit.exe
```

Deje esa ventana abierta. La bandeja queda en [http://localhost:8025](http://localhost:8025).

Para enviar a un correo real, cambie en `src/Monolegal.Web/appsettings.json` la sección `Smtp` con el servidor, el puerto, el usuario y la contraseña de ese correo. No suba contraseñas al repositorio.

### 5. Abrir y ejecutar en Visual Studio

1. Abra el archivo `Monolegal.sln`.
2. Si Visual Studio pide restaurar paquetes NuGet, acepte.
3. En el Explorador de soluciones, clic derecho en **Monolegal.Web**.
4. Elija **Establecer como proyecto de inicio**.
5. En la barra superior, deje el perfil **Monolegal.Web** o **http**.
6. Presione **F5**.

El navegador abre [http://localhost:5080](http://localhost:5080).

Si el puerto 5080 está ocupado, Visual Studio muestra otra dirección en la consola. Use esa.

### 6. Probar el proceso en la pantalla

1. Pulse **Ver sus facturas** en un cliente. La página baja a la tabla de ese cliente.
2. Mire la columna **Estado**.
3. Si la factura está en `primerrecordatorio`, pulse **Enviar correo y pasar a segundo recordatorio**.
4. Si está en `segundorecordatorio`, pulse **Enviar correo y desactivar**.
5. Confirme en la tabla que el estado cambió.
6. Abra [http://localhost:8025](http://localhost:8025) y revise que el correo llegó.
7. En Compass, actualice la colección `facturas` de la base `monolegal` y compruebe el nuevo `estado`.

También puede usar **Procesar cliente** para todas las facturas pendientes de una persona, o **Procesar los 3 clientes** para las de los tres. **Restablecer datos** vuelve a dejar el ejemplo inicial.

## Qué debe quedar encendido al mismo tiempo

1. MongoDB, en el puerto 27017.
2. Mailpit, si quiere ver los correos de prueba, en el puerto 1025. La bandeja es el puerto 8025.
3. La aplicación, con F5. La pantalla es el puerto 5080.

Si Compass dice que no puede conectar, MongoDB está apagado. Enciéndalo y vuelva a intentar.
