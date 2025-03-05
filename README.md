# ErpMicroservices

## Overview

This project is a microservices-based application built using **.NET 8** with **CQRS** (Command Query Responsibility Segregation) architecture. It consists of two core services:

1. **User Service** - Manages user-related operations.
2. **Employee Service** - Handles employee-related CRUD operations.

Both services communicate with a **SQL Server** database running in a **Docker Compose** setup.

---

## Technologies Used

- **.NET 8**
- **CQRS with MediatR**
- **Entity Framework Core**
- **SQL Server (Dockerized)**
- **Docker & Docker Compose**
- **Keycloak** for authentication & authorization
- **RabbitMQ** (optional, for event-driven architecture)
- **Swagger** for API documentation
- **GRPC APIs**

---

## Getting Started

### Prerequisites

Ensure you have the following installed:

- **Docker & Docker Compose**
- **.NET 8 SDK**
- **Postman or any API client** (optional, for testing)

### Clone the Repository

```sh
git clone https://github.com/murtaza-nazar-kf/ErpMicroservices.git
cd ErpMicroservices
```

### Environment Variables

Create a `.env` file in the root directory with the following:

```ini
# Database connection
USERS_CONNECTION="Server=sqlserver;Database=UserServiceDB;User Id=sa;Password=YourStrong!Passw0rd"
EMPLOYEES_CONNECTION="Server=sqlserver;Database=EmployeeServiceDB;User Id=sa;Password=YourStrong!Passw0rd"
KEYCLOAK_DB_CONNECTION="jdbc:sqlserver://sqlserver:1433;encrypt=true;trustServerCertificate=true;loginTimeout=600;socketTimeout=600;"
DEFAULT_CONNECTION_PASSWORD=YourStrong!Passw0rd

# RabbitMQ credentials
RABBITMQ_DEFAULT_USER="user"
RABBITMQ_DEFAULT_PASS="password"

# RabbitMQ connection
RABBITMQ__HOSTNAME=rabbitmq
RABBITMQ__USERNAME=user
RABBITMQ__PASSWORD=password

# Keycloak credentials & settings
KEYCLOAK_DB_PASSWORD="YourStrong!Passw0rd"
KEYCLOAK_ADMIN_USER="admin"
KEYCLOAK_ADMIN_PASS="admin"
KEYCLOAK_AUTHORITY=http://localhost:8080/realms/microservices-realm or http://auth.m.erp.com/realms/microservices-realm
KEYCLOAK_CLIENTID="dotnet-client"
KEYCLOAK_CLIENTSECRET=your_secret

# Certificate paths and passwords
CERT_PASSWORD="123456789"
CERT_PATH="./certs/certificate.pfx"

# ASP.NET Core settings
ASPNETCORE_ENVIRONMENT="Production"
USER_SERVICE_ASPNETCORE_URLS="http://+:5000;https://+:5001"

```

---

## Running the Application

### Start SQL Server & Keycloak

Run the following command to start the dependencies:

```sh
docker-compose up -d
```

This will start **SQL Server** and **Keycloak** in Docker containers.

### Apply Database Migrations

Run migrations for each service:

```sh
dotnet ef database update --project UserService.Infrastructure
```

```sh
dotnet ef database update --project EmployeeService.Infrastructure
```

### Start the Microservices

Start each service in a separate terminal:

```sh
cd UserService
 dotnet run
```

```sh
cd EmployeeService
 dotnet run
```

### Accessing the Services

- **User Service**: `http://localhost:5000/swagger`
- **Employee Service**: `http://localhost:5002/swagger`
- **Keycloak Admin Console**: `http://localhost:8080/admin`
- **RabbitMQ Management UI**: `http://localhost:15672`

or using domain-based URLs:

- **User Service**: `http://users.m.erp.com/swagger` (HTTPS)
- **Employee Service**: `http://employees.m.erp.com/swagger`
- **Keycloak Admin Console**: `http://auth.m.erp.com/admin`
- **RabbitMQ Management UI**: `http://rabbitmq.m.erp.com`

---

## API Endpoints

### Employee Service (REST & gRPC)

| Method | Endpoint | Description |
|--------|---------|-------------|
| GET | `/api/employees` | Get all employees |
| GET | `/api/employees/{id}` | Get employee by ID |
| POST | `/api/employees` | Create a new employee |
| PUT | `/api/employees/{id}` | Update employee details |
| DELETE | `/api/employees/{id}` | Delete an employee |
| gRPC | `GetAllEmployees` | Get all employees |
| gRPC | `GetEmployeeById` | Get employee by ID |
| gRPC | `CreateEmployee` | Create a new employee |
| gRPC | `UpdateEmployee` | Update employee details |
| gRPC | `DeleteEmployee` | Delete an employee |

### User Service (REST & gRPC)

| Method | Endpoint | Description |
|--------|---------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create a new user |
| PUT | `/api/users/{id}` | Update user details |
| DELETE | `/api/users/{id}` | Delete a user |
| gRPC | `GetAllUsers` | Get all users |
| gRPC | `GetUserById` | Get user by ID |
| gRPC | `CreateUser` | Create a new user |
| gRPC | `UpdateUser` | Update user details |
| gRPC | `DeleteUser` | Delete a user |

---

## Authentication & Authorization

This project uses **Keycloak** for authentication.

### Steps to Get a Token

1. Open Keycloak Admin Panel (`http://localhost:8080/admin`).
2. Create a realm, client, and users.
3. Use Postman to get an access token:

   ```sh
   POST http://localhost:8080/realms/{your-realm}/protocol/openid-connect/token
   ```

   Send the following in the request body:

   ```sh
   grant_type=password
   client_id={your-client-id}
   username={your-username}
   password={your-password}
   ```

4. Use the returned token in the `Authorization` header for protected endpoints.

---

## Contributing

1. Fork the repository
2. Create a new feature branch (`git checkout -b feature-name`)
3. Commit your changes (`git commit -m "Added new feature"`)
4. Push to the branch (`git push origin feature-name`)
5. Create a Pull Request

---

## License

This project is licensed under the MIT License.

---

## Contact

For any questions, reach out to [mortadanazar@kf.iq](mailto:mortadanazar@kf.iq).
