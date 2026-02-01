# ProductsSales - Sistema de Productos y Ventas

Aplicación de escritorio para la gestión de productos y ventas, con API REST y base de datos SQL Server. Desarrollada en .NET 8 con arquitectura limpia.

## Características

- **Autenticación** con JWT (JSON Web Tokens)
- **Gestión de productos** (CRUD): crear, listar, editar y eliminar productos
- **Registro de ventas** con múltiples ítems por venta
- **Reportes** de ventas con filtros por fecha y paginación
- **Patrón de resiliencia** (Polly): reintentos automáticos y circuit breaker para llamadas HTTP
- **Docker** y Docker Compose para despliegue
- **Interfaz MDI** (Multiple Document Interface) en Windows Forms
- **Pruebas unitarias** con xUnit, Moq y FluentAssertions
- **Infraestructura como Código (IaC)** con Bicep para Azure

## Tecnologías

| Componente   | Tecnología              |
|--------------|-------------------------|
| API          | ASP.NET Core 8, Swagger |
| Cliente      | Windows Forms (.NET 8)  |
| Base de datos| SQL Server + Entity Framework Core |
| Autenticación| JWT, BCrypt             |
| Validación   | FluentValidation        |
| Resiliencia  | Polly (Retry, Circuit Breaker) |
| Pruebas      | xUnit, Moq, FluentAssertions  |
| IaC          | Bicep (Azure)                 |

## Arquitectura

```
ProductsSales/
├── ProductsSales.Domain/         # Entidades y lógica de dominio
├── ProductsSales.Application/    # Servicios, DTOs, validadores e interfaces
├── ProductsSales.Infrastructure/ # DbContext, repositorios, seguridad
├── ProductsSales.Api/            # API REST (controladores, middlewares)
├── ProductsSales.WinForms/       # Cliente de escritorio Windows
├── ProductsSales.Tests/          # Pruebas unitarias
└── infrastructure/bicep/         # IaC para Azure (Bicep)
```

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (o SQL Server LocalDB)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (opcional, para ejecutar con Docker)

## Inicio rápido

### Opción 1: Ejecución local (sin Docker)

1. **Configurar la base de datos**
   - Crear la base de datos `ProductsSalesDb` en SQL Server
   - O usar LocalDB (la cadena de conexión por defecto lo usa)
   - Las migraciones se aplican automáticamente al iniciar la API

2. **Configurar la cadena de conexión**
   - Editar `ProductsSales.Api/appsettings.json` y ajustar `ConnectionStrings:DefaultConnection`

3. **Ejecutar la API**
   ```bash
   cd c:\ProductSales\ProductsSales
   dotnet run --project ProductsSales.Api
   ```
   - La API estará disponible en `http://localhost:5134`
   - Swagger: `http://localhost:5134/swagger`

4. **Ejecutar la aplicación de escritorio**
   ```bash
   cd c:\ProductSales\ProductsSales
   dotnet run --project ProductsSales.WinForms
   ```

### Opción 1b: Conexión a Azure SQL

Si despliegas la base de datos en Azure con Bicep (ver `infrastructure/bicep/README.md`):

1. **Configurar la cadena de conexión** en `ProductsSales.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=sql-productssales-dev.database.windows.net;Database=ProductsSalesDb;User ID=sqladmin;Password=TuPassword;Encrypt=True;TrustServerCertificate=False;"
   }
   ```

2. **Permitir tu IP en el firewall** de Azure SQL:
   ```bash
   az sql server firewall-rule create --resource-group rg-productssales-dev --server sql-productssales-dev --name AllowMyIP --start-ip-address TU_IP --end-ip-address TU_IP
   ```

3. **Ejecutar migraciones**:
   ```bash
   cd c:\ProductSales\ProductsSales
   dotnet ef database update --project ProductsSales.Infrastructure --startup-project ProductsSales.Api
   ```

4. **Ejecutar la API**:
   ```bash
   cd c:\ProductSales\ProductsSales
   dotnet run --project ProductsSales.Api
   ```

### Opción 2: Ejecución con Docker

1. **Configurar variables de entorno**
   ```bash
   cp .env.example .env
   # Editar .env con DB_PASSWORD y JWT_SECRET_KEY
   ```

2. **Levantar los contenedores**
   ```bash
   docker-compose up -d
   ```

3. **Ejecutar la aplicación de escritorio**
   ```bash
   cd c:\ProductSales\ProductsSales
   dotnet run --project ProductsSales.WinForms
   ```
   - La app se conectará a la API en `http://localhost:5134`

## Variables de entorno (Docker)

| Variable       | Descripción                    | Valor por defecto                             |
|----------------|--------------------------------|-----------------------------------------------|
| DB_PASSWORD    | Contraseña de SQL Server (sa)  | `ProductsSales123!`                           |
| JWT_SECRET_KEY | Clave secreta para JWT         | `ProductsSales_SecretKey_Minimum_32_Characters_Long_For_HS256` |

## Usuario por defecto

Al iniciar la API por primera vez se crea un usuario administrador:

| Campo       | Valor     |
|-------------|-----------|
| Usuario     | `aethos`  |
| Contraseña  | `admin123`|

## API - Endpoints principales

| Método | Endpoint               | Descripción          |
|--------|------------------------|----------------------|
| POST   | `/api/auth/login`      | Iniciar sesión       |
| GET    | `/api/products`        | Listar productos     |
| POST   | `/api/products`        | Crear producto       |
| PUT    | `/api/products/{id}`   | Actualizar producto  |
| DELETE | `/api/products/{id}`   | Eliminar producto    |
| GET    | `/api/sales`           | Listar ventas        |
| POST   | `/api/sales`           | Registrar venta      |
| GET    | `/api/sales/report`    | Reporte de ventas    |

Los endpoints protegidos requieren el header `Authorization: Bearer {token}`.

## Pruebas unitarias

Desde la raíz del proyecto:

```bash
cd c:\ProductSales\ProductsSales
dotnet test ProductsSales.Tests
```

Los tests cubren:
- **ProductService**: CRUD de productos
- **SaleService**: creación de ventas, validación de stock
- **AuthService**: login con credenciales válidas/inválidas
- **ProductValidators**: validación de DTOs (CreateProductDto, UpdateProductDto)

## Comandos Docker útiles

```bash
# Levantar servicios
docker-compose up -d

# Ver logs de la API
docker-compose logs -f productssales-api

# Detener servicios
docker-compose down
```

